using System;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    [RequireComponent(typeof(Image)), DisallowMultipleComponent]
    public class Drawable : Element
    {
        [SerializeField] bool NonRedrawable;
        [SerializeField] Image Mask;
        [SerializeField] Image Overlay;
        [SerializeField] Selectable Selectable;
        [SerializeField] TMP_Text[] Texts;

        Vector3[] Origins;

        protected override void Start()
        {
            base.Start();

            InitOrigins();

            if (!NonRedrawable &&
                  UIManager.TryGetData(GetKey(), out var data))
                SetData(data);
        }

        public override string GetKey() => _Type.ToString();
        public bool IsNonRedrawable() => NonRedrawable;

        public override void SetData(Data data)
        {
            if (data == null)
                return;

            var drawData = data as DrawData;

            InitOrigins();

            if (TryGetComponent<Image>(out var basege))
            {
                basege.sprite = drawData.Base;
                if (drawData.Base.border.magnitude > 0.0001f)
                    basege.type = Image.Type.Sliced;
            }

            if (Mask)
            {
                Mask.sprite = drawData.Mask;
                if (drawData.Base.border.magnitude > 0.0001f)
                    Mask.type = Image.Type.Sliced;
            }

            if (Overlay)
            {
                if (drawData.Overlay)
                {
                    Overlay.enabled = true;
                    Overlay.sprite = drawData.Overlay;
                    if (drawData.Base.border.magnitude > 0.0001f)
                    {
                        Overlay.type = Image.Type.Sliced;
                        Overlay.fillCenter = false;
                    }
                }
                else
                    Overlay.enabled = false;
            }

            if (Selectable &&
                 drawData._Selectable != null)
            {
                Selectable.transition = drawData._Selectable.Transition;

                switch (Selectable.transition)
                {
                    case Selectable.Transition.ColorTint:
                    var colors = Selectable.colors;

                    if (drawData._Selectable.NormalColor.magnitude >= 0.00001f)
                        colors.normalColor = drawData._Selectable.NormalColor.ToColor();
                    if (drawData._Selectable.HighlightedColor.magnitude >= 0.00001f)
                        colors.highlightedColor = drawData._Selectable.HighlightedColor.ToColor();
                    if (drawData._Selectable.PressedColor.magnitude >= 0.00001f)
                        colors.pressedColor = drawData._Selectable.PressedColor.ToColor();
                    if (drawData._Selectable.SelectedColor.magnitude >= 0.00001f)
                        colors.selectedColor = drawData._Selectable.SelectedColor.ToColor();
                    if (drawData._Selectable.DisabledColor.magnitude >= 0.00001f)
                        colors.disabledColor = drawData._Selectable.DisabledColor.ToColor();

                    Selectable.colors = colors;
                    break;

                    case Selectable.Transition.SpriteSwap:
                    var state = Selectable.spriteState;

                    state.highlightedSprite = drawData._Selectable.HighlightedSprite;
                    state.pressedSprite = drawData._Selectable.PressedSprite;
                    state.selectedSprite = drawData._Selectable.SelectedSprite;
                    state.disabledSprite = drawData._Selectable.DisabledSprite;

                    Selectable.spriteState = state;
                    break;
                }
            }

            if (Texts != null &&
                 drawData._Text != null)
                for (int t = 0; t < Texts.Length; t++)
                {
                    var text = Texts[t];
                    if (!text)
                        continue;

                    text.font = drawData._Text.Font;
                    if (drawData._Text.FontSize != 0)
                        text.fontSize = drawData._Text.FontSize;
                    if (drawData._Text.CharacterSpacing != 0)
                        text.characterSpacing = drawData._Text.CharacterSpacing;
                    if (drawData._Text.WordSpacing != 0)
                        text.wordSpacing = drawData._Text.WordSpacing;

                    text.color = drawData._Text.Color;
                    text.rectTransform.localPosition = Origins[t] + drawData._Text.Offset;
                }
        }

        void InitOrigins()
        {
            if (Origins != null)
                return;

            Origins = new Vector3[Texts.Length];
            for (int t = 0; t < Texts.Length; t++)
                if (Texts[t])
                    Origins[t] = Texts[t].rectTransform.localPosition;
        }

        #region DATA
        [Serializable]
        public class DrawData : Data
        {
            public Sprite Base;
            public Sprite Mask;
            public Sprite Overlay;

            public Selectable _Selectable;
            public Text _Text;

            [Serializable]
            public class Selectable
            {
                public UnityEngine.UI.Selectable.Transition Transition;

                public Vector4 NormalColor;
                public Vector4 HighlightedColor;
                public Vector4 PressedColor;
                public Vector4 SelectedColor;
                public Vector4 DisabledColor;

                public Sprite HighlightedSprite;
                public Sprite PressedSprite;
                public Sprite SelectedSprite;
                public Sprite DisabledSprite;
            }

            [Serializable]
            public class Text
            {
                public string LanguageKey;

                public int FontSize;
                public int CharacterSpacing;
                public int WordSpacing;
                public TMP_FontAsset Font;

                public Color Color;
                public Vector3 Offset;
            }
        }
        #endregion 
    }
}