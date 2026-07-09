using UnityEngine;

namespace UI
{
    public class Saver : MonoBehaviour
    {
        [SerializeField] Savable[] Savables;

        void Start()
        {
            if (Savables != null)
                for (int s = 0; s < Savables.Length; s++)
                    Savables[s].Load();
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            Savables = GameObject.FindObjectsByType<Savable>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        }
#endif
    }
}