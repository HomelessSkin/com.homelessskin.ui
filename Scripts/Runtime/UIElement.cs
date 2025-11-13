using UnityEngine;

namespace UI
{
    public abstract class UIElement : MonoBehaviour
    {
        [SerializeField] protected ElementType _Type;
        [SerializeField] protected string Key;

        protected UIManagerBase UIManager;

        public ElementType GetElementType() => _Type;
        public virtual string GetKey() => Key;

        protected virtual void Start()
        {
            UIManager = GameObject
                .FindGameObjectWithTag("UIManager")
                .GetComponent<UIManagerBase>();
        }
    }
}