using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Newtonsoft.Json;

using TMPro;

using Unity.Entities;

#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UI
{
    public abstract class UIManagerBase : MonoBehaviour
    {
        protected EntityManager EntityManager;

        [Space]
        [SerializeField] protected Localizator _Localizator;
        #region LOCALIZATOR
        [Serializable]
        protected class Localizator
        {
            public Localizable[] Localizables;

            [Space]
            public TextAsset DefaultLanguage;
            public TextAsset[] Localizations;

            [Space]
            public TextStyle[] DefaultStyles;
            public TextStyle[] TextStyles;

            public Dictionary<string, string> DefaultDict = new Dictionary<string, string>();
            public Dictionary<string, string> Current = new Dictionary<string, string>();
        }

        public void SetLanguage(string langKey)
        {
            var lang = langKey.Split("-")[0].ToUpper();

            Debug.Log($"Setting Language with {lang} key");

            _Localizator.Current = Deserialize(_Localizator.DefaultLanguage.text);
            var langStyles = _Localizator.TextStyles.Where(x => x.LanguageKey == lang).ToArray();

            for (int i = 0; i < _Localizator.Localizations.Length; i++)
                if (_Localizator.Localizations[i].name == lang)
                {
                    _Localizator.Current = Deserialize(_Localizator.Localizations[i].text);

                    break;
                }

            for (int i = 0; i < _Localizator.Localizables.Length; i++)
            {
                if (_Localizator.Current.TryGetValue(_Localizator.Localizables[i].GetKey(), out var value))
                    _Localizator.Localizables[i].SetValue(value);
                else
                    _Localizator.Localizables[i].SetValue(_Localizator.DefaultDict[_Localizator.Localizables[i].GetKey()]);

                if (GetStyle(_Localizator.Localizables[i].GetElementType(), _Localizator.TextStyles, out var style, lang))
                    _Localizator.Localizables[i].SetStyle(style);
                else if (GetStyle(_Localizator.Localizables[i].GetElementType(), _Localizator.DefaultStyles, out var def))
                    _Localizator.Localizables[i].SetStyle(def);
            }
        }
        public string GetTranslation(string key)
        {
            if (_Localizator.Current.ContainsKey(key))
                return _Localizator.Current[key];
            else if (_Localizator.DefaultDict.ContainsKey(key))
                return _Localizator.DefaultDict[key];

            return $"No Value for <{key}> key!";
        }

        bool GetStyle(UIElement.Type element, TextStyle[] styles, out TextStyle style, string langKey = "default")
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
        public void Reload()
        {
            var defDict = new Dictionary<string, string>();
            for (int i = 0; i < _Localizator.Localizables.Length; i++)
                defDict[_Localizator.Localizables[i].GetKey()] = _Localizator.Localizables[i].GetValue();

            defDict[_Tutorial.Key] = _Tutorial.Value;

            for (int i = 0; i < _Messenger.Messages.Length; i++)
                defDict[_Messenger.Messages[i]] = _Messenger.Messages[i];

            for (int i = 0; i < _Confirm.Keys.Length; i++)
                defDict[_Confirm.Keys[i]] = _Confirm.Keys[i];

            File.WriteAllText(Application.dataPath + "/Resources/UI/Localizations/_default.json", Serialize(defDict));

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            var defKeys = defDict.Keys.ToArray();
            _Localizator.Localizations = Resources.LoadAll<TextAsset>("UI/Localizations/").Where(x => !x.name.Contains("_default")).ToArray();
            for (int i = 0; i < _Localizator.Localizations.Length; i++)
            {
                var text = _Localizator.Localizations[i].text;
                var dict = Deserialize(text);

                for (int j = 0; j < defKeys.Length; j++)
                    if (!dict.ContainsKey(defKeys[j]))
                        text += $"{defKeys[j]} ; {defDict[defKeys[j]]}\n";

                File.WriteAllText(Application.dataPath + $"/Resources/UI/Localizations/{_Localizator.Localizations[i].name}.json", text);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
#endif
        #endregion

        [Space]
        [SerializeField] protected Drawer _Drawer;
        #region DRAWER
        [Serializable]
        protected class Drawer : WindowBase
        {
            public static string ThemePref = "theme";

            public Drawable[] Drawables;
            [Space]
            public Theme Default;
            public string DefaultPath;
            public TextAsset DefaultManifest;
            [Space]
            public Theme Current;
            public List<Theme> Themes = new List<Theme>();
            [Space]
            public MenuScroll ThemeScroll;
        }

        public void OpenThemes()
        {
            _Drawer.ThemeScroll.Head.content = Instantiate(_Drawer.ThemeScroll.ContentPrefab, _Drawer.ThemeScroll.View).transform as RectTransform;

            for (int l = 0; l < _Drawer.Themes.Count; l++)
            {
                var go = Instantiate(_Drawer.ThemeScroll.ItemPrefab, _Drawer.ThemeScroll.Head.content);
                var lp = go.GetComponent<ListTheme>();
                lp.Init(l, _Drawer.Themes[l], this);
            }

            _Drawer.SetEnabled(true);
        }
        public void CloseThemes()
        {
            Destroy(_Drawer.ThemeScroll.Head.content.gameObject);

            _Drawer.SetEnabled(false);
        }
        public void ReloadThemes()
        {
            if (!Directory.Exists(Application.persistentDataPath))
                Directory.CreateDirectory(Application.persistentDataPath);

            var manifests = Directory.GetFiles(Application.persistentDataPath, "manifest.json", SearchOption.AllDirectories);
            for (int m = 0; m < manifests.Length; m++)
            {
                var path = manifests[m];

                var manifest = JsonConvert.DeserializeObject<Theme.Manifest>(File.ReadAllText(path));
                if (manifest.sprites != null)
                    _Drawer.Themes.Add(new Theme(manifest, path.Replace("manifest.json", "")));
            }
        }
        public void SelectTheme(int index) => RedrawTheme(_Drawer.Themes[index]);
        public bool TryGetSprite(string key, out Sprite sprite)
        {
            if (_Drawer.Current.Sprites.TryGetValue(key, out sprite))
                return true;
            else if (_Drawer.Default.Sprites.TryGetValue(key, out sprite))
                return true;

            return false;
        }

        void LoadTheme()
        {
            LoadDefaultTheme();

            var saved = PlayerPrefs.GetString(Drawer.ThemePref);
            switch (saved)
            {
                case null:
                case "":
                case "Default":
                {
                    RedrawTheme(_Drawer.Default);
                }
                break;
                default:
                {
                    var manifests = Directory.GetFiles(Application.persistentDataPath, "manifest.json", SearchOption.AllDirectories);
                    var found = false;
                    for (int m = 0; m < manifests.Length; m++)
                    {
                        var path = manifests[m];

                        var manifest = JsonConvert.DeserializeObject<Theme.Manifest>(File.ReadAllText(path));
                        if (manifest.sprites != null &&
                             manifest.name == saved)
                        {
                            found = true;

                            RedrawTheme(new Theme(manifest, path.Replace("manifest.json", "")));

                            break;
                        }
                    }

                    if (!found)
                        goto case "Default";
                }
                break;
            }
        }
        void LoadDefaultTheme() => _Drawer.Default = new Theme(
                    JsonConvert.DeserializeObject<Theme.Manifest>(_Drawer.DefaultManifest.text),
                    _Drawer.DefaultPath, true);
        void RedrawTheme(Theme theme)
        {
            _Drawer.Current = theme;

            for (int d = 0; d < _Drawer.Drawables.Length; d++)
            {
                var drawable = _Drawer.Drawables[d];
                var key = drawable.GetKey();
                if (_Drawer.Current.Sprites.TryGetValue(key, out var sprite))
                    drawable.SetValue(sprite);
                else
                    drawable.SetValue(_Drawer.Default.Sprites[key]);
            }

            PlayerPrefs.SetString(Drawer.ThemePref, theme.Name);
            PlayerPrefs.Save();
        }
        #endregion

        [Space]
        [SerializeField] protected Messenger _Messenger;
        #region MESSENGER
        [Serializable]
        protected class Messenger : WindowBase
        {
            public TMP_Text MessageText;
            public TMP_Text AdditionText;

            [Space]
            public string[] Messages;

            public Message Current = new Message();
            public Queue<Message> Q = new Queue<Message>();
        }
        protected class Message
        {
            public int Index;
            public float Time;
            public float CallTime;
            public AdditionType Addition;
        }

        public enum AdditionType : byte
        {
            Null = 0,
            AdTimer = 1,

        }

        public void AddMessage(int index, float time = 5f, AdditionType addition = AdditionType.Null)
        {
            if (index >= _Messenger.Messages.Length)
            {
                Debug.Log($"{index} greater then range of Messages array");

                return;
            }

            _Messenger.Q.Enqueue(new Message
            {
                Index = index,
                Time = time,
                Addition = addition
            });
        }

        void RefreshCurrent()
        {
            if (_Messenger.Current.Time == 0f)
            {
                _Messenger.MessageText.text = "";
                _Messenger.SetEnabled(false);
            }
            else
            {
                _Messenger.Current.CallTime = Time.realtimeSinceStartup;

                _Messenger.MessageText.text = GetTranslation(_Messenger.Messages[_Messenger.Current.Index]);
                _Messenger.SetEnabled(true);
            }
        }
        string GetAddition(AdditionType addition)
        {
            //switch (addition)
            //{
            //}

            return "";
        }
        #endregion

        [Space]
        [SerializeField] protected Confirm _Confirm;
        #region CONFIRMATION
        [Serializable]
        protected class Confirm : WindowBase
        {
            public UnityAction CurrentAction;

            public TMP_Text Text;
            public MenuButton ConfirmButton;
            public MenuButton DeclineButton;

            [Space]
            public string[] Keys;
        }

        void InitConfirmation(int key, UnityAction action)
        {
            _Confirm.Text.text = GetTranslation(_Confirm.Keys[key]);
            _Confirm.CurrentAction = action;

            _Confirm.ConfirmButton.AddListener(action);
            _Confirm.ConfirmButton.AddListener(ClearConfirmation);
            _Confirm.DeclineButton.AddListener(ClearConfirmation);

            _Confirm.SetEnabled(true);
        }
        void ClearConfirmation()
        {
            _Confirm.Text.text = "";
            _Confirm.CurrentAction = null;

            _Confirm.ConfirmButton.RemoveAllListeners();
            _Confirm.DeclineButton.RemoveAllListeners();
            _Confirm.SetEnabled(false);
        }
        #endregion

        [Space]
        [SerializeField] protected Tutorial _Tutorial;
        #region TUTORIAL
        [Serializable]
        protected class Tutorial : WindowBase
        {
            public TMP_Text Text;
            public string Key;
            public string Value;
        }

        public void ShowTutorial()
        {
            var variant = GetTranslation(_Tutorial.Key);
            _Tutorial.Text.text = variant;

            _Tutorial.SetEnabled(true);
        }
        public void HideTutorial() => _Tutorial.SetEnabled(false);
        #endregion

        protected virtual void Awake()
        {
#if UNITY_ENTITIES_INSTALLED
            EntityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
#endif

            if (_Localizator != null && _Localizator.DefaultLanguage)
                _Localizator.DefaultDict = Deserialize(_Localizator.DefaultLanguage.text);

            LoadTheme();
        }
        protected virtual void Update()
        {
            if (_Messenger.Current.Time != 0f)
            {
                if (_Messenger.Current.Addition != AdditionType.Null)
                    _Messenger.AdditionText.text = GetAddition(_Messenger.Current.Addition);

                if (_Messenger.Current.Time + _Messenger.Current.CallTime < Time.realtimeSinceStartup)
                {
                    _Messenger.Current.Time = 0f;
                    if (_Messenger.Q.Count > 0)
                        _Messenger.Current = _Messenger.Q.Dequeue();

                    _Messenger.AdditionText.text = "";

                    RefreshCurrent();
                }
            }
            else if (_Messenger.Q.Count > 0)
            {
                _Messenger.Current = _Messenger.Q.Dequeue();

                RefreshCurrent();
            }
        }
        protected virtual void OnDestroy()
        {
            StopAllCoroutines();
        }
#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            _Localizator.Localizables = (Localizable[])GameObject
                .FindObjectsByType(typeof(Localizable), FindObjectsInactive.Include, FindObjectsSortMode.None);

            _Drawer.Drawables = (Drawable[])GameObject
                .FindObjectsByType(typeof(Drawable), FindObjectsInactive.Include, FindObjectsSortMode.None);
        }
        protected virtual void Reset()
        {
            Core.Util.Tool.CreateTag("UIManager");
            gameObject.tag = "UIManager";

            if (!Directory.Exists(Application.dataPath + "/Resources/UI/Localizations/"))
                Directory.CreateDirectory(Application.dataPath + "/Resources/UI/Localizations/");

            if (!File.Exists(Application.dataPath + "/Resources/UI/Localizations/_default.json"))
                File.Create(Application.dataPath + "/Resources/UI/Localizations/_default.json");
        }
#endif

        [Serializable]
        protected class MenuScroll
        {
            public ScrollRect Head;
            public Transform View;
            public GameObject ContentPrefab;
            public GameObject ItemPrefab;
        }
    }
}