using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace UI
{
    public class ScrollRect : Selectable
    {
        [SerializeField] float ScrollSpeed = 3f;
        [SerializeField] float Damping = 0.05f;
        [SerializeField] List<RectTransform> Children;

        bool IsHolding;
        float Delta;
        Vector3 Velocity;

        void Update()
        {
            if (IsHolding)
            {
                var scroll = Mouse.current.scroll.ReadValue();

                Velocity.x += scroll.x;
                Velocity.y += scroll.y;
            }

            Scroll();
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            base.OnPointerEnter(eventData);

            IsHolding = true;
        }
        public override void OnPointerExit(PointerEventData eventData)
        {
            base.OnPointerExit(eventData);

            IsHolding = false;
        }

        void Scroll()
        {
            if (Velocity.magnitude <= 0.001f)
            {
                Velocity = Vector3.zero;

                return;
            }

            Velocity *= 1f - Damping;

            var dp = Time.deltaTime * ScrollSpeed * Velocity;
            for (int c = 0; c < Children.Count; c++)
                Children[c].position += dp;
        }
    }
}