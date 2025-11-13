using System;
using System.Collections.Generic;
using System.IO;

using UnityEngine;

namespace UI
{
    [Serializable]
    public struct Theme
    {
        public string Name;
        public Dictionary<string, Drawable.Data> Sprites;

#if UNITY_EDITOR
        public List<Drawable.Data> Preview;
#endif

        public Theme(Manifest_V1 manifest, string path, bool fromResources = false)
        {
            Name = manifest.name;
            Sprites = new Dictionary<string, Drawable.Data>();

#if UNITY_EDITOR
            Preview = new List<Drawable.Data>();
#endif

            for (int s = 0; s < manifest.elements.Length; s++)
            {
                var info = manifest.elements[s];

                var filePath = path + $"{info.@base.fileName}";

                if (fromResources || File.Exists(filePath))
                {
                    var text = new Texture2D(2, 2);
                    if (fromResources)
                    {
                        filePath = filePath.Replace(".png", "");
                        text = Resources.Load<Texture2D>(filePath);
                    }
                    else if (!text.LoadImage(File.ReadAllBytes(filePath)))
                        continue;

                    text.filterMode = (FilterMode)info.@base.filterMode;
                    var baseSprite = Sprite
                        .Create(text,
                        new Rect(0f, 0f, text.width, text.height),
                        new Vector2(text.width / 2f, text.height / 2f),
                        (info.@base.pixelPerUnit > 0 ? info.@base.pixelPerUnit : 1),
                        0,
                        SpriteMeshType.FullRect,
                        new Vector4(info.@base.borders.left, info.@base.borders.right, info.@base.borders.top, info.@base.borders.bottom),
                        false);

                    baseSprite.name = info.@base.fileName;

                    var data = new Drawable.Data { Base = baseSprite };
                    Sprites[info.key] = data;

#if UNITY_EDITOR
                    Preview.Add(data);
#endif
                }
            }
        }
    }
}