using UnityEngine;

namespace UI
{
    [CreateAssetMenu(fileName = "Scriptable Translation", menuName = "UI/Scriptable Translation")]
    public class ScriptableTranslation : ScriptableObject
    {
        public Translator.Translation Translation;
    }
}