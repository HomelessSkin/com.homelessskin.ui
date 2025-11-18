using System;
using System.Numerics;

using Newtonsoft.Json;

namespace UI
{
    #region MANIFEST
    [Serializable]
    public class Manifest
    {
        public Version v;
        public string V
        {
            get
            {
                if (v == null)
                    v = new Version(0, 0, 1);

                return $"{v.major}.{v.minor}.{v.patch}";
            }
        }
        public string name;
        public string languageKey;
        public Font font;

        [Serializable]
        public class Version
        {
            public int major;
            public int minor;
            public int patch;

            public Version() { }
            public Version(int a, int b, int c)
            {
                major = a;
                minor = b;
                patch = c;
            }
        }

        [Serializable]
        public class Font
        {
            public string assetName;
            public Vector4 color;
        }

        [Serializable]
        public class Sprite
        {
            public string fileName;
            public int pixelPerUnit;
            public int filterMode;
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

        [Serializable]
        public class Element
        {
            public string key;

            public Sprite @base;
            public Sprite mask;
            public Sprite overlay;

            public Text text;

            [Serializable]
            public class Text
            {
                public int fontSize;
                public int characterSpacing;
                public int wordSpacing;

                public int xOffset;
                public int yOffset;
            }
        }

        public static Manifest_V2 CreateNew() => new Manifest_V2
        {
            v = new Version(0, 0, 2),
            name = "NewTheme",
            elements = new Manifest_V2.Element[0],
            font = new Font { }
        };
        public static Manifest_V2 Cast(string serialized)
        {
            var manifest = JsonConvert.DeserializeObject<Manifest>(serialized);

            Manifest_V2 result;
            switch (manifest.V)
            {
                case "0.0.0":
                result = new Manifest_V2(JsonConvert.DeserializeObject<Manifest_V0>(serialized));
                break;
                case "0.0.1":
                case "0.0.2":
                result = JsonConvert.DeserializeObject<Manifest_V2>(serialized);
                break;
                default:
                result = CreateNew();
                break;
            }
            result.name = manifest.name;

            if (result.v == null)
                result.v = new Version(0, 0, 1);

            return result;
        }
    }

    [Serializable]
    public class Manifest_V0 : Manifest
    {
        public int version;
        public Sprite_V0[] sprites;

        [Serializable]
        public class Sprite_V0 : Sprite
        {
            public string key;
        }
    }

    [Serializable]
    public class Manifest_V2 : Manifest
    {
        public Element[] elements;

        public Manifest_V2() { }
        public Manifest_V2(Manifest_V0 m0)
        {
            if (m0.sprites != null)
            {
                elements = new Element[m0.sprites.Length];
                for (int e = 0; e < elements.Length; e++)
                {
                    var sprite = m0.sprites[e];
                    elements[e] = new Element
                    {
                        key = sprite.key,

                        @base = new Sprite
                        {
                            fileName = sprite.fileName,
                            pixelPerUnit = sprite.pixelPerUnit,
                            filterMode = sprite.filterMode,
                            borders = sprite.borders,
                        }
                    };
                }
            }
        }
    }
    #endregion
}