using System;

using TMPro;

using UnityEngine;

namespace UI
{
    [DisallowMultipleComponent]
    public class Localizable : Element
    {
        [SerializeField] TMP_Text Value;

        public string GetValue() => Value.text;

        public override void SetData(Data data) => Value.text = (data as LocalData).Text;

#if UNITY_EDITOR
        void OnValidate()
        {
            Value = GetComponent<TMP_Text>();
        }
#endif

        [Serializable]
        public class LocalData : Data
        {
            public string Text;
        }
    }
}