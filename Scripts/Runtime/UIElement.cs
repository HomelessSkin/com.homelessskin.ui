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
            Button = 1,
            Field_Dark = 2,
            Field_Light = 3,
            Twitch = 4,

        }
    }
}