#if UNITY_EDITOR
using UnityEditor;

using UnityEngine;

namespace Core.UI
{
    [CustomEditor(typeof(LocalizationManager))]
    public class LocalizationManagerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var manager = (LocalizationManager)target;
            if (target == null)
                return;

            if (GUILayout.Button("Reload"))
                manager.Reload();

            base.OnInspectorGUI();
        }
    }
}
#endif