using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using Core;

using UnityEngine;

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
        public IStorage.Settings _Settings => StorageSettings;
        [Space]
        [SerializeField] IStorage.Settings StorageSettings;

        public virtual void Save(IStorage.Data data)
        {
            if (string.IsNullOrEmpty(_Settings.PersistentPath))
            {
                Log.Error(this, $"No Persistent Path! Please set this Value inside UIManager's relevant field!");

                return;
            }

            ((IStorage)this).Store(data);
        }
        public virtual string Load(string name, string type = null)
        {
            var serialized = ((IStorage)this).Collect(name, type);
            if (string.IsNullOrEmpty(serialized))
                Log.Warning(this, $"Can not find File {name}!");

            return serialized;
        }
        public virtual async Task SaveAsync(IStorage.Data data)
        {
            if (string.IsNullOrEmpty(_Settings.PersistentPath))
            {
                Log.Error(this, $"No Persistent Path! Please set this Value inside UIManager's relevant field!");

                return;
            }

            await ((IStorage)this).StoreAsync(data);
        }
        public virtual async Task<string> LoadAsync(string name, string type = null)
        {
            var serialized = await ((IStorage)this).CollectAsync(name, type);
            if (string.IsNullOrEmpty(serialized))
                Log.Warning(this, $"Can not find File {name}!");

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
        public abstract void AddData(string serialized, string path, bool fromResources = false);
        public virtual void CollectAllData()
        {
            AllData.Clear();

            if (!string.IsNullOrEmpty(_Settings.ResourcesPath))
            {
                var resManifests = Resources.LoadAll<TextAsset>(_Settings.ResourcesPath);
                for (int m = 0; m < resManifests.Length; m++)
                {
                    var file = resManifests[m];
                    AddData(file.text, $"{_Settings.ResourcesPath}", true);
                }
            }

            if (!string.IsNullOrEmpty(_Settings.PersistentPath))
            {
                var dir = (this as IStorage)._Settings.Dir;

                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                else
                {
                    var buildManifests = Directory.GetFiles(dir, _Settings.DataFile, SearchOption.AllDirectories);
                    for (int m = 0; m < buildManifests.Length; m++)
                    {
                        var path = buildManifests[m];
                        AddData(File.ReadAllText(path), path, false);
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
        public UnityEngine.UI.ScrollRect Head;
        public Transform View;
        public GameObject ContentPrefab;
        public GameObject ItemPrefab;

        public void Open<T>()
            where T : ScrollItem
        {
            if (IsEnabled())
                return;

            Head.content = GameObject.Instantiate(ContentPrefab, View).transform as RectTransform;

            for (int a = 0; a < AllData.Count; a++)
            {
                var go = GameObject.Instantiate(ItemPrefab, Head.content);
                go.GetComponent<T>().Init(a, AllData[a]);
            }

            SetEnabled(true);
        }
        public void OpenWithType<T>(string type)
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
                go.GetComponent<T>().Init(a, AllData[a]);
            }

            SetEnabled(true);
        }
        public void OpenButType<T>(string type)
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
                go.GetComponent<T>().Init(a, AllData[a]);
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