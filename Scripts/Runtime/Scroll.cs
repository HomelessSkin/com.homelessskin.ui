using System.Collections.Generic;

using UnityEngine;

namespace UI
{
    public class Scroll : Juggler
    {
        [Space]
        [SerializeField] int MaxView = 10;
        [SerializeField] float Spacing = 0f;
        [SerializeField] Vector2 PoolPosition;

        Queue<RectTransform> Pool = new Queue<RectTransform>();

        RectTransform Content => transform as RectTransform;

        protected override void ResetPositions()
        {
            Positions.Clear();

            Canvas.ForceUpdateCanvases();

            var pos = Vector2.zero;
            for (int i = Items.Count - 1; i >= 0; i--)
            {
                Positions[i] = pos;

                pos.y += Items[i].rect.height + Spacing;
            }
        }

        public void Init(GameObject prefab)
        {
            for (int c = 0; c < MaxView; c++)
            {
                var t = Instantiate(prefab, Content).GetComponent<RectTransform>();
                t.anchoredPosition = PoolPosition;
                t.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Content.rect.width);

                Pool.Enqueue(t);
            }
        }
        public void ToView(RectTransform transform)
        {
            transform.gameObject.SetActive(true);

            Items.Add(transform);
            if (Items.Count >= MaxView)
                ToPool(0);

            Juggle();
        }
        public void ToPool(int index)
        {
            var transform = Items[index];
            transform.gameObject.SetActive(false);
            transform.anchoredPosition = PoolPosition;

            Pool.Enqueue(transform);
            Items.RemoveAt(index);

            Juggle();
        }
        public bool TryGetFromPool(out RectTransform transform) => Pool.TryDequeue(out transform);
        public List<RectTransform> GetView() => Items;
    }
}