using UnityEngine;

namespace UI
{
    [CreateAssetMenu(fileName = "_Style", menuName = "UI/TextStyle")]
    public class TextStyle : ScriptableObject
    {
        public ElementType Element;
        public int FontSize;
        public int CharacterSpacing;
        public int WordSpacing;
    }
}