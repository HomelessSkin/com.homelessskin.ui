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
        public TMP_FontAsset FontAsset;
        public Dictionary<string, Drawable.Data> Sprites;

#if UNITY_EDITOR
        public List<Drawable.Data> Preview;
#endif

        public Theme(Manifest_V2 manifest, string path, bool fromResources = false)
        {
            var font = manifest.font == null || string.IsNullOrEmpty(manifest.font.assetName) ?
             TMP_Settings.defaultFontAsset :
             Resources.Load<TMP_FontAsset>(path + $"{manifest.font.assetName}");

            Name = manifest.name;
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
                    Base = TryLoadSprite(info.@base),
                    Mask = TryLoadSprite(info.mask),
                    Overlay = TryLoadSprite(info.overlay),

                    _Text = info.text == null ? null : LoadText(info.text),
                };
                data.Name = info.key;

                Sprites[info.key] = data;

#if UNITY_EDITOR
                Preview.Add(data);
#endif
            }

            Sprite TryLoadSprite(Manifest.Sprite sprite)
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

                    text.filterMode = (FilterMode)sprite.filterMode;
                    var result = Sprite
                        .Create(text,
                        new Rect(0f, 0f, text.width, text.height),
                        new Vector2(text.width / 2f, text.height / 2f),
                        (sprite.pixelPerUnit > 0 ? sprite.pixelPerUnit : 1),
                        0,
                        SpriteMeshType.FullRect,
                        new Vector4(sprite.borders.left, sprite.borders.right, sprite.borders.top, sprite.borders.bottom),
                        false);

                    result.name = sprite.fileName;

                    return result;
                }

                return null;
            }
            Drawable.Data.Text LoadText(Manifest.Element.Text text) => new Drawable.Data.Text
            {
                FontSize = text.fontSize,
                Font = font,

                Offset = new Vector3(text.xOffset, text.yOffset),
            };
        }
    }
}