using Core.Util;

using Unity.Entities;

using UnityEngine;

namespace UI
{
    public abstract class UIManagerBase : MonoBehaviour
    {
        [SerializeField] protected LocalizationManager Localizator;
        [SerializeField] protected MessageManager Messenger;

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

        public void AddMessage(int index, float time = 5f, MessageManager.AdditionType addition = MessageManager.AdditionType.Null) => Messenger.AddMessage(index, time, addition);
    }
}