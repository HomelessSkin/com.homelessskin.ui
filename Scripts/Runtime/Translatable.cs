using System.Collections.Generic;

using TMPro;

using UnityEngine;

namespace UI
{
    [RequireComponent(typeof(TMP_Text))]
    public class Translatable : MonoBehaviour
    {
        [SerializeField] string Key;
        [SerializeField] string Default;
        [SerializeField] TMP_Text Text;

        public void Translate(Dictionary<string, string> dict)
        {
            if (dict.TryGetValue(Key, out var value) && !string.IsNullOrEmpty(value))
                Text.text = value;
            else
                Text.text = Default;
        }
        public string GetKey() => Key;

#if UNITY_EDITOR
        void OnValidate()
        {
            if (!Text)
                Text = GetComponent<TMP_Text>();

            if (Text)
                Default = Text.text;
        }
#endif
    }
}