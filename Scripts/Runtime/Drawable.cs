using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    [RequireComponent(typeof(Image))]
    public class Drawable : UIElement
    {
        [SerializeField] Image Value;

        public Sprite GetValue() => Value.sprite;
        public void SetValue(Sprite sprite) => Value.sprite = sprite;
    }
}