using System.Collections.Generic;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace UI
{
    public class ScrollRect : Selectable
    {
        [Space]
        [SerializeField] float ValidationPeriod = 1f;
        [SerializeField] Vector2 Rect;
        [SerializeField] Vector2 Offset;
        [SerializeField] Vector2 Pivot;
        [SerializeField] Vector2 TargetOrigin;

        bool IsHolding;
        float T;
        Vector2 Target;
        Vector2 Velocity;

        [Space]
        [SerializeField] float ScrollSpeed = 3f;
        [SerializeField] float Damping = 0.05f;
        [SerializeField] float MaxVelocity = 50f;

        [Space]
        [SerializeField] List<RectTransform> Children;

        protected override void Start()
        {
            Target = TargetOrigin + Offset;
        }
        void Update()
        {
            if (IsHolding)
                Target += ScrollSpeed * Mouse.current.scroll.ReadValue();

            Scroll();
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            base.OnPointerEnter(eventData);

            IsHolding = true;

            if (Children.Count > 0)
                Target = Children[Children.Count - 1].anchoredPosition;
        }
        public override void OnPointerExit(PointerEventData eventData)
        {
            base.OnPointerExit(eventData);

            IsHolding = false;

            //Target = TargetOrigin + Offset;
        }

        void Scroll()
        {
            if (Children.Count == 0 ||
                !Children[Children.Count - 1])
                return;

            var dt = Time.deltaTime;
            var vec = Target - Children[Children.Count - 1].anchoredPosition;
            if (vec.magnitude <= 0.001f)
            {
                Velocity = Vector2.zero;

                T += dt;
                if (T >= ValidationPeriod)
                {
                    T = 0f;

                    ValidateDistances();
                }

                return;
            }

            Velocity *= 1f - Damping;
            Velocity += vec;
            Velocity = Vector2.ClampMagnitude(Velocity, MaxVelocity);

            var dp = dt * Velocity;
            for (int c = 0; c < Children.Count; c++)
                if (Children[c])
                    Children[c].anchoredPosition += dp;
        }
        void ValidateDistances()
        {
            var pos = Target;
            for (int c = Children.Count - 1; c >= 0; c--)
            {
                var rt = Children[c];

                rt.pivot = Pivot;
                rt.anchoredPosition = pos;
                rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Rect.x);

                pos.y += rt.rect.height;
                pos += Offset;
            }
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            Target = TargetOrigin + Offset;

            ValidateDistances();
        }
#endif
    }
}