using System;


#if UNITY_STANDALONE || UNITY_EDITOR
using UnityEngine;
#else
using Newtonsoft.Json;
#endif

namespace UI
{
    #region MANIFEST
    [Serializable]
    public class Manifest
    {
        public string V => $"{v.major}.{v.minor}.{v.patch}";
        public string name;
        public string languageKey;
        public VersionData v;
        public FontData font;

        public static Manifest_V2 CreateNew() => new Manifest_V2
        {
            v = new VersionData(0, 0, 2),
            name = "NewTheme",
            elements = new ElementData[0],
            font = new FontData { }
        };
        public static Manifest_V2 Cast(string serialized)
        {
            Manifest manifest = null;
#if UNITY_STANDALONE || UNITY_EDITOR
            manifest = JsonUtility.FromJson<Manifest>(serialized);
#else
            manifest = JsonConvert.DeserializeObject<Manifest>(serialized);
#endif

            Manifest_V2 result = null;
            switch (manifest.V)
            {
                case "0.0.0":
#if UNITY_STANDALONE || UNITY_EDITOR
                result = new Manifest_V2(JsonUtility.FromJson<Manifest_V0>(serialized));
#else
                result = new Manifest_V2(JsonConvert.DeserializeObject<Manifest_V0>(serialized));
#endif
                break;
                case "0.0.1":
                case "0.0.2":
#if UNITY_STANDALONE || UNITY_EDITOR
                result = JsonUtility.FromJson<Manifest_V2>(serialized);
#else
                result = JsonConvert.DeserializeObject<Manifest_V2>(serialized);
#endif
                break;
                default:
                result = CreateNew();
                break;
            }
            result.name = manifest.name;

            return result;
        }
    }

    [Serializable]
    public class Vector4Data
    {
        public float X;
        public float Y;
        public float Z;
        public float W;

        public Vector4Data() { }
        public Vector4Data(float x, float y, float z, float w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        public Vector4Data(System.Numerics.Vector4 color)
        {
            X = color.X;
            Y = color.Y;
            Z = color.Z;
            W = color.W;
        }
    }

    [Serializable]
    public class VersionData
    {
        public int major;
        public int minor;
        public int patch;

        public VersionData() { }
        public VersionData(int a, int b, int c)
        {
            major = a;
            minor = b;
            patch = c;
        }
    }

    [Serializable]
    public class FontData
    {
        public string assetName;
        public Vector4Data color;
    }

    [Serializable]
    public class SpriteData
    {
        public string fileName;
    }

    [Serializable]
    public class CustomSprite : SpriteData
    {
        public int pixelPerUnit;
        public int filterMode;
        public Borders borders;
    }

    [Serializable]
    public class Borders
    {
        public int left;
        public int right;
        public int top;
        public int bottom;
    }

    [Serializable]
    public class SelectableData
    {
        public byte transition;

        public Vector4Data normalColor;
        public Vector4Data highlightedColor;
        public Vector4Data pressedColor;
        public Vector4Data selectedColor;
        public Vector4Data disabledColor;

        public SpriteData highlightedSprite;
        public SpriteData pressedSprite;
        public SpriteData selectedSprite;
        public SpriteData disabledSprite;
    }

    [Serializable]
    public class ElementData
    {
        public string key;

        public CustomSprite @base;
        public SpriteData mask;
        public CustomSprite overlay;

        public SelectableData selectable;
        public TextData text;
    }

    [Serializable]
    public class TextData
    {
        public int fontSize;
        public int characterSpacing;
        public int wordSpacing;

        public int xOffset;
        public int yOffset;
    }

    [Serializable]
    public class Manifest_V0 : Manifest
    {
        public int version;
        public Sprite_V0[] sprites;

        [Serializable]
        public class Sprite_V0 : CustomSprite
        {
            public string key;
        }
    }

    [Serializable]
    public class Manifest_V2 : Manifest
    {
        public ElementData[] elements;

        public Manifest_V2() { }
        public Manifest_V2(Manifest_V0 m0)
        {
            if (m0.sprites != null)
            {
                elements = new ElementData[m0.sprites.Length];
                for (int e = 0; e < elements.Length; e++)
                {
                    var sprite = m0.sprites[e];
                    elements[e] = new ElementData
                    {
                        key = sprite.key,

                        @base = new CustomSprite
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