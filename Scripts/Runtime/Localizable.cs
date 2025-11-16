using TMPro;

using UnityEngine;

namespace UI
{
    [DisallowMultipleComponent]
    public class Localizable : UIElement
    {
        [SerializeField] TMP_Text Value;

        public string GetValue() => Value.text;
        public void SetValue(string value) => Value.text = value;

#if UNITY_EDITOR
        void OnValidate()
        {
            Value = GetComponent<TMP_Text>();
        }
#endif
    }
}