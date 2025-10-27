using TMPro;

using UnityEngine;

namespace Core.UI
{
    public class Localizable : MonoBehaviour
    {
        [SerializeField] ElementKey Element;
        [SerializeField] string Name;
        [SerializeField] TMP_Text Value;

        public void SetValue(string value)
        {
            Value.text = value;
        }
        public void SetStyle(TextStyle style)
        {
            Value.fontSize = style.FontSize;
            Value.characterSpacing = style.CharacterSpacing;
            Value.wordSpacing = style.WordSpacing;
        }

        public ElementKey GetElementKey() => Element;
        public string GetKey() => Name;
        public string GetText() => Value.text;

#if UNITY_EDITOR
        void OnValidate()
        {
            Value = GetComponent<TMP_Text>();
        }
#endif
    }
}