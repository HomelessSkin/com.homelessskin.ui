using System;
using System.Collections.Generic;
using System.IO;

using Core;

using TMPro;

#if UNITY_ENTITIES_INSTALLED
using Unity.Entities;
#endif

#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;
using UnityEngine.Events;

namespace UI
{
    public abstract class UIManagerBase : MonoBehaviour
    {
        protected EntityManager EntityManager;

        [Space]
        [SerializeField] FPS _FPS;
        #region FPS
        [Serializable]
        class FPS : WindowBase
        {
            [Space]
            [Range(1, 60)] public int FramesCount = 5;
            public TMP_Text FPSGUI;

            int Frame;
            float StoredDelta;

            public void LockFPS(int type)
            {
                switch (type)
                {
                    case 0:
                    Application.targetFrameRate = -1;
                    break;
                    case 1:
                    Application.targetFrameRate = 30;
                    break;
                    case 2:
                    Application.targetFrameRate = 60;
                    break;
                    case 3:
                    Application.targetFrameRate = 120;
                    break;
                    case 4:
                    Application.targetFrameRate = 150;
                    break;
                }
            }
            public void UpdateCounter()
            {
                if (!FPSGUI)
                    return;

                if (Frame < FramesCount)
                {
                    Frame++;

                    StoredDelta += Time.deltaTime;
                }
                else if (Frame >= FramesCount && StoredDelta > 0f)
                {
                    Frame = 0;

                    FPSGUI.text = (1f / (StoredDelta / FramesCount)).ToString("0.0");

                    StoredDelta = 0f;
                }
            }
        }

        public void EnableFPSGUI(bool value) => _FPS.SetEnabled(value);
        public void LockFPS(int type) => _FPS.LockFPS(type);
        #endregion

        [Space]
        [SerializeField] protected Logger _Logger;
        #region LOGGER
        [Serializable]
        protected class Logger : WindowBase
        {
            [Space]
            public Type _Type;
            public TMP_Text LogText;

            [Space]
            public int MaxRowCount = 100;

            string Console;

            Log.Message Current;

            public void Update()
            {
                switch (_Type)
                {
                    case Type.Console:
                    AsConsole();
                    break;
                    case Type.PopUp:
                    AsPopUp();
                    break;
                }

                void AsConsole()
                {
                    var list = new List<string>();
                    if (!string.IsNullOrEmpty(Console))
                    {
                        list.AddRange(Console.Split("|", StringSplitOptions.RemoveEmptyEntries));
                        while (list.Count > MaxRowCount)
                            list.RemoveAt(0);
                    }

                    while (Log.Read(out var message))
                    {
                        list.Add($"{message.Text}\n");
                        if (list.Count > MaxRowCount)
                            list.RemoveAt(0);
                    }

                    Console = "";
                    for (int t = 0; t < list.Count; t++)
                        Console += $"{list[t]}|";

                    if (LogText)
                        LogText.text = Console.Replace("|", "");
                }
                void AsPopUp()
                {
                    if (Current != null &&
                         Current.Time != 0f)
                    {
                        if (Current.Time + Current.CallTime < Time.realtimeSinceStartup)
                        {
                            Current.Time = 0f;

                            Log.Read(out Current);

                            RefreshCurrent();
                        }
                    }
                    else if (Log.Read(out Current))
                        RefreshCurrent();

                    void RefreshCurrent()
                    {
                        if (Current == null ||
                              Current.Time == 0f)
                        {
                            LogText.text = "";

                            SetEnabled(false);
                        }
                        else
                        {
                            Current.CallTime = Time.realtimeSinceStartup;
                            LogText.text = Current.Text;

                            SetEnabled(true);
                        }
                    }
                }
            }

            public enum Type : byte
            {
                Console = 0,
                PopUp = 1,

            }
        }
        #endregion

        [Space]
        [SerializeField] protected Confirm _Confirm;
        #region CONFIRMATION
        [Serializable]
        protected class Confirm : WindowBase
        {
            public UnityAction CurrentAction;

            public TMP_Text Text;
            public MenuButton ConfirmButton;
            public MenuButton DeclineButton;

            [Space]
            public string[] Keys;
        }

        void InitConfirmation(int key, UnityAction action)
        {
            //_Confirm.Text.text = GetTranslation(_Confirm.Keys[key]).Text;
            _Confirm.CurrentAction = action;

            _Confirm.ConfirmButton.AddListener(action);
            _Confirm.ConfirmButton.AddListener(ClearConfirmation);
            _Confirm.DeclineButton.AddListener(ClearConfirmation);

            _Confirm.SetEnabled(true);
        }
        void ClearConfirmation()
        {
            _Confirm.Text.text = "";
            _Confirm.CurrentAction = null;

            _Confirm.ConfirmButton.RemoveAllListeners();
            _Confirm.DeclineButton.RemoveAllListeners();
            _Confirm.SetEnabled(false);
        }
        #endregion

        public virtual void Close()
        {
#if UNITY_EDITOR
            EditorApplication.ExitPlaymode();
#else
            Application.Quit();
#endif
        }

        protected virtual void Awake()
        {
#if UNITY_ENTITIES_INSTALLED
            EntityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
#endif
        }
        protected virtual void Update()
        {
            _Logger.Update();
        }
        protected virtual void LateUpdate()
        {
            _FPS.UpdateCounter();
        }
        protected virtual void OnDestroy()
        {

        }

#if UNITY_EDITOR
        protected virtual void OnValidate()
        {

        }
        protected virtual void Reset()
        {
            Core.Tool.CreateTag("UIManager");
            gameObject.tag = "UIManager";

            if (!Directory.Exists(Application.dataPath + "/Resources/UI/Localizations/"))
                Directory.CreateDirectory(Application.dataPath + "/Resources/UI/Localizations/");

            if (!File.Exists(Application.dataPath + "/Resources/UI/Localizations/_default.json"))
                File.Create(Application.dataPath + "/Resources/UI/Localizations/_default.json");
        }
#endif
    }
}