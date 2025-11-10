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
        public Dictionary<string, Sprite> Sprites;

#if UNITY_EDITOR
        public List<Sprite> Preview;
#endif

        public Theme(Manifest manifest, string path, bool fromResources = false)
        {
            Name = manifest.name;
            Sprites = new Dictionary<string, Sprite>();

#if UNITY_EDITOR
            Preview = new List<Sprite>();
#endif

            for (int s = 0; s < manifest.sprites.Length; s++)
            {
                var info = manifest.sprites[s];

                var filePath = path + $"{info.fileName}";

                if (fromResources || File.Exists(filePath))
                {
                    var text = new Texture2D(2, 2);
                    if (fromResources)
                        text = Resources.Load<Texture2D>(filePath);
                    else if (!text.LoadImage(File.ReadAllBytes(filePath)))
                        continue;

                    var sprite = Sprite
                        .Create(text,
                        new Rect(0f, 0f, text.width, text.height),
                        new Vector2(text.width / 2f, text.height / 2f),
                        (info.pixelPerUnit > 0 ? info.pixelPerUnit : 1),
                        0,
                        SpriteMeshType.FullRect,
                        new Vector4(info.borders.left, info.borders.right, info.borders.top, info.borders.bottom));

                    sprite.name = info.fileName;

                    Sprites[info.key] = sprite;

#if UNITY_EDITOR
                    Preview.Add(sprite);
#endif
                }
            }
        }

        #region MANIFEST
        [Serializable]
        public class Manifest
        {
            public string name;
            public Sprite[] sprites;

            [Serializable]
            public class Sprite
            {
                public string key;
                public string fileName;
                public int pixelPerUnit;
                public Borders borders;

                [Serializable]
                public class Borders
                {
                    public int left;
                    public int right;
                    public int top;
                    public int bottom;
                }
            }
        }
        #endregion
    }
}