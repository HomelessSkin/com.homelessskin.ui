using TMPro;

using UnityEngine;

namespace UI
{
    public class HorizontalScrollSelection : Juggler
    {
        [Space]
        [SerializeField] Vector2 Position;
        [SerializeField] Vector2 RightHide;
        [SerializeField] Vector2 LeftHide;

        int Prev = 1;
        int Current = 0;

        public void Init(string value)
        {
            Items[0].GetComponent<TMP_Text>().text = value;

            Positions[Current] = Position;
        }
        public void ScrollLeft(string value)
        {
            Positions.Clear();

            Prev = Current;
            Current = (Current + 1) % 2;

            Items[Current].anchoredPosition = RightHide;
            Items[Current].GetComponent<TMP_Text>().text = value;

            Positions[Prev] = LeftHide;
            Positions[Current] = Position;
        }
        public void ScrollRight(string value)
        {
            Positions.Clear();

            Prev = Current;
            Current = (Current + 1) % 2;

            Items[Current].anchoredPosition = LeftHide;
            Items[Current].GetComponent<TMP_Text>().text = value;

            Positions[Prev] = RightHide;
            Positions[Current] = Position;
        }

        protected override void MoveToTarget(int index, float dt)
        {
            var pos = Positions[index];
            var delta = pos - Items[index].anchoredPosition;
            if (delta.magnitude > ScrollEnd)
                Items[index].anchoredPosition += ScrollSpeed * dt * delta.normalized;
            else
            {
                Items[index].anchoredPosition = pos;

                Positions.Remove(index);
            }
        }
    }
}