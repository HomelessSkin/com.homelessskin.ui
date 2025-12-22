using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using Core;

using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    #region WINDOW BASE
    [Serializable]
    public abstract class WindowBase
    {
        [SerializeField] protected Transform Panel;

        public void SetEnabled(bool value) => Panel.gameObject.SetActive(value);
        public bool IsEnabled() => Panel.gameObject.activeSelf;
    }
    #endregion

    #region STORAGE
    [Serializable]
    public abstract class Storage : WindowBase, IStorage
    {
        public UIManagerBase Manager;

        public int _MaxSaveFiles => MaxSaveFiles;
        public string _DataFile => DataFile;
        public string _ResourcesPath => ResourcesPath;
        public string _PersistentPath => PersistentPath;
        public string _Dir => $"{Application.persistentDataPath}/{PersistentPath}";

        [Space]
        [SerializeField] int MaxSaveFiles;
        [SerializeField] string DataFile = "*.json";

        [Space]
        [SerializeField] string ResourcesPath;
        [SerializeField] string PersistentPath;

        public virtual void Store(IStorage.Data data)
        {
            if (string.IsNullOrEmpty(PersistentPath))
            {
                Manager.Log(this.GetType().FullName, $"No Persistent Path! Please set this Value inside UIManager's relevant field!", LogLevel.Error);

                return;
            }

            ((IStorage)this).Store(data);
        }
        public virtual string Collect(string name, string type = null)
        {
            var serialized = ((IStorage)this).Collect(name, type);
            if (string.IsNullOrEmpty(serialized))
                Manager.Log(this.GetType().FullName, $"Can not find File {name}!", LogLevel.Warning);

            return serialized;
        }
        public virtual async Task StoreAsync(IStorage.Data data)
        {
            if (string.IsNullOrEmpty(PersistentPath))
            {
                Manager.Log(this.GetType().FullName, $"No Persistent Path! Please set this Value inside UIManager's relevant field!", LogLevel.Error);

                return;
            }

            await ((IStorage)this).StoreAsync(data);
        }
        public virtual async Task<string> CollectAsync(string name, string type = null)
        {
            var serialized = await ((IStorage)this).CollectAsync(name, type);
            if (string.IsNullOrEmpty(serialized))
                Manager.Log(this.GetType().FullName, $"Can not find File {name}!", LogLevel.Warning);

            return serialized;
        }
    }
    #endregion

    #region DATA STORAGE
    [Serializable]
    public abstract class DataStorage : Storage
    {
        [Space]
        public List<IStorage.Data> AllData = new List<IStorage.Data>();

        public void AddData(IStorage.Data data) => AllData.Add(data);
        public abstract void AddData(string serialized, string path, bool fromResources = false, UIManagerBase manager = null);
        public virtual void CollectAllData()
        {
            AllData.Clear();

            if (!string.IsNullOrEmpty(_ResourcesPath))
            {
                var resManifests = Resources.LoadAll<TextAsset>(_ResourcesPath);
                for (int m = 0; m < resManifests.Length; m++)
                {
                    var file = resManifests[m];
                    AddData(file.text, $"{_ResourcesPath}", true, Manager);
                }
            }

            if (!string.IsNullOrEmpty(_PersistentPath))
            {
                if (!Directory.Exists(_Dir))
                    Directory.CreateDirectory(_Dir);
                else
                {
                    var buildManifests = Directory.GetFiles(_Dir, _DataFile, SearchOption.AllDirectories);
                    for (int m = 0; m < buildManifests.Length; m++)
                    {
                        var path = buildManifests[m];
                        AddData(File.ReadAllText(path), path, false, Manager);
                    }
                }
            }
        }
    }
    #endregion

    #region SCROLL BASE
    [Serializable]
    public abstract class ScrollBase : DataStorage
    {
        public ScrollRect Head;
        public Transform View;
        public GameObject ContentPrefab;
        public GameObject ItemPrefab;

        public void Open<T>(UIManagerBase manager)
            where T : ScrollItem
        {
            if (IsEnabled())
                return;

            Head.content = GameObject.Instantiate(ContentPrefab, View).transform as RectTransform;

            for (int a = 0; a < AllData.Count; a++)
            {
                var go = GameObject.Instantiate(ItemPrefab, Head.content);
                go.GetComponent<T>().Init(a, AllData[a], manager);
            }

            SetEnabled(true);
        }
        public void OpenWithType<T>(string type, UIManagerBase manager)
            where T : ScrollItem
        {
            if (IsEnabled())
                return;

            Head.content = GameObject.Instantiate(ContentPrefab, View).transform as RectTransform;

            for (int a = 0; a < AllData.Count; a++)
            {
                if (AllData[a].Type != type)
                    continue;

                var go = GameObject.Instantiate(ItemPrefab, Head.content);
                go.GetComponent<T>().Init(a, AllData[a], manager);
            }

            SetEnabled(true);
        }
        public void OpenButType<T>(string type, UIManagerBase manager)
            where T : ScrollItem
        {
            if (IsEnabled())
                return;

            Head.content = GameObject.Instantiate(ContentPrefab, View).transform as RectTransform;

            for (int a = 0; a < AllData.Count; a++)
            {
                if (AllData[a].Type == type)
                    continue;

                var go = GameObject.Instantiate(ItemPrefab, Head.content);
                go.GetComponent<T>().Init(a, AllData[a], manager);
            }

            SetEnabled(true);
        }
        public void Close()
        {
            if (Head.content)
                GameObject.Destroy(Head.content.gameObject);

            SetEnabled(false);
        }
    }
    #endregion

    #region PERSONAL STORAGE
    [Serializable]
    public abstract class PersonalizedStorage : ScrollBase, IPrefKey
    {
        public string DefaultPath;
        public string PrefKey;
        public string _Key => PrefKey;

        [Space]
        public IStorage.Data Default = new IStorage.Data();
        public IStorage.Data Current = new IStorage.Data();

        [Space]
        public Element[] Elements;

        protected abstract void LoadDefault();
        public virtual void SetData(IStorage.Data data)
        {
            Current = data;

            this.SavePrefString(data.Name);
        }
        public virtual void PickSaved()
        {
            LoadDefault();

            var saved = this.LoadPrefString();
            switch (saved)
            {
                case null:
                case "":
                case "Default":
                {
                    SetData(Default);
                }
                break;
                default:
                {
                    var found = false;
                    for (int m = 0; m < AllData.Count; m++)
                    {
                        var data = AllData[m];
                        if (data.Name == saved)
                        {
                            found = true;

                            SetData(data);

                            break;
                        }
                    }

                    if (!found)
                        goto case "Default";
                }
                break;
            }
        }

        public bool TryGetValue(string key, out Element.Data value) => TryGetValue<Element.Data>(key, out value);
        public bool TryGetValue<T>(string key, out T value) where T : Element.Data => TryGetCurrent<T>(key, out value) || TryGetDefault<T>(key, out value);
        public bool TryGetCurrent(string key, out Element.Data value) => TryGetCurrent<Element.Data>(key, out value);
        public bool TryGetCurrent<T>(string key, out T value) where T : Element.Data
        {
            value = null;
            if ((Current as Container).Map.TryGetValue(key, out var data))
            {
                value = data as T;

                return true;
            }

            return false;
        }
        public bool TryGetDefault(string key, out Element.Data value) => TryGetDefault<Element.Data>(key, out value);
        public bool TryGetDefault<T>(string key, out T value) where T : Element.Data
        {
            value = null;
            if ((Default as Container).Map.TryGetValue(key, out var data))
            {
                value = data as T;

                return true;
            }

            return false;
        }

        [Serializable]
        public class Container : IStorage.Data
        {
            public Dictionary<string, Element.Data> Map = new Dictionary<string, Element.Data>();
        }
    }
    #endregion
}