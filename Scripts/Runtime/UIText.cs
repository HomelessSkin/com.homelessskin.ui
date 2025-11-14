using System;

using TMPro;

using UnityEngine;

namespace UI
{
    [DisallowMultipleComponent]
    public class UIText : UIElement
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

        [Serializable]
        public class Data
        {
            public bool Localizable;
            public string Text;
            public Vector3 Offset;
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            Value = GetComponent<TMP_Text>();
        }
#endif
    }
}