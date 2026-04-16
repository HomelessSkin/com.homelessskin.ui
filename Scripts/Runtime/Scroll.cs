using System.Collections.Generic;

using UnityEngine;

namespace UI
{
    public class Scroll : MonoBehaviour
    {
        [SerializeField] int MaxView = 10;
        [SerializeField] float ScrollSpeed = 10f;
        [SerializeField] float Spacing = 0f;
        [SerializeField] Vector2 PoolPosition;

        int MinY;
        int MaxY;

        Vector2 ContentShift;
        Vector2Int ContentCurrent;
        Vector2Int ContentTarget;

        List<RectTransform> View = new List<RectTransform>();
        Queue<RectTransform> Pool = new Queue<RectTransform>();

        RectTransform Content => transform as RectTransform;

        void Start()
        {
            MinY = (int)Content.anchoredPosition.y - 40;

            ContentShift = Content.anchoredPosition;
            ContentTarget = ContentCurrent = Vector2Int.RoundToInt(ContentShift) + 40 * Vector2Int.down;
        }
        void Update()
        {
            var dt = Time.deltaTime;
            var pos = Vector2.zero;
            for (int v = View.Count - 1; v >= 0; v--)
            {
                var rect = View[v].anchoredPosition;
                var delta = pos - rect;

                View[v].anchoredPosition += ScrollSpeed * dt * delta;

                pos.y += View[v].rect.height + Spacing;
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
            View.Add(transform);
            if (View.Count >= MaxView)
                ToPool(0);
        }
        public void ToPool(int index)
        {
            var t = View[index];
            t.gameObject.SetActive(false);
            t.anchoredPosition = PoolPosition;

            Pool.Enqueue(t);
            View.RemoveAt(index);
        }
        public bool TryGetFromPool(out RectTransform transform)
        {
            if (Pool.TryDequeue(out transform))
            {
                transform.gameObject.SetActive(true);

                return true;
            }

            return false;
        }
        public List<RectTransform> GetView() => View;
    }
}