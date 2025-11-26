#if UNITY_EDITOR
using UnityEditor;

using UnityEngine;

namespace UI
{
    [CustomEditor(typeof(UIManagerBase), true)]
    public class UIManagerBaseEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var manager = (UIManagerBase)target;
            if (target == null)
                return;

            manager.SendMessage("OnValidate");

            if (GUILayout.Button("Reload"))
                manager.Reload();

            base.OnInspectorGUI();
        }
    }
}
#endif