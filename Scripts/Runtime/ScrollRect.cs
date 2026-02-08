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
        [SerializeField] Vector2 Rect;
        [SerializeField] Vector3 Offset;
        [SerializeField] Vector3 Pivot;
        [SerializeField] Vector3 TargetOrigin;

        [Space]
        [SerializeField] float ScrollSpeed = 3f;
        [SerializeField] float Damping = 0.05f;
        [SerializeField] float MaxVelocity = 50f;

        [Space]
        [SerializeField] List<RectTransform> Children;

        bool IsHolding;
        Vector3 Target;
        Vector3 Velocity;

        protected override void Start()
        {
        }
        void Update()
        {
            if (IsHolding)
            {
                var scroll = ScrollSpeed * Mouse.current.scroll.ReadValue();

                Target.x += scroll.x;
                Target.y += scroll.y;
            }

            Scroll();
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            base.OnPointerEnter(eventData);

            IsHolding = true;

            if (Children.Count > 0)
                Target = Children[0].position;
        }
        public override void OnPointerExit(PointerEventData eventData)
        {
            base.OnPointerExit(eventData);

            IsHolding = false;

            Target = TargetOrigin;
        }

        void Scroll()
        {
            if (Children.Count == 0 ||
                !Children[0])
                return;

            var vec = Target - Children[0].position;
            if (vec.magnitude <= 0.001f)
            {
                Velocity = Vector3.zero;

                return;
            }

            Velocity *= 1f - Damping;
            Velocity += vec;
            Velocity = Vector3.ClampMagnitude(Velocity, MaxVelocity);

            var dp = Time.deltaTime * Velocity;
            for (int c = 0; c < Children.Count; c++)
                if (Children[c])
                    Children[c].position += dp;
        }
        void ValidateDistances()
        {
            var pos = TargetOrigin + Offset;
            for (int c = Children.Count - 1; c >= 0; c--)
            {
                var rt = Children[c];

                rt.pivot = Pivot;
                rt.position = pos;
                rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Rect.x);

                pos.y += rt.rect.height / 100f;
                pos += Offset;
            }
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            Target = TargetOrigin;

            ValidateDistances();
        }
#endif
    }
}