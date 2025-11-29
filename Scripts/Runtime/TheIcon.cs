using System;

using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    [RequireComponent(typeof(Image)), DisallowMultipleComponent]
    public class TheIcon : Element, IRedrawable
    {
        [SerializeField] bool NonRedrawable;
        [SerializeField] Image Image;

        Vector3 Origin;

        protected override void Start()
        {
            base.Start();

            if (!NonRedrawable &&
                 UIManager.TryGetDrawerData(GetKey(), out var data))
                SetData(data);
        }
        public override void SetData(Data data)
        {
            if (data == null)
                return;

            var icon = data as IconData;

            InitOrigin();

            if (Image && icon.Sprite)
                Image.sprite = icon.Sprite;
        }

        void InitOrigin()
        {
            if (Origin.magnitude > 0.00001f)
                return;

            Origin = Image.rectTransform.localPosition;
        }

        [Serializable]
        public class IconData : Data
        {
            public Sprite Sprite;
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            Image = GetComponent<Image>();
        }

        public bool IsRedrawable() => !NonRedrawable;
#endif
    }
}