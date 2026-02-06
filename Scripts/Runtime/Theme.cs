using System;
using System.Collections.Generic;
using System.IO;

using TMPro;

using UnityEngine;

namespace UI
{
    [Serializable]
    public class Theme : PersonalizedStorage.Container
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

            var color = new Color(manifest.font.color.x / 255f,
                manifest.font.color.y / 255f,
                manifest.font.color.z / 255f,
                manifest.font.color.w / 255f);

            var lang = manifest.languageKey == null || string.IsNullOrEmpty(manifest.languageKey) ? "default" : manifest.languageKey;

            Name = manifest.name;
            LanguageKey = lang;
            FontAsset = font;

            if (manifest.icons != null)
                for (int i = 0; i < manifest.icons.Length; i++)
                {
                    var icon = manifest.icons[i];
                    var sprite = TryLoadSprite(icon);
                    if (sprite)
                        Map[icon.fileName.Replace(".png", "").ToLower()] = new TheIcon.IconData
                        {
                            Sprite = sprite
                        };
                }

            if (manifest.elements != null)
                for (int s = 0; s < manifest.elements.Length; s++)
                {
                    var info = manifest.elements[s];
                    var data = new Drawable.DrawData
                    {
                        Base = TryLoadSprite(info.@base),
                        Mask = TryLoadSprite(info.mask, info.@base),
                        Overlay = TryLoadSprite(info.overlay),

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
            Sprite TryLoadSprite(SpriteData sprite, CustomSprite param = null)
            {
                if (sprite == null)
                    return null;

                if (param == null)
                    param = sprite as CustomSprite;

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
                    else if (text.LoadImage(File.ReadAllBytes(filePath)))
                    {
                        //text.alphaIsTransparency = true;
                        //text.Apply();
                    }
                    else
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

                NormalColor = selectable.normalColor.ToColor(),
                HighlightedColor = selectable.highlightedColor.ToColor(),
                PressedColor = selectable.pressedColor.ToColor(),
                SelectedColor = selectable.selectedColor.ToColor(),
                DisabledColor = selectable.disabledColor.ToColor(),

                HighlightedSprite = TryLoadSprite(selectable.highlightedSprite, @base),
                PressedSprite = TryLoadSprite(selectable.pressedSprite, @base),
                SelectedSprite = TryLoadSprite(selectable.selectedSprite, @base),
                DisabledSprite = TryLoadSprite(selectable.disabledSprite, @base),
            };
        }
    }
}