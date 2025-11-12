using UnityEngine;

namespace UI
{
    public abstract class UIElement : MonoBehaviour
    {
        [SerializeField] protected Type _Type;
        [SerializeField] protected string Key;

        protected UIManagerBase UIManager;

        public Type GetElementType() => _Type;
        public virtual string GetKey() => Key;

        protected virtual void Start()
        {
            UIManager = GameObject
                .FindGameObjectWithTag("UIManager")
                .GetComponent<UIManagerBase>();
        }

        public enum Type
        {
            Null = 0,
            Menu_Button = 1,
            Big_Panel = 2,
            Mid_Panel = 3,
            Small_Panel = 4,
            Chat_Message = 5,
            Drop_Down_Content = 6,
            Drop_Down_Item = 7,
            Input_Area = 8,
            Text = 9,

        }
    }
}