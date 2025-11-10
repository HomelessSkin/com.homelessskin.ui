using UnityEngine;

namespace UI
{
    public abstract class UIElement : MonoBehaviour
    {
        [SerializeField] Type _Type;
        [SerializeField] string Key;

        public Type GetElementType() => _Type;
        public string GetKey() => Key;

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