using Core.Util;

using Unity.Entities;

using UnityEngine;

namespace Core.UI
{
    public abstract class UIManagerBase : MonoBehaviour
    {
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
    }
}