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

namespace UI
{
    public abstract class UIManagerBase : MonoBehaviour
    {
        protected EntityManager EntityManager;

        [Space]
        [SerializeField] protected Localizator _Localizator;
        #region LOCALIZATOR
        [Serializable]
        protected class Localizator : ScrollBase
        {
            public override void AddData(string serialized, string path, bool fromResources = false) =>
                AllData.Add(new Localization(JsonConvert.DeserializeObject<Localization.Data>(serialized)));
            public override void SetData(Data data)
            {
                base.SetData(data);

                for (int d = 0; d < Elements.Length; d++)
                {
                    var localizable = Elements[d] as Localizable;
                    if (TryGetValue(localizable.GetKey(), out var elData))
                        localizable.SetData(elData);
                }
            }

            protected override void LoadDefault() =>
                Default = new Localization(Deserialize(Resources.Load<TextAsset>(DefaultPath).text));

            public string Serialize(Localization localization)
            {
                var data = new Localization.Data { name = localization.Name, dictionary = new Localization.Data.KVP[localization.Store.Count] };
                var c = 0;
                foreach (var kvp in localization.Store)
                {
                    data.dictionary[c] = new Localization.Data.KVP { key = kvp.Key, value = (kvp.Value as Localizable.LocalData).Text };

                    c++;
                }

                return JsonConvert.SerializeObject(data);
            }
            public Localization.Data Deserialize(string text) =>
                JsonConvert.DeserializeObject<Localization.Data>(text);
        }

        public void OpenLocalizations()
        {
            if ((_Drawer.Current as Theme).LanguageKey != "default")
            {
                AddMessage("theme lang override", 2f);

                return;
            }

            _Localizator.Open<ListLocalization>(this);
        }
        public void CloseLocalizations() => _Localizator.Close();
        public void ReloadLocalizations() => _Localizator.Collect();
        public Localizable.LocalData GetTranslation(string key)
        {
            if (_Localizator.TryGetValue<Localizable.LocalData>(key, out var data))
                return data;

            return null;
        }

        public void SelectLanguage(string langKey)
        {
            if (string.IsNullOrEmpty(langKey))
                return;

            var data = _Localizator.Default;
            for (int l = 0; l < _Localizator.AllData.Count; l++)
                if (_Localizator.AllData[l].Name == langKey)
                {
                    data = _Localizator.AllData[l];

                    Debug.Log($"Setting Language with {langKey} key");

                    break;
                }

            _Localizator.SetData(data);
        }

#if UNITY_EDITOR
        public void Reload()
        {
            var local = new Localization("en");
            for (int i = 0; i < _Localizator.Elements.Length; i++)
                local.Store[_Localizator.Elements[i].GetKey()] = new Localizable.LocalData { Text = (_Localizator.Elements[i] as Localizable).GetValue() };

            local.Store[_Tutorial.Key] = new Localizable.LocalData { Text = _Tutorial.Value };

            for (int i = 0; i < _Messenger.Messages.Length; i++)
                local.Store[_Messenger.Messages[i]] = new Localizable.LocalData { Text = _Messenger.Messages[i] };

            for (int i = 0; i < _Confirm.Keys.Length; i++)
                local.Store[_Confirm.Keys[i]] = new Localizable.LocalData { Text = _Confirm.Keys[i] };

            File.WriteAllText($"{Application.dataPath}/Resources/{_Localizator.DefaultPath}.json", _Localizator.Serialize(local));

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            var localizations = Resources
                .LoadAll<TextAsset>(_Localizator.ResourcesPath)
                .Where(x => !x.name.Contains("_default"))
                .ToArray();

            for (int i = 0; i < localizations.Length; i++)
            {
                var data = new Localization(_Localizator.Deserialize(localizations[i].text));
                foreach (var kvp in local.Store)
                    if (!data.Store.ContainsKey(kvp.Key))
                        data.Store[kvp.Key] = kvp.Value;

                File.WriteAllText($"{Application.dataPath}/Resources/{_Localizator.ResourcesPath}{localizations[i].name}.json", _Localizator.Serialize(data));
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
        protected class Drawer : ScrollBase
        {
            public Theme _Current => Current as Theme;

            public override void AddData(string serialized, string path, bool fromResources = false)
            {
                var manifest = Manifest.Cast(serialized);
                if (fromResources)
                    path += $"{manifest.name}/";
                else
                    path.Replace("manifest.json", "");

                AllData.Add(new Theme(manifest, path, fromResources));
            }
            public override void SetData(Data data)
            {
                base.SetData(data);

                for (int d = 0; d < Elements.Length; d++)
                {
                    var drawable = Elements[d] as Drawable;
                    if (drawable.IsNonRedrawable())
                        continue;

                    var key = drawable.GetKey();
                    if (TryGetValue(key, out var elData))
                        drawable.SetData(elData);
                }
            }

            protected override void LoadDefault() =>
                Default = new Theme(Manifest.Cast(Resources.Load<TextAsset>(DefaultPath).text), DefaultPath.Replace("manifest", ""), true);
        }

        public void OpenThemes() => _Drawer.Open<ListTheme>(this);
        public void CloseThemes() => _Drawer.Close();
        public void ReloadThemes() => _Drawer.Collect();
        public bool TryGetDrawData(string key, out Element.Data data) => _Drawer.TryGetValue(key, out data);

        public void SelectTheme(int index) => RedrawTheme(_Drawer.AllData[index]);

        protected virtual void RedrawTheme(Storage.Data data)
        {
            _Drawer.SetData(data);

            SelectLanguage((data as Theme).LanguageKey);
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

                _Messenger.MessageText.text = GetTranslation(_Messenger.Messages[_Messenger.Current.Index]).Text;
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
            _Confirm.Text.text = GetTranslation(_Confirm.Keys[key]).Text;
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
            _Tutorial.Text.text = GetTranslation(_Tutorial.Key).Text;
            _Tutorial.SetEnabled(true);
        }
        public void HideTutorial() => _Tutorial.SetEnabled(false);
        #endregion

        protected virtual void Awake()
        {
#if UNITY_ENTITIES_INSTALLED
            EntityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
#endif

            ReloadThemes();
            ReloadLocalizations();

            _Drawer.Load();
            _Localizator.Load();
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