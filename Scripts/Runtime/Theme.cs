using System;
using System.Collections.Generic;
using System.IO;

using TMPro;

using UnityEngine;

namespace UI
{
    [Serializable]
    public struct Theme
    {
        public string Name;
        public string LanguageKey;
        public TMP_FontAsset FontAsset;
        public Dictionary<string, Drawable.Data> Sprites;

#if UNITY_EDITOR
        public List<Drawable.Data> Preview;
#endif

        public Theme(Manifest_V2 manifest, string path, bool fromResources = false)
        {
            var font = manifest.font == null || string.IsNullOrEmpty(manifest.font.assetName) ?
             TMP_Settings.defaultFontAsset :
             LoadFont();

            var color = new Color(manifest.font.color.X / 255f,
                manifest.font.color.Y / 255f,
                manifest.font.color.Z / 255f,
                manifest.font.color.W / 255f);

            var lang = manifest.languageKey == null || string.IsNullOrEmpty(manifest.languageKey) ? "default" : manifest.languageKey;

            Name = manifest.name;
            LanguageKey = lang;
            FontAsset = font;
            Sprites = new Dictionary<string, Drawable.Data>();

#if UNITY_EDITOR
            Preview = new List<Drawable.Data>();
#endif

            for (int s = 0; s < manifest.elements.Length; s++)
            {
                var info = manifest.elements[s];
                var data = new Drawable.Data
                {
                    Base = TryLoadSprite(info.@base, info.@base),
                    Mask = TryLoadSprite(info.mask, info.@base),

                    Overlay = TryLoadSprite(info.overlay, info.overlay),

                    _Selectable = new Drawable.Data.Selectable
                    {
                        Transition = info.selectable.transition == 0 ? UnityEngine.UI.Selectable.Transition.ColorTint : UnityEngine.UI.Selectable.Transition.SpriteSwap,

                        NormalColor = new Vector4(info.selectable.normalColor.X, info.selectable.normalColor.Y, info.selectable.normalColor.Z, info.selectable.normalColor.W),
                        HighlightedColor = new Vector4(info.selectable.highlightedColor.X, info.selectable.highlightedColor.Y, info.selectable.highlightedColor.Z, info.selectable.highlightedColor.W),
                        PressedColor = new Vector4(info.selectable.pressedColor.X, info.selectable.pressedColor.Y, info.selectable.pressedColor.Z, info.selectable.pressedColor.W),
                        SelectedColor = new Vector4(info.selectable.selectedColor.X, info.selectable.selectedColor.Y, info.selectable.selectedColor.Z, info.selectable.selectedColor.W),
                        DisabledColor = new Vector4(info.selectable.disabledColor.X, info.selectable.disabledColor.Y, info.selectable.disabledColor.Z, info.selectable.disabledColor.W),

                        HighlightedSprite = TryLoadSprite(info.selectable.highlightedSprite, info.@base),
                        PressedSprite = TryLoadSprite(info.selectable.pressedSprite, info.@base),
                        SelectedSprite = TryLoadSprite(info.selectable.selectedSprite, info.@base),
                        DisabledSprite = TryLoadSprite(info.selectable.disabledSprite, info.@base),
                    },
                    _Text = info.text == null ? null : LoadText(info.text),
                };
                data.Name = info.key;

                Sprites[info.key] = data;

#if UNITY_EDITOR
                Preview.Add(data);
#endif
            }

            TMP_FontAsset LoadFont()
            {
                var result = TMP_Settings.defaultFontAsset;
                var filePath = path + manifest.font.assetName;

                if (fromResources)
                    result = Resources.Load<TMP_FontAsset>(filePath.Replace(".asset", ""));
                else if (File.Exists(filePath))
                {
                    var bundle = AssetBundle.LoadFromFile(filePath);
                    var assets = bundle.GetAllAssetNames();
                    bundle.LoadAsset("tempfontholder");

                    result = bundle.LoadAsset<TMP_FontAsset>(assets[0]);
                }

                return result ?? TMP_Settings.defaultFontAsset;
            }
            Drawable.Data.Text LoadText(Manifest.Element.Text text) => new Drawable.Data.Text
            {
                LanguageKey = lang,

                Font = font,
                FontSize = text.fontSize,
                CharacterSpacing = text.characterSpacing,
                WordSpacing = text.wordSpacing,

                Color = color,
                Offset = new Vector3(text.xOffset, text.yOffset),
            };
            Sprite TryLoadSprite(Manifest.Sprite sprite, Manifest.CustomSprite param)
            {
                if (sprite == null)
                    return null;

                var filePath = path + $"{sprite.fileName}";
                if (fromResources || File.Exists(filePath))
                {
                    var text = new Texture2D(2, 2);
                    if (fromResources)
                    {
                        filePath = filePath.Replace(".png", "");
                        text = Resources.Load<Texture2D>(filePath);

                        if (!text)
                            return null;
                    }
                    else if (!text.LoadImage(File.ReadAllBytes(filePath)))
                        return null;

                    text.filterMode = (FilterMode)param.filterMode;
                    var result = Sprite
                        .Create(text,
                        new Rect(0f, 0f, text.width, text.height),
                        new Vector2(text.width / 2f, text.height / 2f),
                        (param.pixelPerUnit > 0 ? param.pixelPerUnit : 1),
                        0,
                        SpriteMeshType.FullRect,
                        new Vector4(param.borders.left, param.borders.bottom, param.borders.right, param.borders.top),
                        false);

                    result.name = sprite.fileName;

                    return result;
                }

                return null;
            }
        }
    }
}