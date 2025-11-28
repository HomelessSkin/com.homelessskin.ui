using System;
using System.Collections.Generic;
using System.IO;

using TMPro;

using UnityEngine;

namespace UI
{
    [Serializable]
    public class Theme : Storage.Container
    {
        public string LanguageKey;
        public TMP_FontAsset FontAsset;

#if UNITY_EDITOR
        public List<Element.Data> Preview = new List<Element.Data>();
#endif

        public Theme(Manifest_V2 manifest, string path, bool fromResources)
        {
            var font = string.IsNullOrEmpty(manifest.font.assetName) ?
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

            for (int s = 0; s < manifest.elements.Length; s++)
            {
                var info = manifest.elements[s];
                var data = new Drawable.DrawData
                {
                    Base = TryLoadSprite(info.@base, info.@base),
                    Mask = TryLoadSprite(info.mask, info.@base),
                    Overlay = TryLoadSprite(info.overlay, info.overlay),

                    _Selectable = info.selectable == null ? null : LoadSelectable(info.selectable, info.@base),
                    _Text = info.text == null ? null : LoadText(info.text),
                };

                Map[info.key] = data;

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
                    var bundle = AssetBundle.LoadFromFile(filePath.Replace(".manifest", ""));

                    bundle.LoadAsset("tempfontholder");
                    result = bundle.LoadAsset<TMP_FontAsset>(bundle.GetAllAssetNames()[0]);
                    bundle.Unload(false);
                }

                return result ?? TMP_Settings.defaultFontAsset;
            }
            Sprite TryLoadSprite(SpriteData sprite, CustomSprite param)
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
            Drawable.DrawData.Text LoadText(TextData text) => new Drawable.DrawData.Text
            {
                LanguageKey = lang,

                Font = font,
                FontSize = text.fontSize,
                CharacterSpacing = manifest.font.characterSpacing,
                WordSpacing = manifest.font.wordSpacing,

                Color = color,
                Offset = new Vector3(text.xOffset, text.yOffset),
            };
            Drawable.DrawData.Selectable LoadSelectable(SelectableData selectable, CustomSprite @base) => new Drawable.DrawData.Selectable
            {
                Transition = selectable.transition == 0 ? UnityEngine.UI.Selectable.Transition.ColorTint : UnityEngine.UI.Selectable.Transition.SpriteSwap,

                NormalColor = new Vector4(selectable.normalColor.X, selectable.normalColor.Y, selectable.normalColor.Z, selectable.normalColor.W),
                HighlightedColor = new Vector4(selectable.highlightedColor.X, selectable.highlightedColor.Y, selectable.highlightedColor.Z, selectable.highlightedColor.W),
                PressedColor = new Vector4(selectable.pressedColor.X, selectable.pressedColor.Y, selectable.pressedColor.Z, selectable.pressedColor.W),
                SelectedColor = new Vector4(selectable.selectedColor.X, selectable.selectedColor.Y, selectable.selectedColor.Z, selectable.selectedColor.W),
                DisabledColor = new Vector4(selectable.disabledColor.X, selectable.disabledColor.Y, selectable.disabledColor.Z, selectable.disabledColor.W),

                HighlightedSprite = TryLoadSprite(selectable.highlightedSprite, @base),
                PressedSprite = TryLoadSprite(selectable.pressedSprite, @base),
                SelectedSprite = TryLoadSprite(selectable.selectedSprite, @base),
                DisabledSprite = TryLoadSprite(selectable.disabledSprite, @base),
            };
        }
    }
}