using System.Collections.Generic;

using UnityEngine;

namespace UI
{
    public abstract class Juggler : MonoBehaviour
    {
        [SerializeField] float ScrollSpeed = 10f;

        [Space]
        [SerializeField] protected List<RectTransform> Items = new List<RectTransform>();

        bool IsResetRequired = false;

        protected Dictionary<int, Vector2> Positions = new Dictionary<int, Vector2>();

        protected virtual void Update()
        {
            if (IsResetRequired)
            {
                IsResetRequired = false;

                ResetPositions();
            }

            if (Positions.Count > 0)
            {
                var dt = Time.deltaTime;
                for (int v = 0; v < Items.Count; v++)
                    if (Positions.ContainsKey(v))
                        MoveToTarget(v, dt);
            }
        }

        protected abstract void ResetPositions();

        protected virtual void MoveToTarget(int index, float dt)
        {
            var delta = Positions[index] - Items[index].anchoredPosition;

            if (delta.sqrMagnitude > 0.001f)
                Items[index].anchoredPosition += ScrollSpeed * dt * delta;
            else
                Positions.Remove(index);
        }

        protected void Juggle() => IsResetRequired = true;
    }
}