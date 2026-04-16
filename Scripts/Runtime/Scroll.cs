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

        bool IsResetRequired = false;

        List<RectTransform> View = new List<RectTransform>();
        Queue<RectTransform> Pool = new Queue<RectTransform>();
        Dictionary<int, Vector2> Targets = new Dictionary<int, Vector2>();

        RectTransform Content => transform as RectTransform;

        void Update()
        {
            if (IsResetRequired)
            {
                IsResetRequired = false;

                ResetTargets();
            }

            if (Targets.Count > 0)
            {
                var dt = Time.deltaTime;
                for (int v = 0; v < View.Count; v++)
                    if (Targets.TryGetValue(v, out var pos))
                    {
                        var rect = View[v].anchoredPosition;
                        var delta = pos - rect;

                        if (delta.sqrMagnitude > 0.001f)
                            View[v].anchoredPosition += ScrollSpeed * dt * delta;
                        else
                            Targets.Remove(v);
                    }
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

            View.Add(transform);
            if (View.Count >= MaxView)
                ToPool(0);

            IsResetRequired = true;
        }
        public void ToPool(int index)
        {
            var transform = View[index];
            transform.gameObject.SetActive(false);
            transform.anchoredPosition = PoolPosition;

            Pool.Enqueue(transform);
            View.RemoveAt(index);

            IsResetRequired = true;
        }
        public bool TryGetFromPool(out RectTransform transform) => Pool.TryDequeue(out transform);
        public List<RectTransform> GetView() => View;

        void ResetTargets()
        {
            Targets.Clear();

            Canvas.ForceUpdateCanvases();

            var pos = Vector2.zero;
            for (int v = View.Count - 1; v >= 0; v--)
            {
                Targets[v] = pos;

                pos.y += View[v].rect.height + Spacing;
            }
        }
    }
}