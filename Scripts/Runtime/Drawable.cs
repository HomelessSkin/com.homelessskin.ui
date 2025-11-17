using System;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    [RequireComponent(typeof(Image)), DisallowMultipleComponent]
    public class Drawable : UIElement
    {
        [SerializeField] Image Base;
        [SerializeField] Image Mask;
        [SerializeField] Image Overlay;
        [SerializeField] TMP_Text Text;

        public override string GetKey() => _Type.ToString();
        public void SetValue(Data data)
        {
            if (data == null)
                return;

            if (Base)
                Base.sprite = data.Base;

            if (Mask)
                Mask.sprite = data.Mask;

            if (Overlay)
            {
                if (data.Overlay)
                {
                    Overlay.enabled = true;
                    Overlay.sprite = data.Overlay;
                }
                else
                    Overlay.enabled = false;
            }

            if (Text && data._Text != null)
            {
                Text.font = data._Text.Font;

                if (data._Text.FontSize != 0)
                    Text.fontSize = data._Text.FontSize;
                if (data._Text.CharacterSpacing != 0)
                    Text.characterSpacing = data._Text.CharacterSpacing;
                if (data._Text.WordSpacing != 0)
                    Text.wordSpacing = data._Text.WordSpacing;

                Text.rectTransform.anchoredPosition3D += data._Text.Offset;
            }
        }

        protected override void Start()
        {
            base.Start();

            if (UIManager.TryGetData(GetKey(), out var data))
                SetValue(data);
        }

        #region DATA
        [Serializable]
        public class Data
        {
            public string Name;

            public Sprite Base;
            public Sprite Mask;
            public Sprite Overlay;

            public Text _Text;

            [Serializable]
            public class Text
            {
                public string LanguageKey;

                public int FontSize;
                public int CharacterSpacing;
                public int WordSpacing;
                public TMP_FontAsset Font;

                public Vector3 Offset;
            }
        }
        #endregion 
    }
}