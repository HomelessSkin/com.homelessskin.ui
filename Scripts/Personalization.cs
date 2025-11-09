using System;
using System.Collections.Generic;

using UnityEngine;

namespace UI
{
    [CreateAssetMenu(fileName = "_Variant", menuName = "UI/Personalization")]
    public class Personalization : ScriptableObject
    {
        public KeySprite[] Elements;

        void OnValidate()
        {
            var types = new List<UIElement.Type>();
            for (int e = 0; e < Elements.Length; e++)
            {
                var element = Elements[e];
                if (element.Key == UIElement.Type.Null)
                    continue;

                if (types.Contains(element.Key))
                    Debug.LogError($"Element type {element.Key} used multiple times in {this.name} personalization variant!\n" +
                        $"You should not do that, cause every UIElement must use only one Sprite!");
                else
                    types.Add(element.Key);
            }
        }

        [Serializable]
        public class KeySprite
        {
            public UIElement.Type Key;
            public Sprite Value;
        }
    }
}
