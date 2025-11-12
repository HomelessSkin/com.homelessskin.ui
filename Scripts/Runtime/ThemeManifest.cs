using System;
using System.Collections.Generic;
using System.IO;

using Newtonsoft.Json;

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

        public static Manifest_V1 GetManifest_WithCast(string serialized)
        {
            var manifest = JsonConvert.DeserializeObject<Manifest>(serialized);

            Manifest_V1 result = null;
            switch (manifest.version)
            {
                case 0:
                result = new Manifest_V1(JsonConvert.DeserializeObject<Manifest_V0>(serialized));
                break;
                case 1:
                result = JsonConvert.DeserializeObject<Manifest_V1>(serialized);
                break;
            }
            result.name = manifest.name;
            result.version = 1;

            return result;
        }

        #region MANIFEST
        [Serializable]
        public class Manifest
        {
            public int version;
            public string name;

            [Serializable]
            public class Borders
            {
                public int left;
                public int right;
                public int top;
                public int bottom;
            }
        }

        [Serializable]
        public class Manifest_V0 : Manifest
        {
            public Sprite[] sprites;

            [Serializable]
            public class Sprite
            {
                public string key;

                public string fileName;
                public int pixelPerUnit;
                public int filterMode;
                public Borders borders;
            }
        }

        [Serializable]
        public class Manifest_V1 : Manifest
        {
            public Element[] elements;

            public Manifest_V1() { }
            public Manifest_V1(Manifest_V0 m0)
            {
                elements = new Element[m0.sprites.Length];
                for (int e = 0; e < elements.Length; e++)
                {
                    var sprite = m0.sprites[e];
                    elements[e] = new Element
                    {
                        key = sprite.key,

                        @base = new Element.Sprite
                        {
                            fileName = sprite.fileName,
                            pixelPerUnit = sprite.pixelPerUnit,
                            filterMode = sprite.filterMode,
                            borders = sprite.borders,
                        }
                    };
                }
            }

            [Serializable]
            public class Element
            {
                public string key;

                public Sprite @base;
                public Sprite mask;
                public Sprite overlay;

                [Serializable]
                public class Sprite
                {
                    public string fileName;
                    public int pixelPerUnit;
                    public int filterMode;
                    public Borders borders;
                }
            }
        }
        #endregion
    }
}