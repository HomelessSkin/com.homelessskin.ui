using System;

using UnityEngine;

namespace UI
{
    public class ShowButtonListener : MonoBehaviour
    {
        [SerializeField] bool SkipSetEnabled;
        [SerializeField] Dependency[] DependsOn;

        bool State;

        public void Enable() => gameObject.SetActive(true);
        public void Disable() => gameObject.SetActive(false);
        public void SwitchEnable()
        {
            if (!gameObject.activeInHierarchy)
                Enable();
            else
                Disable();
        }
        public void SwitchEnableWithDependency()
        {
            if (!gameObject.activeInHierarchy)
                EnableWithDependency();
            else
                DisableWithDependency();
        }
        public void EnableWithDependency()
        {
            if (DependsOn != null && DependsOn.Length > 0)
                for (int i = 0; i < DependsOn.Length; i++)
                    if (DependsOn[i].Object.activeSelf != DependsOn[i].MustBe)
                    {
                        gameObject.SetActive(false);

                        return;
                    }

            gameObject.SetActive(true);
        }
        public void DisableWithDependency()
        {
            if (DependsOn != null && DependsOn.Length > 0)
                for (int i = 0; i < DependsOn.Length; i++)
                    if (DependsOn[i].Object.activeSelf != DependsOn[i].MustBe)
                    {
                        gameObject.SetActive(false);

                        return;
                    }

            gameObject.SetActive(false);
        }

        void SetEnabled(bool value)
        {
            if (SkipSetEnabled)
                return;

            if (!value)
            {
                State = gameObject.activeInHierarchy;
                gameObject.SetActive(false);
            }
            else
                gameObject.SetActive(State);
        }

        [Serializable]
        public struct Dependency
        {
            public GameObject Object;
            public bool MustBe;
        }
    }
}