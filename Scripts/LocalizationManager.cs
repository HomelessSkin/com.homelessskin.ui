using System.Collections.Generic;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;

using System.IO;
#endif

using UnityEngine;

namespace Core.UI
{
    public class LocalizationManager : MonoBehaviour
    {
        [Header("Game Objects")]
        [SerializeField] MessagePanel Messenger;
        [SerializeField] TutorialPanel Tutorial;
        [SerializeField] ConfirmationPanel Confirmation;
        [SerializeField] Localizable[] Localizables;

        [Header("Localizations")]
        [SerializeField] TextAsset Default;
        [SerializeField] TextAsset[] Localizations;

        [Header("Styles")]
        [SerializeField] TextStyle[] DefaultStyles;
        [SerializeField] TextStyle[] TextStyles;

        Dictionary<string, string> DefaultDict;
        Dictionary<string, string> Current;

        void Start()
        {
            DefaultDict = Deserialize(Default.text);
        }

        internal void SetLanguage(string langKey)
        {
            var lang = langKey.Split("-")[0].ToUpper();

            Debug.Log($"Setting Language with {lang} key");

            Current = Deserialize(Default.text);
            var langStyles = TextStyles.Where(x => x.LanguageKey == lang).ToArray();

            for (int i = 0; i < Localizations.Length; i++)
                if (Localizations[i].name == lang)
                {
                    Current = Deserialize(Localizations[i].text);

                    break;
                }

            for (int i = 0; i < Localizables.Length; i++)
            {
                if (Current.ContainsKey(Localizables[i].GetKey()))
                    Localizables[i].SetValue(Current[Localizables[i].GetKey()]);
                else
                    Localizables[i].SetValue(DefaultDict[Localizables[i].GetKey()]);

                if (GetStyle(Localizables[i].GetElementKey(), TextStyles, out var style, lang))
                    Localizables[i].SetStyle(style);
                else if (GetStyle(Localizables[i].GetElementKey(), DefaultStyles, out var def))
                    Localizables[i].SetStyle(def);
            }
        }

        internal string GetString(string key)
        {
            if (Current.ContainsKey(key))
                return Current[key];
            else if (DefaultDict.ContainsKey(key))
                return DefaultDict[key];

            return $"No Value for <{key}> key!";
        }

        bool GetStyle(ElementKey element, TextStyle[] styles, out TextStyle style, string langKey = "default")
        {
            for (int i = 0; i < styles.Length; i++)
                if (styles[i].LanguageKey == langKey && styles[i].Element == element)
                {
                    style = styles[i];

                    return true;
                }

            style = null;

            return false;
        }
        string Serialize(Dictionary<string, string> dict)
        {
            var keys = dict.Keys.ToArray();
            var str = "";
            for (int i = 0; i < keys.Length; i++)
                str += $"{keys[i]} ; {dict[keys[i]]}\n";

            return str;
        }
        Dictionary<string, string> Deserialize(string text)
        {
            var dict = new Dictionary<string, string>();
            var array = text.Split("\n");
            for (int i = 0; i < array.Length; i++)
                if (array[i] != "")
                {
                    var str = array[i].Split(" ; ");
                    if (!dict.ContainsKey(str[0]))
                        dict[str[0]] = str[1];
                }

            return dict;
        }

#if UNITY_EDITOR
        internal void Reload()
        {
            var defDict = new Dictionary<string, string>();
            for (int i = 0; i < Localizables.Length; i++)
                defDict[Localizables[i].GetKey()] = Localizables[i].GetText();

            if (Tutorial)
                defDict[Tutorial.GetKey()] = Tutorial.GetDefault();
            if (Messenger)
            {
                var keys = Messenger.GetMessages();
                for (int i = 0; i < keys.Length; i++)
                    defDict[keys[i]] = keys[i];
            }
            if (Confirmation)
            {
                var keys = Confirmation.GetKeys();
                for (int i = 0; i < keys.Length; i++)
                    defDict[keys[i]] = keys[i];
            }

            File.WriteAllText(Application.dataPath + "/Resources/UI/Localizations/_default.json", Serialize(defDict));

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            var defKeys = defDict.Keys.ToArray();
            Localizations = Resources.LoadAll<TextAsset>("UI/Localizations/").Where(x => !x.name.Contains("_default")).ToArray();
            for (int i = 0; i < Localizations.Length; i++)
            {
                var text = Localizations[i].text;
                var dict = Deserialize(text);

                for (int j = 0; j < defKeys.Length; j++)
                    if (!dict.ContainsKey(defKeys[j]))
                        text += $"{defKeys[j]} ; {defDict[defKeys[j]]}\n";

                File.WriteAllText(Application.dataPath + $"/Resources/UI/Localizations/{Localizations[i].name}.json", text);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        void OnValidate()
        {
            Localizables = (Localizable[])GameObject
                .FindObjectsByType(typeof(Localizable), FindObjectsInactive.Include, FindObjectsSortMode.None);
        }
        void Reset()
        {
            Core.Util.Tool.CreateTag("LocalizationManager");
            gameObject.tag = "LocalizationManager";
        }
#endif
    }

    public enum ElementKey : byte
    {
        Null = 0,
        Button = 1,

    }
}