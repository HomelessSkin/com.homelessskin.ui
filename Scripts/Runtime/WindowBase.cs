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
    public abstract class ScrollBase : WindowBase
    {
        public ScrollRect Head;
        public Transform View;
        public GameObject ContentPrefab;
        public GameObject ItemPrefab;

        public void Open<U, T>(List<U> values, UIManagerBase manager)
            where U : IInitData
            where T : ScrollItem
        {
            if (IsEnabled())
                return;

            Head.content = GameObject.Instantiate(ContentPrefab, View).transform as RectTransform;

            for (int l = 0; l < values.Count; l++)
            {
                var go = GameObject.Instantiate(ItemPrefab, Head.content);
                var lp = go.GetComponent<T>();
                lp.Init(l, values[l], manager);
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