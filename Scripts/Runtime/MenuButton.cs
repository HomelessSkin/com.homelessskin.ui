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
        [SerializeField] UnityEvent OnClick;

        [Space]
        [SerializeField] UnityEvent OnEnabledTargets;

        [Space]
        [SerializeField] GameObject[] Targets;

        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);

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
                    OnEnabledTargets.Invoke();
            }

            Sys.Add(new SetPointerStateRequest { State = MouseState.UI }, World.DefaultGameObjectInjectionWorld.EntityManager);
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
        public void Press() => OnClick.Invoke();
        public void AddListener(UnityAction action) => OnClick.AddListener(action);
        public void RemoveAllListeners() => OnClick.RemoveAllListeners();
        public void InitAsDropItem(UnityAction<int> call, int index, string name)
        {
            SetLabel(name);

            OnClick.AddListener(() => call.Invoke(index));
        }
    }
}