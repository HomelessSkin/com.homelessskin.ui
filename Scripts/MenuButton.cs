using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Core.UI
{
    public class MenuButton : Selectable
    {
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
        public void Press() => OnClick.Invoke();
        public void AddListener(UnityAction action) => OnClick.AddListener(action);
        public void RemoveAllListeners() => OnClick.RemoveAllListeners();
    }
}