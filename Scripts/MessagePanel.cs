using System.Collections.Generic;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

namespace Core.UI
{
    public class MessagePanel : MonoBehaviour
    {
#if UNITY_EDITOR
        [SerializeField] GUIStyle Style;
        [SerializeField] bool ShowMessages;
#endif

        [SerializeField] LocalizationManager Localizator;
        [SerializeField] Image Back;
        [SerializeField] TMP_Text MessageText;
        [SerializeField] TMP_Text AdditionText;
        [SerializeField] string[] Messages;

        Message Current;
        Queue<Message> Q = new Queue<Message>();

        void Start()
        {
            Current = new Message();
            Q = new Queue<Message>();
        }
        void Update()
        {
#if UNITY_EDITOR
            if (!ShowMessages)
                return;
#endif

            if (Current.Time != 0f)
            {
                if (Current.Addition != AdditionType.Null)
                    AdditionText.text = GetAddition(Current.Addition);

                if (Current.Time + Current.CallTime < Time.realtimeSinceStartup)
                {
                    Current.Time = 0f;
                    if (Q.Count > 0)
                        Current = Q.Dequeue();

                    AdditionText.text = "";

                    RefreshCurrent();
                }
            }
            else if (Q.Count > 0)
            {
                Current = Q.Dequeue();

                RefreshCurrent();
            }
        }

        public void AddMessage(int index, float time = 5f, AdditionType addition = AdditionType.Null)
        {
            if (index >= Messages.Length)
            {
                Debug.Log($"{index} greater then range of Messages array");

                return;
            }

            Q.Enqueue(new Message
            {
                Index = index,
                Time = time,
                Addition = addition
            });
        }

        void RefreshCurrent()
        {
            if (Current.Time == 0f)
            {
                MessageText.text = "";
                MessageText.gameObject.SetActive(false);
                AdditionText.gameObject.SetActive(false);
                Back.gameObject.SetActive(false);
            }
            else
            {
                Current.CallTime = Time.realtimeSinceStartup;

                MessageText.text = Localizator.GetString(Messages[Current.Index]);
                MessageText.gameObject.SetActive(true);
                AdditionText.gameObject.SetActive(true);
                Back.gameObject.SetActive(true);
            }
        }

        string GetAddition(AdditionType addition)
        {
            //switch (addition)
            //{
            //}

            return "";
        }

        class Message
        {
            public int Index;
            public float Time;
            public float CallTime;
            public AdditionType Addition;
        }

        public enum AdditionType : byte
        {
            Null = 0,
            AdTimer = 1,

        }

#if UNITY_EDITOR
        public string[] GetMessages() => Messages;

        void OnGUI()
        {
            //GUI.Label(new Rect(30, 30, 300, 30), YG2.timerInterAdv.ToString(), Style);
            //GUI.Label(new Rect(30, 60, 300, 30), Current.Time.ToString(), Style);
            //GUI.Label(new Rect(30, 90, 300, 30), Current.CallTime.ToString(), Style);
            //GUI.Label(new Rect(30, 120, 300, 30), Time.realtimeSinceStartup.ToString(), Style);
        }
#endif
    }
}