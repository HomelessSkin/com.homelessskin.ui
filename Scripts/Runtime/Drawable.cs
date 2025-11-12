using System;

using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    [RequireComponent(typeof(Image)), DisallowMultipleComponent]
    public class Drawable : UIElement
    {
        [SerializeField] Image Base;
        [SerializeField] Image Mask;
        [SerializeField] Image Overlay;

        public override string GetKey() => _Type.ToString();
        public Sprite GetValue() => Base.sprite;
        public void SetValue(Data data)
        {
            Base.sprite = data.Base;
            Mask.sprite = data.Mask;

            if (data.Overlay)
                Overlay.sprite = data.Overlay;
            else
            {
                var color = Overlay.color;
                color.a = 0f;
                Overlay.color = color;
            }
        }

        protected override void Start()
        {
            base.Start();

            if (UIManager.TryGetSprite(GetKey(), out var data))
                SetValue(data);
        }

        #region DATA
        [Serializable]
        public class Data
        {
            public Sprite Base;
            public Sprite Mask;
            public Sprite Overlay;
        }
        #endregion 
    }
}