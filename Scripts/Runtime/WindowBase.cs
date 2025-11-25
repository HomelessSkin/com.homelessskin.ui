using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    [Serializable]
    public abstract class WindowBase
    {
        [SerializeField] protected Transform Panel;

        public void SetEnabled(bool value) => Panel.gameObject.SetActive(value);
        public bool IsEnabled() => Panel.gameObject.activeSelf;
    }

    [Serializable]
    public abstract class Storage : WindowBase
    {
        public Data Default = new Data();
        public Data Current = new Data();

        public List<Data> AllData = new List<Data>();
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

        [Serializable]
        public class Data
        {
            public string Name;

            public Dictionary<string, Element.Data> Store = new Dictionary<string, Element.Data>();
        }
    }

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
}