using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Core.Util;

using TMPro;

using Unity.Entities;

#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;
using UnityEngine.Events;

namespace UI
{
    public abstract class UIManagerBase : MonoBehaviour
    {
        protected EntityManager EntityManager;

        [Space]
        [SerializeField] protected Localizator _Localizator;
        #region LOCALIZATOR
        [Serializable]
        protected class Localizator : Storage, IPrefKey
        {
            public string _Key => "localization";

            [Space]
            public TextAsset DefaultLanguage;
            public TextAsset[] Localizations;
        }

        public void SetLanguage(string langKey)
        {
            if (string.IsNullOrEmpty(langKey))
                return;

            Debug.Log($"Setting Language with {langKey} key");

            _Localizator.Current.Store = Deserialize(_Localizator.DefaultLanguage.text);
            for (int i = 0; i < _Localizator.Localizations.Length; i++)
                if (_Localizator.Localizations[i].name == langKey)
                {
                    _Localizator.Current.Store = Deserialize(_Localizator.Localizations[i].text);

                    break;
                }

            for (int i = 0; i < _Localizator.Elements.Length; i++)
            {
                var key = _Localizator.Elements[i].GetKey();
                Localizable.LocalData data;
                if (_Localizator.TryGetCurrent<Localizable.LocalData>(key, out data))
                    _Localizator.Elements[i].SetData(data);
                else if (_Localizator.TryGetDefault<Localizable.LocalData>(key, out data))
                    _Localizator.Elements[i].SetData(data);
            }
        }
        public string GetTranslation(string key)
        {
            Localizable.LocalData data;
            if (_Localizator.TryGetCurrent<Localizable.LocalData>(key, out data))
                return data.Text;
            else if (_Localizator.TryGetDefault<Localizable.LocalData>(key, out data))
                return data.Text;

            return $"No Value for <{key}> key!";
        }

        string Serialize(Dictionary<string, Localizable.LocalData> dict)
        {
            var str = "";
            foreach (var kvp in dict)
                str += $"{kvp.Key} ; {kvp.Value.Text}\n";

            return str;
        }
        Dictionary<string, Element.Data> Deserialize(string text)
        {
            var dict = new Dictionary<string, Element.Data>();
            var array = text.Split("\n");
            for (int i = 0; i < array.Length; i++)
                if (!string.IsNullOrEmpty(array[i]))
                {
                    var str = array[i].Split(" ; ");
                    if (!dict.ContainsKey(str[0]))
                        dict[str[0]] = new Localizable.LocalData { Text = str[1] };
                }

            return dict;
        }

#if UNITY_EDITOR
        public void Reload()
        {
            var defDict = new Dictionary<string, Localizable.LocalData>();
            for (int i = 0; i < _Localizator.Elements.Length; i++)
                defDict[_Localizator.Elements[i].GetKey()] = new Localizable.LocalData { Text = (_Localizator.Elements[i] as Localizable).GetValue() };

            defDict[_Tutorial.Key] = new Localizable.LocalData { Text = _Tutorial.Value };

            for (int i = 0; i < _Messenger.Messages.Length; i++)
                defDict[_Messenger.Messages[i]] = new Localizable.LocalData { Text = _Messenger.Messages[i] };

            for (int i = 0; i < _Confirm.Keys.Length; i++)
                defDict[_Confirm.Keys[i]] = new Localizable.LocalData { Text = _Confirm.Keys[i] };

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
        protected class Drawer : ScrollBase, IPrefKey
        {
            public string _Key => "theme";
            public Theme _Default => Default as Theme;
            public Theme _Current => Current as Theme;

            [Space]
            public string DefaultPath;
            public TextAsset DefaultManifest;
            [Space]
            public string ResourcesPath;
        }

        public void OpenThemes() => _Drawer.Open<ListTheme>(this);
        public void CloseThemes() => _Drawer.Close();
        public void ReloadThemes()
        {
            _Drawer.AllData.Clear();

            var resManifests = Resources.LoadAll<TextAsset>(_Drawer.ResourcesPath);
            for (int m = 0; m < resManifests.Length; m++)
            {
                var file = resManifests[m];

                var manifest = Manifest.Cast(file.text);
                if (manifest.elements != null)
                    _Drawer.AllData.Add(new Theme(manifest, $"{_Drawer.ResourcesPath}{manifest.name}/", true));
            }

            if (!Directory.Exists(Application.persistentDataPath))
                Directory.CreateDirectory(Application.persistentDataPath);
            else
            {
                var buildManifests = Directory.GetFiles(Application.persistentDataPath, "manifest.json", SearchOption.AllDirectories);
                for (int m = 0; m < buildManifests.Length; m++)
                {
                    var path = buildManifests[m];

                    var manifest = Manifest.Cast(File.ReadAllText(path));
                    if (manifest.elements != null)
                        _Drawer.AllData.Add(new Theme(manifest, path.Replace("manifest.json", "")));
                }
            }
        }
        public void SelectTheme(int index) => RedrawTheme((Theme)_Drawer.AllData[index]);
        public bool TryGetData(string key, out Element.Data data)
        {
            if (_Drawer.TryGetCurrent(key, out data))
                return true;
            else if (_Drawer.TryGetDefault(key, out data))
                return true;

            return false;
        }

        void LoadTheme()
        {
            LoadDefaultTheme();

            var saved = _Drawer.LoadPrefString();
            switch (saved)
            {
                case null:
                case "":
                case "Default":
                {
                    RedrawTheme(_Drawer._Default);
                }
                break;
                default:
                {
                    var found = false;
                    for (int m = 0; m < _Drawer.AllData.Count; m++)
                    {
                        var theme = _Drawer.AllData[m];
                        if (theme._Name == saved)
                        {
                            found = true;

                            RedrawTheme(theme);

                            break;
                        }
                    }

                    if (!found)
                        goto case "Default";
                }
                break;
            }
        }
        void LoadDefaultTheme() => _Drawer.Default =
            new Theme(Manifest.Cast(_Drawer.DefaultManifest.text), _Drawer.DefaultPath, true);
        protected virtual void RedrawTheme(Storage.Data storage)
        {
            _Drawer.Current = storage;
            _Drawer.SavePrefString(storage._Name);

            for (int d = 0; d < _Drawer.Elements.Length; d++)
            {
                var drawable = _Drawer.Elements[d] as Drawable;
                if (drawable.IsNonRedrawable())
                    continue;

                var key = drawable.GetKey();
                if (_Drawer._Current.Store.TryGetValue(key, out var data))
                    drawable.SetData(data);
                else
                    drawable.SetData(_Drawer._Default.Store[key]);
            }

            SetLanguage(_Drawer._Current.LanguageKey);
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

        public void AddMessage(string key, float time = 5f, AdditionType addition = AdditionType.Null)
        {
            var index = -1;
            for (int i = 0; i < _Messenger.Messages.Length; i++)
                if (_Messenger.Messages[i] == key)
                {
                    index = i;

                    break;
                }

            if (index >= 0)
                AddMessage(index, time, addition);
            else
                Debug.LogWarning($"Message key {key} not found!");
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
                _Localizator.Default.Store = Deserialize(_Localizator.DefaultLanguage.text);

            ReloadThemes();
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
            _Localizator.Elements = (Element[])GameObject
                .FindObjectsByType(typeof(Localizable), FindObjectsInactive.Include, FindObjectsSortMode.None);

            _Drawer.Elements = (Element[])GameObject
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
    }
}