using TMPro;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    public class MenuButton : Selectable
    {
        [SerializeField] TMP_Text Label;
        [SerializeField] UnityEvent OnClick;
        [SerializeField] UnityEvent OnEnabledTargets;
        [SerializeField] GameObject[] Targets;

        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);

            OnClick.Invoke();

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
        }
        protected override void OnDestroy()
        {
            OnClick?.RemoveAllListeners();
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