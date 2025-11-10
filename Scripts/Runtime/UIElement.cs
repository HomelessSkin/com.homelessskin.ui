using UnityEngine;

namespace UI
{
    public abstract class UIElement : MonoBehaviour
    {
        [SerializeField] protected Type _Type;
        [SerializeField] protected string Key;

        public Type GetElementType() => _Type;
        public virtual string GetKey() => Key;

        public enum Type
        {
            Null = 0,
            Menu_Button = 1,
            Field_Dark = 2,
            Field_Light = 3,
            Chat_Message = 4,
            Drop_Down_Content = 5,
            Drop_Down_Item = 6,
            Input_Area = 7,
            Text = 8,

        }
    }
}