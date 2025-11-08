using UnityEngine;

namespace UI
{
    public abstract class WindowBase
    {
        [SerializeField] protected Transform Panel;

        protected void SetEnabled(bool value) => Panel.gameObject.SetActive(value);
    }
}