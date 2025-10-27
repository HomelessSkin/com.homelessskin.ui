using UnityEngine;

namespace Core.UI
{
    [CreateAssetMenu(fileName = "_Style", menuName = "UI/TextStyle")]
    public class TextStyle : ScriptableObject
    {
        public ElementKey Element;
        public string LanguageKey;
        public int FontSize;
        public int CharacterSpacing;
        public int WordSpacing;
    }
}