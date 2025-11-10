using TMPro;

using UnityEngine;

namespace UI
{
    public class Localizable : UIElement
    {
        [SerializeField] TMP_Text Value;

        public string GetValue() => Value.text;
        public void SetValue(string value) => Value.text = value;
        public void SetStyle(TextStyle style)
        {
            Value.fontSize = style.FontSize;
            Value.characterSpacing = style.CharacterSpacing;
            Value.wordSpacing = style.WordSpacing;
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            Value = GetComponent<TMP_Text>();
        }
#endif
    }
}