using System;

using UnityEngine;

namespace UI
{
    public abstract class Element : MonoBehaviour
    {
        [SerializeField] protected ElementType _Type;
        [SerializeField] protected string Key;

        protected UIManagerBase UIManager;

        protected virtual void Start()
        {
            UIManager = GameObject
                .FindGameObjectWithTag("UIManager")
                .GetComponent<UIManagerBase>();
        }

        public virtual string GetKey() => Key;
        public ElementType GetElementType() => _Type;

        public abstract void SetData(Data data);

        [Serializable]
        public abstract class Data { }
    }
}