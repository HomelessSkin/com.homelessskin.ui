using System;

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
        public MenuScroll Scroll;

        public void Close()
        {
            if (Scroll.Head.content)
                GameObject.Destroy(Scroll.Head.content.gameObject);

            SetEnabled(false);
        }

        [Serializable]
        public class MenuScroll
        {
            public ScrollRect Head;
            public Transform View;
            public GameObject ContentPrefab;
            public GameObject ItemPrefab;
        }
    }
}