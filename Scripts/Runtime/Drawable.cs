using System;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    [RequireComponent(typeof(Image)), DisallowMultipleComponent]
    public class Drawable : UIElement
    {
        [SerializeField] bool NonRedrawable;
        [SerializeField] Image Mask;
        [SerializeField] Image Overlay;
        [SerializeField] Selectable Selectable;
        [SerializeField] TMP_Text[] Texts;

        Vector3[] Origins;

        public override string GetKey() => _Type.ToString();
        public bool IsNonRedrawable() => NonRedrawable;
        public void SetValue(Data data)
        {
            if (data == null)
                return;

            InitOrigins();

            if (TryGetComponent<Image>(out var basege))
            {
                basege.sprite = data.Base;
                if (data.Base.border.magnitude > 0.0001f)
                    basege.type = Image.Type.Sliced;
            }

            if (Mask)
            {
                Mask.sprite = data.Mask;
                if (data.Base.border.magnitude > 0.0001f)
                    Mask.type = Image.Type.Sliced;
            }

            if (Overlay)
            {
                if (data.Overlay)
                {
                    Overlay.enabled = true;
                    Overlay.sprite = data.Overlay;
                    if (data.Base.border.magnitude > 0.0001f)
                    {
                        Overlay.type = Image.Type.Sliced;
                        Overlay.fillCenter = false;
                    }
                }
                else
                    Overlay.enabled = false;
            }

            if (Selectable &&
                 data._Selectable != null)
            {
                Selectable.transition = data._Selectable.Transition;

                switch (Selectable.transition)
                {
                    case Selectable.Transition.ColorTint:
                    var colors = Selectable.colors;

                    if (data._Selectable.NormalColor.magnitude >= 0.00001f)
                        colors.normalColor = data._Selectable.NormalColor;
                    if (data._Selectable.HighlightedColor.magnitude >= 0.00001f)
                        colors.highlightedColor = data._Selectable.HighlightedColor;
                    if (data._Selectable.PressedColor.magnitude >= 0.00001f)
                        colors.pressedColor = data._Selectable.PressedColor;
                    if (data._Selectable.SelectedColor.magnitude >= 0.00001f)
                        colors.selectedColor = data._Selectable.SelectedColor;
                    if (data._Selectable.DisabledColor.magnitude >= 0.00001f)
                        colors.disabledColor = data._Selectable.DisabledColor;

                    Selectable.colors = colors;
                    break;

                    case Selectable.Transition.SpriteSwap:
                    var state = Selectable.spriteState;

                    state.highlightedSprite = data._Selectable.HighlightedSprite;
                    state.pressedSprite = data._Selectable.PressedSprite;
                    state.selectedSprite = data._Selectable.SelectedSprite;
                    state.disabledSprite = data._Selectable.DisabledSprite;

                    Selectable.spriteState = state;
                    break;
                }
            }

            if (Texts != null &&
                data._Text != null)
                for (int t = 0; t < Texts.Length; t++)
                {
                    var text = Texts[t];
                    if (!text)
                        continue;

                    text.font = data._Text.Font;
                    if (data._Text.FontSize != 0)
                        text.fontSize = data._Text.FontSize;
                    if (data._Text.CharacterSpacing != 0)
                        text.characterSpacing = data._Text.CharacterSpacing;
                    if (data._Text.WordSpacing != 0)
                        text.wordSpacing = data._Text.WordSpacing;

                    text.color = data._Text.Color;
                    text.rectTransform.localPosition = Origins[t] + data._Text.Offset;
                }
        }

        protected override void Start()
        {
            base.Start();

            InitOrigins();

            if (!NonRedrawable &&
                  UIManager.TryGetData(GetKey(), out var data))
                SetValue(data);
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
        public class Data
        {
            public string Name;

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