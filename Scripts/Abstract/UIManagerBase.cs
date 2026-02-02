using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Core;

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
        [SerializeField] protected Canvasser _Canvasser;
        #region CANVASSER
        [Serializable]
        protected class Canvasser
        {
            public Reference[] Refs;
            #region REFERENCE
            [Serializable]
            public class Reference
            {
                [HideInInspector] public string Name;
                public UpdateType UpdateMode;
                public bool RenderFrame;
                public Canvas Canvas;
                public Camera Camera;

                public void Setup()
                {
                    Canvas.renderMode = RenderMode.ScreenSpaceCamera;
                    Canvas.worldCamera = Camera;

                    switch (UpdateMode)
                    {
                        case UpdateType.Default:
                        Camera.enabled = true;
                        break;
                        case UpdateType.Manual:
                        Camera.enabled = false;
                        break;
                    }

                    QueueRender();
                }
                public void Render()
                {
                    RenderFrame = false;
                    if (UpdateMode == UpdateType.Default)
                        return;

                    Camera.Render();
                }
                public void QueueRender() => RenderFrame = true;

                public enum UpdateType : byte
                {
                    Default = 0,
                    Manual = 1,


                }
            }
            #endregion

            public void Setup()
            {
                for (int r = 0; r < Refs.Length; r++)
                    Refs[r].Setup();
            }
            public void QueueRender(int index = 0) => Refs[index].QueueRender();
            public void QueueRender(string name)
            {
                if (string.IsNullOrEmpty(name))
                    return;

                for (int r = 0; r < Refs.Length; r++)
                    if (Refs[r].Name == name)
                        Refs[r].QueueRender();
            }
            public void QueueAll()
            {
                for (int r = 0; r < Refs.Length; r++)
                    Refs[r].QueueRender();
            }
            public void Render()
            {
                for (int r = 0; r < Refs.Length; r++)
                    Refs[r].Render();
            }
        }
        #endregion

        [Space]
        [SerializeField] protected Localizator _Localizator;
        #region LOCALIZATOR
        [Serializable]
        protected class Localizator : PersonalizedStorage
        {
            public override void AddData(string serialized, string path, bool fromResources = false) =>
                AllData.Add(new Localization(JsonUtility.FromJson<Localization.Data>(serialized)));
            public override void SetData(IStorage.Data data)
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
                Log.Warning(this, "theme lang override");

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

                    Log.Info(this, $"Setting Language with {langKey} key");

                    break;
                }

            _Localizator.SetData(data);
        }

#if UNITY_EDITOR
        public void Reload()
        {
            OnValidate();

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
                .LoadAll<TextAsset>(_Localizator._Settings.ResourcesPath)
                .Where(x => !x.name.Contains("_default"))
                .ToArray();

            for (int i = 0; i < localizations.Length; i++)
            {
                var data = new Localization(_Localizator.Deserialize(localizations[i].text));
                foreach (var kvp in local.Map)
                    if (!data.Map.ContainsKey(kvp.Key))
                        data.Map[kvp.Key] = kvp.Value;

                File.WriteAllText($"{Application.dataPath}/Resources/{_Localizator._Settings.ResourcesPath}{localizations[i].name}.json", _Localizator.Serialize(data));
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
            public override void AddData(string serialized, string path, bool fromResources = false)
            {
                var manifest = Manifest.Cast(serialized);
                if (fromResources)
                    path += $"{manifest.name}/";
                else
                    path = path.Replace("manifest.json", "");

                AllData.Add(new Theme(manifest, path, fromResources));
            }
            public override void SetData(IStorage.Data data)
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

        protected virtual void RedrawTheme(IStorage.Data data)
        {
            _Drawer.SetData(data);

            SelectLanguage((data as Theme).LanguageKey);
        }
        #endregion

        [Space]
        [SerializeField] protected Logger _Logger;
        #region LOGGER
        [Serializable]
        protected class Logger : WindowBase
        {
            [Space]
            public Type _Type;
            public TMP_Text LogText;

            [Space]
            public int MaxRowCount = 100;

            string Console;

            Log.Message Current;

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

                    var list = new List<string>();
                    if (!string.IsNullOrEmpty(Console))
                    {
                        list.AddRange(Console.Split("|", StringSplitOptions.RemoveEmptyEntries));
                        while (list.Count > MaxRowCount)
                            list.RemoveAt(0);
                    }

                    while (Log.Read(out var message))
                    {
                        list.Add($"{message.Text}\n");
                        if (list.Count > MaxRowCount)
                            list.RemoveAt(0);
                    }

                    Console = "";
                    for (int t = 0; t < list.Count; t++)
                        Console += $"{list[t]}|";

                    LogText.text = Console.Replace("|", "");
                }
                void AsPopUp()
                {
                    if (Current != null &&
                         Current.Time != 0f)
                    {
                        if (Current.Time + Current.CallTime < Time.realtimeSinceStartup)
                        {
                            Current.Time = 0f;

                            Log.Read(out Current);

                            RefreshCurrent();
                        }
                    }
                    else if (Log.Read(out Current))
                        RefreshCurrent();

                    void RefreshCurrent()
                    {
                        if (Current == null ||
                              Current.Time == 0f)
                        {
                            LogText.text = "";

                            SetEnabled(false);
                        }
                        else
                        {
                            Current.CallTime = Time.realtimeSinceStartup;
                            LogText.text = Current.Text;

                            SetEnabled(true);
                        }
                    }
                }
            }

            public enum Type : byte
            {
                Console = 0,
                PopUp = 1,

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

            _Canvasser.Setup();

            _Drawer.CollectAllData();
            _Drawer.PickSaved();

            _Localizator.CollectAllData();
            _Localizator.PickSaved();
        }
        protected virtual void Update()
        {
            _Logger.Update();
        }
        protected virtual void LateUpdate()
        {
            _Canvasser.Render();
        }
        protected virtual void OnDestroy()
        {

        }

#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            if (_Canvasser.Refs != null)
                for (int r = 0; r < _Canvasser.Refs.Length; r++)
                    if (_Canvasser.Refs[r].Canvas)
                        _Canvasser.Refs[r].Name = _Canvasser.Refs[r].Canvas.name;

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
            Core.Tool.CreateTag("UIManager");
            gameObject.tag = "UIManager";

            if (!Directory.Exists(Application.dataPath + "/Resources/UI/Localizations/"))
                Directory.CreateDirectory(Application.dataPath + "/Resources/UI/Localizations/");

            if (!File.Exists(Application.dataPath + "/Resources/UI/Localizations/_default.json"))
                File.Create(Application.dataPath + "/Resources/UI/Localizations/_default.json");
        }
#endif
    }
}