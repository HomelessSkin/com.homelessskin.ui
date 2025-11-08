using Core.Util;

using Unity.Entities;

using UnityEngine;

namespace UI
{
    public abstract class UIManagerBase : MonoBehaviour
    {
        [SerializeField] protected LocalizationManager Localizator;
        [SerializeField] protected MessagePanel Messenger;

        protected EntityManager EntityManager;

        protected virtual void Start()
        {
            EntityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        }
#if UNITY_EDITOR
        void Reset()
        {
            Tool.CreateTag("UIManager");
            gameObject.tag = "UIManager";
        }
#endif
        void OnDestroy()
        {
            StopAllCoroutines();
        }

        internal void AddMessage(int index, float time = 5f, MessagePanel.AdditionType addition = MessagePanel.AdditionType.Null) => Messenger.AddMessage(index, time, addition);
    }
}