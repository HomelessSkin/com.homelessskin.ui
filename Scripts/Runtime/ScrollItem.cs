using TMPro;

using UnityEngine;

namespace UI
{
    public abstract class ScrollItem : MonoBehaviour
    {
        [SerializeField] protected TMP_Text Name;

        protected UIManagerBase Manager;

        public virtual void Init(int index, IInitData data, UIManagerBase manager)
        {
            Manager = manager;

            Name.text = data._Name;
        }
    }

    public interface IInitData
    {
        public string _Name { get; set; }
    }
}