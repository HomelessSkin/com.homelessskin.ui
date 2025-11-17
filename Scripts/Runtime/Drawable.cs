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
        [SerializeField] TMP_Text[] Texts;

        public override string GetKey() => _Type.ToString();
        public void SetValue(Data data)
        {
            if (data == null)
                return;

            if (Base)
            {
                Base.sprite = data.Base;
                Base.type = Image.Type.Sliced;
            }

            if (Mask)
            {
                Mask.sprite = data.Mask;
                Mask.type = Image.Type.Sliced;
            }

            if (Overlay)
            {
                if (data.Overlay)
                {
                    Overlay.enabled = true;
                    Overlay.sprite = data.Overlay;
                    Overlay.type = Image.Type.Sliced;
                }
                else
                    Overlay.enabled = false;
            }

            if (Texts != null &&
                data._Text != null)
                for (int t = 0; t < Texts.Length; t++)
                {
                    var text = Texts[t];

                    text.font = data._Text.Font;

                    if (data._Text.FontSize != 0)
                        text.fontSize = data._Text.FontSize;
                    if (data._Text.CharacterSpacing != 0)
                        text.characterSpacing = data._Text.CharacterSpacing;
                    if (data._Text.WordSpacing != 0)
                        text.wordSpacing = data._Text.WordSpacing;

                    text.rectTransform.anchoredPosition3D += data._Text.Offset;
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