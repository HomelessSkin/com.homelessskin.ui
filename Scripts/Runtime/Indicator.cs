using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    public class Indicator : Selectable
    {
        [SerializeField] float WaitingTime;

        float T;

        protected override void Start()
        {
            T = WaitingTime;
        }
        void Update()
        {
            T -= Time.deltaTime;

            if (T <= 0f && currentSelectionState == SelectionState.Selected)
                OnDeselect(null);
        }

        public void Refresh() => OnSelect(null);
        public override void OnSelect(BaseEventData eventData)
        {
            base.OnSelect(eventData);

            T = WaitingTime;
        }
    }
}