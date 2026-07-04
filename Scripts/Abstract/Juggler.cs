using System;
using System.Collections.Generic;

using UnityEngine;

namespace UI
{
    public abstract class Juggler : MonoBehaviour
    {
        [SerializeField] protected float ScrollSpeed = 10f;
        [SerializeField] protected float ScrollEnd = 0.001f;

        [Space]
        [SerializeField] protected List<RectTransform> Items = new List<RectTransform>();

        protected Dictionary<int, Vector2> Positions = new Dictionary<int, Vector2>();

        protected virtual void Update()
        {
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
    }
}