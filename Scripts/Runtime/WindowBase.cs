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
    public abstract class Storage : WindowBase
    {
        public UIManagerBase Manager;

        [Space]
        public string DataFile = "*.json";

        [Space]
        public string ResourcesPath;
        public string PersistentPath;
        public string Dir => $"{Application.persistentDataPath}/{PersistentPath}";

        public virtual string Collect(string name, string type = null)
        {
            var path = $"{Dir}";
            if (!string.IsNullOrEmpty(type))
                path += $"{type}/";
            path += $"{name}{DataFile.Replace("*", "")}";

            if (File.Exists(path))
                return File.ReadAllText(path);

            Manager.Log(this.GetType().ToString(), $"Can not find File at {path}!", UIManagerBase.LogLevel.Warning);

            return null;
        }
        public virtual async Task<string> CollectAsync(string name, string type = null)
        {
            var path = $"{Dir}";
            if (!string.IsNullOrEmpty(type))
                path += $"{type}/";
            path += $"{name}{DataFile.Replace("*", "")}";

            if (File.Exists(path))
                return await File.ReadAllTextAsync(path);

            Manager.Log(this.GetType().ToString(), $"Can not find File at {path}!", UIManagerBase.LogLevel.Warning);

            return "";
        }
        public virtual void Store(Data data)
        {
            if (string.IsNullOrEmpty(PersistentPath))
            {
                Manager.Log(this.GetType().FullName, $"No Persistent Path! Please set this Value inside UIManager's relevant field!", UIManagerBase.LogLevel.Error);

                return;
            }

            if (!Directory.Exists($"{Dir}/{data.Type}"))
                Directory.CreateDirectory($"{Dir}/{data.Type}");

            File.WriteAllText($"{Dir}/{data.Type}/{data.Name}{DataFile.Replace("*", "")}", data.Serialize());
        }
        public async virtual Task StoreAsync(Data data)
        {
            if (string.IsNullOrEmpty(PersistentPath))
            {
                Manager.Log(this.GetType().FullName, $"No Persistent Path! Please set this Value inside UIManager's relevant field!", UIManagerBase.LogLevel.Error);

                return;
            }

            if (!Directory.Exists($"{Dir}/{data.Type}"))
                Directory.CreateDirectory($"{Dir}/{data.Type}");

            await File.WriteAllTextAsync($"{Dir}/{data.Type}/{data.Name}{DataFile.Replace("*", "")}", data.Serialize());
        }

        [Serializable]
        public class Data
        {
            public string Name;
            public string Type;

            public virtual string Serialize() => JsonUtility.ToJson(this, true);
        }
    }
    #endregion

    #region DATA STORAGE
    [Serializable]
    public abstract class DataStorage : Storage
    {
        [Space]
        public List<Data> AllData = new List<Data>();

        public void AddData(Data data) => AllData.Add(data);
        public abstract void AddData(string serialized, string path, bool fromResources = false, UIManagerBase manager = null);
        public virtual void CollectAllData()
        {
            AllData.Clear();

            if (!string.IsNullOrEmpty(ResourcesPath))
            {
                var resManifests = Resources.LoadAll<TextAsset>(ResourcesPath);
                for (int m = 0; m < resManifests.Length; m++)
                {
                    var file = resManifests[m];
                    AddData(file.text, $"{ResourcesPath}", true, Manager);
                }
            }

            if (!string.IsNullOrEmpty(PersistentPath))
            {
                if (!Directory.Exists(Dir))
                    Directory.CreateDirectory(Dir);
                else
                {
                    var buildManifests = Directory.GetFiles(Dir, DataFile, SearchOption.AllDirectories);
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
        public Data Default = new Data();
        public Data Current = new Data();

        [Space]
        public Element[] Elements;

        protected abstract void LoadDefault();
        public virtual void SetData(Data data)
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
        public class Container : Data
        {
            public Dictionary<string, Element.Data> Map = new Dictionary<string, Element.Data>();
        }
    }
    #endregion
}