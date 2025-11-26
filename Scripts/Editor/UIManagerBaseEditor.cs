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


            if (GUILayout.Button("Reload"))
            {
                manager.SendMessage("OnValidate");
                manager.Reload();
            }

            base.OnInspectorGUI();
        }
    }
}
#endif