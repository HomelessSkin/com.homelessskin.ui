using TMPro;

using UnityEngine;

namespace UI
{
    public abstract class ScrollItem : MonoBehaviour
    {
        [SerializeField] protected TMP_Text Name;

        protected UIManagerBase Manager;

        public virtual void Init(int index, ScrollBase.Container data, UIManagerBase manager)
        {
            Manager = manager;

            Name.text = data.Name;
        }
    }
}