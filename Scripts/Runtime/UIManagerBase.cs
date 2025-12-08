using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using TMPro;

#if UNITY_ENTITIES_INSTALLED
using Unity.Entities;
#endif

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
        protected class Localizator : PersonalizedStorage
        {
            public override void AddData(string serialized, string path, bool fromResources = false, UIManagerBase manager = null) =>
                AllData.Add(new Localization(JsonUtility.FromJson<Localization.Data>(serialized)));
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
                var data = new Localization.Data { name = localization.Name, dictionary = new Localization.Data.KVP[localization.Map.Count] };
                var c = 0;
                foreach (var kvp in localization.Map)
                {
                    data.dictionary[c] = new Localization.Data.KVP { key = kvp.Key, value = (kvp.Value as Localizable.LocalData).Text };

                    c++;
                }

                return JsonUtility.ToJson(data, true);
            }
            public Localization.Data Deserialize(string text) =>
                JsonUtility.FromJson<Localization.Data>(text);
        }

        public void OpenLocalizations()
        {
            if ((_Drawer.Current as Theme).LanguageKey != "default")
            {
                Log(this.GetType().ToString(), "theme lang override", LogLevel.Warning);

                return;
            }

            _Localizator.Open<ListLocalization>(this);
        }
        public void CloseLocalizations() => _Localizator.Close();
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

                    Log(this.GetType().ToString(), $"Setting Language with {langKey} key");

                    break;
                }

            _Localizator.SetData(data);
        }

#if UNITY_EDITOR
        public void Reload()
        {
            var local = new Localization("en");
            for (int i = 0; i < _Localizator.Elements.Length; i++)
                local.Map[_Localizator.Elements[i].GetKey()] = new Localizable.LocalData { Text = (_Localizator.Elements[i] as Localizable).GetValue() };

            local.Map[_Tutorial.Key] = new Localizable.LocalData { Text = _Tutorial.Value };

            for (int i = 0; i < _Confirm.Keys.Length; i++)
                local.Map[_Confirm.Keys[i]] = new Localizable.LocalData { Text = _Confirm.Keys[i] };

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
                foreach (var kvp in local.Map)
                    if (!data.Map.ContainsKey(kvp.Key))
                        data.Map[kvp.Key] = kvp.Value;

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
        protected class Drawer : PersonalizedStorage
        {
            public override void AddData(string serialized, string path, bool fromResources = false, UIManagerBase manager = null)
            {
                var manifest = Manifest.Cast(serialized);
                if (fromResources)
                    path += $"{manifest.name}/";
                else
                    path = path.Replace("manifest.json", "");

                AllData.Add(new Theme(manifest, path, fromResources));
            }
            public override void SetData(Data data)
            {
                base.SetData(data);

                for (int d = 0; d < Elements.Length; d++)
                {
                    var element = Elements[d];
                    if (!(element as IRedrawable).IsRedrawable())
                        continue;

                    var key = element.GetKey();
                    if (!string.IsNullOrEmpty(key) &&
                         TryGetValue(key, out var elData))
                        element.SetData(elData);
                }
            }

            protected override void LoadDefault() =>
                Default = new Theme(Manifest.Cast(Resources.Load<TextAsset>(DefaultPath).text), DefaultPath.Replace("manifest", ""), true);
        }

        public void OpenThemes() => _Drawer.Open<ListTheme>(this);
        public void CloseThemes() => _Drawer.Close();
        public void RefreshThemes()
        {
            _Drawer.Close();
            _Drawer.CollectAllData();
            _Drawer.Open<ListTheme>(this);

            RedrawTheme(_Drawer.Current);
        }
        public bool TryGetDrawerData(string key, out Element.Data data) => _Drawer.TryGetValue(key, out data);

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
        protected class Messenger : Storage
        {
            [Space]
            public Type _Type;
            public TMP_Text MessageText;

            [Space]
            public float[] Timers = new float[Enum.GetValues(typeof(LogLevel)).Length];

            Message Current;
            Queue<Message> Q = new Queue<Message>();

            public void Update()
            {
                switch (_Type)
                {
                    case Type.Console:
                    AsConsole();
                    break;
                    case Type.PopUp:
                    AsPopUp();
                    break;
                }

                void AsConsole()
                {
                    if (!IsEnabled())
                        SetEnabled(true);

                    while (Q.Count > 0)
                        MessageText.text += $"{Q.Dequeue().Text}\n";
                }
                void AsPopUp()
                {
                    if (Current != null && Current.Time != 0f)
                    {
                        if (Current.Time + Current.CallTime < Time.realtimeSinceStartup)
                        {
                            Current.Time = 0f;
                            if (Q.Count > 0)
                                Current = Q.Dequeue();

                            RefreshCurrent();
                        }
                    }
                    else if (Q.Count > 0)
                    {
                        Current = Q.Dequeue();

                        RefreshCurrent();
                    }

                    void RefreshCurrent()
                    {
                        if (Current.Time == 0f)
                        {
                            MessageText.text = "";

                            SetEnabled(false);
                        }
                        else
                        {
                            Current.CallTime = Time.realtimeSinceStartup;
                            MessageText.text = Current.Text;

                            SetEnabled(true);
                        }
                    }
                }
            }
            public void Enqueue(Message message) => Q.Enqueue(message);

            public enum Type : byte
            {
                Console = 0,
                PopUp = 1,

            }
        }

        protected class Message
        {
            public string Text;
            public float Time;
            public float CallTime;
        }

        public enum LogLevel : byte
        {
            Nominal = 0,
            Error = 1,
            Warning = 2,

        }

        public void Log(string agent, string key, LogLevel level = LogLevel.Nominal)
        {
            var log = $"[{agent}] {key}";

            _Messenger.Enqueue(new Message
            {
                Text = log,
                CallTime = Time.realtimeSinceStartup,
                Time = _Messenger.Timers[(int)level],
            });

            switch (level)
            {
                case LogLevel.Nominal:
                Debug.Log(log);
                break;
                case LogLevel.Warning:
                Debug.LogWarning(log);
                break;
                case LogLevel.Error:
                Debug.LogError(log);
                break;
            }
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

        public virtual void Close()
        {
#if UNITY_EDITOR
            EditorApplication.ExitPlaymode();
#else
            Application.Quit();
#endif
        }

        protected virtual void Awake()
        {
#if UNITY_ENTITIES_INSTALLED
            EntityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
#endif

            _Drawer.CollectAllData();
            _Drawer.PickSaved();

            _Localizator.CollectAllData();
            _Localizator.PickSaved();
        }
        protected virtual void Update()
        {
            _Messenger.Update();
        }
        protected virtual void OnDestroy()
        {

        }
#if UNITY_EDITOR
        public virtual void OnValidate()
        {
            _Localizator.Manager =
            _Drawer.Manager = this;

            _Localizator.Elements = ((Element[])GameObject
                .FindObjectsByType(typeof(Localizable), FindObjectsInactive.Include, FindObjectsSortMode.None))
                .Where(x => x.gameObject.tag != "EditorOnly")
                .ToArray();

            var list = new List<Element>();
            list.AddRange(((Element[])GameObject
                .FindObjectsByType(typeof(Drawable), FindObjectsInactive.Include, FindObjectsSortMode.None))
                .Where(x => x.gameObject.tag != "EditorOnly")
                .ToArray());
            list.AddRange(((Element[])GameObject
                .FindObjectsByType(typeof(TheIcon), FindObjectsInactive.Include, FindObjectsSortMode.None))
                .Where(x => x.gameObject.tag != "EditorOnly")
                .ToArray());

            _Drawer.Elements = list.ToArray();
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