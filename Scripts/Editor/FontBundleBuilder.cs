#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;

using UnityEditor;

using UnityEngine;

namespace UI
{
    public class FontBundleBuilder : EditorWindow
    {
        Vector2 scrollPosition;

        List<string> availableBundles = new List<string>();
        Dictionary<string, bool> selectedBundles = new Dictionary<string, bool>();

        [MenuItem("Tools/Build Bundles")]
        static void ShowWindow()
        {
            GetWindow<FontBundleBuilder>("Bundles Builder");
        }

        void OnEnable()
        {
            RefreshAvailableBundles();
        }

        void RefreshAvailableBundles()
        {
            availableBundles.Clear();
            selectedBundles.Clear();

            var allAssetPaths = AssetDatabase.GetAllAssetPaths();
            foreach (var assetPath in allAssetPaths)
            {
                var importer = AssetImporter.GetAtPath(assetPath);
                if (importer != null && !string.IsNullOrEmpty(importer.assetBundleName))
                    if (!availableBundles.Contains(importer.assetBundleName))
                    {
                        availableBundles.Add(importer.assetBundleName);
                        selectedBundles[importer.assetBundleName] = true;
                    }
            }

            availableBundles.Sort();
        }

        void OnGUI()
        {
            var path = Application.dataPath + "/Bundles";
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            GUILayout.Label("Select Bundles to Build", EditorStyles.boldLabel);

            if (GUILayout.Button("Refresh Bundle List"))
                RefreshAvailableBundles();

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(200));

            foreach (var bundleName in availableBundles)
                selectedBundles[bundleName] = EditorGUILayout.ToggleLeft(bundleName, selectedBundles[bundleName]);

            EditorGUILayout.EndScrollView();

            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Select All"))
                    foreach (var bundle in availableBundles)
                        selectedBundles[bundle] = true;

                if (GUILayout.Button("Select None"))
                    foreach (var bundle in availableBundles)
                        selectedBundles[bundle] = false;
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            if (GUILayout.Button("Build Selected Bundles"))
                BuildSelectedBundles();

            if (GUILayout.Button("Build All Bundles"))
                BuildPipeline.BuildAssetBundles("Assets/Bundles/", BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows);
        }

        void BuildSelectedBundles()
        {
            var originalBundleSettings = new Dictionary<string, string>();
            foreach (var bundleName in availableBundles)
                if (!selectedBundles[bundleName])
                {
                    var allAssetPaths = AssetDatabase.GetAllAssetPaths();
                    foreach (var assetPath in allAssetPaths)
                    {
                        var importer = AssetImporter.GetAtPath(assetPath);
                        if (importer != null && importer.assetBundleName == bundleName)
                        {
                            originalBundleSettings[assetPath] = importer.assetBundleName;
                            importer.assetBundleName = null;
                        }
                    }
                }

            try
            {
                BuildPipeline.BuildAssetBundles("Assets/Bundles/", BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows);
            }
            finally
            {
                foreach (var kvp in originalBundleSettings)
                {
                    var importer = AssetImporter.GetAtPath(kvp.Key);
                    if (importer != null)
                        importer.assetBundleName = kvp.Value;
                }

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            EditorUtility.DisplayDialog("Build Complete", "Selected bundles built successfully!", "OK");
        }
    }
}
#endif