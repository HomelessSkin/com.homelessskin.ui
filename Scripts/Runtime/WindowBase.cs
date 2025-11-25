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
        public Data Default;
        public Data Current;

        public List<Data> _Data = new List<Data>();
        public Element[] Elements;

        public abstract class Data
        {
            public string _Name { get; }

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

            for (int l = 0; l < _Data.Count; l++)
            {
                var go = GameObject.Instantiate(ItemPrefab, Head.content);
                var lp = go.GetComponent<T>();
                lp.Init(l, _Data[l], manager);
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