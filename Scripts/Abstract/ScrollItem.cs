using Core;

using TMPro;

using UnityEngine;

namespace UI
{
    public abstract class ScrollItem : MonoBehaviour
    {
        [SerializeField] protected TMP_Text Name;

        public virtual void Init(int index, IStorage.Data data)
        {
            Name.text = data.Name;
        }
    }
}