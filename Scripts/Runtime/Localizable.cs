using System;

using TMPro;

using UnityEngine;

namespace UI
{
    [DisallowMultipleComponent]
    public class Localizable : Element
    {
        [SerializeField] TMP_Text Value;

        protected override void Start()
        {
            base.Start();

            SetData(UIManager.GetTranslation(GetKey()));
        }
        public override void SetData(Data data)
        {
            if (data == null)
                return;

            Value.text = (data as LocalData).Text;
        }

#if UNITY_EDITOR
        public string GetValue() => Value.text;

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