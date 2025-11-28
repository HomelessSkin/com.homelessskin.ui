using System;
using System.Collections.Generic;
using System.IO;

using Core.Util;

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
    public abstract class Storage : WindowBase, IPrefKey
    {
        public string DataFile;
        public string PrefKey;
        public string _Key => PrefKey;

        [Space]
        public string DefaultPath;
        public string ResourcesPath;
        public string PersistentPath;
        public string Dir => $"{Application.persistentDataPath}/{PersistentPath}";

        [Space]
        public Data Default = new Data();
        public Data Current = new Data();

        [Space]
        public List<Data> AllData = new List<Data>();

        [Space]
        public Element[] Elements;

        public bool TryGetValue(string key, out Element.Data value) => TryGetValue<Element.Data>(key, out value);
        public bool TryGetValue<T>(string key, out T value) where T : Element.Data => TryGetCurrent<T>(key, out value) || TryGetDefault<T>(key, out value);
        public bool TryGetCurrent(string key, out Element.Data value) => TryGetCurrent<Element.Data>(key, out value);
        public bool TryGetCurrent<T>(string key, out T value) where T : Element.Data
        {
            value = null;
            if (Current.Store.TryGetValue(key, out var data))
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
            if (Default.Store.TryGetValue(key, out var data))
            {
                value = data as T;

                return true;
            }

            return false;
        }
        public void AddData(Data data) => AllData.Add(data);

        public abstract void AddData(string serialized, string path, bool fromResources = false);
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
        public virtual void Collect()
        {
            AllData.Clear();

            if (!string.IsNullOrEmpty(ResourcesPath))
            {
                var resManifests = Resources.LoadAll<TextAsset>(ResourcesPath);
                for (int m = 0; m < resManifests.Length; m++)
                {
                    var file = resManifests[m];
                    AddData(file.text, $"{ResourcesPath}", true);
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
                        AddData(File.ReadAllText(path), path);
                    }
                }
            }
        }
        public virtual void Store(string name, string serialized)
        {
            if (string.IsNullOrEmpty(PersistentPath))
            {
                Debug.LogError($"No Persistent Path for {this.GetType()} Data storing!\nPlease set this Value inside UIManager's relevant field!");

                return;
            }

            if (!Directory.Exists(Dir))
                Directory.CreateDirectory(Dir);

            var path = $"{Dir}{name}_{DataFile}";
            File.Create(path);
            File.WriteAllText(path, serialized);
        }

        [Serializable]
        public class Data
        {
            public string Name;

            public Dictionary<string, Element.Data> Store = new Dictionary<string, Element.Data>();
        }
    }
    #endregion

    #region SCROLL BASE
    [Serializable]
    public abstract class ScrollBase : Storage
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

            for (int l = 0; l < AllData.Count; l++)
            {
                var go = GameObject.Instantiate(ItemPrefab, Head.content);
                var lp = go.GetComponent<T>();
                lp.Init(l, AllData[l], manager);
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
}