using System;

using Newtonsoft.Json;

namespace UI
{
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

        public static Manifest_V1 CreateNew() => new Manifest_V1
        {
            version = 1,
            name = "NewTheme",
            elements = new Manifest_V1.Element[0]
        };
        public static Manifest_V1 Cast(string serialized)
        {
            var manifest = JsonConvert.DeserializeObject<Manifest>(serialized);

            Manifest_V1 result;
            switch (manifest.version)
            {
                case 0:
                result = new Manifest_V1(JsonConvert.DeserializeObject<Manifest_V0>(serialized));
                break;
                case 1:
                result = JsonConvert.DeserializeObject<Manifest_V1>(serialized);
                break;
                default:
                result = CreateNew();
                break;
            }
            result.name = manifest.name;
            result.version = 1;

            return result;
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
