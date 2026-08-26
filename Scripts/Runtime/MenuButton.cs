using System;
using System.Collections.Generic;

using Core;

using Input;

using TMPro;

using Unity.Entities;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    public class MenuButton : Selectable
    {
        [Space]
        [SerializeField] Image Image;
        [SerializeField] TMP_Text Label;

        [Space]
        [Header("OnClick Event is obsolete! Use Outer Inputs instead!")]
        [SerializeField] UnityEvent OnClick;

        [Space]
        [SerializeField] UnityEvent OnEnabledTargets;

        [Space]
        [SerializeField] GameObject[] Targets;

        [Space]
        [SerializeField] List<OuterInput> Inputs;

        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);

            Press();
        }
        protected override void OnDestroy()
        {
            OnClick?.RemoveAllListeners();
            OnEnabledTargets?.RemoveAllListeners();
        }

        public void SetImage(Texture2D texture)
        {
            if (Image)
                Image.sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), Vector2.zero);
        }
        public void SetLabel(string name)
        {
            if (Label)
                Label.text = name;
        }
        public void Press()
        {
            if (OnClick != null && OnClick.GetPersistentEventCount() > 0)
                Log.Warning(this, "OnClick Event is obsolete! Use Outer Inputs instead!");

            OnClick?.Invoke();

            if (Targets != null && Targets.Length > 0)
            {
                var invoke = true;
                for (int i = 0; i < Targets.Length; i++)
                    if (!Targets[i].activeSelf)
                    {
                        invoke = false;

                        break;
                    }

                if (invoke)
                    OnEnabledTargets?.Invoke();
            }

            Sys.Add(new SetPointerStateRequest { State = MouseState.UI }, World.DefaultGameObjectInjectionWorld.EntityManager);

            if (Inputs != null)
                for (int i = 0; i < Inputs.Count; i++)
                    if (Inputs[i] != null && !string.IsNullOrEmpty(Inputs[i].Title))
                        Sys.Add_M(Inputs[i], World.DefaultGameObjectInjectionWorld.EntityManager);
        }
        public void AddInput(OuterInput input)
        {
            if (Inputs == null)
                Inputs = new List<OuterInput>();

            Inputs.Add(input);
        }
        public void RemoveAllInputs() => Inputs.Clear();
        public void InitAsDropItem(UnityAction<int> call, int index, string name)
        {
            SetLabel(name);

            OnClick.AddListener(() => call.Invoke(index));
        }

        [Obsolete("UnityEvent's inside MenuButton class is obsolete! Use AddInput method instead!")]
        public void AddListener(UnityAction action) => OnClick.AddListener(action);
        [Obsolete("UnityEvent's inside MenuButton class is obsolete! Use RemoveAllInputs method instead!")]
        public void RemoveAllListeners() => OnClick.RemoveAllListeners();
    }
}