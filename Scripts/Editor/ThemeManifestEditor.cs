#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;

using UnityEditor;

using UnityEngine;

namespace UI
{
    public class ThemeManifestEditor : EditorWindow
    {
        string jsonFilePath = "";
        Vector2 scrollPosition;
        Manifest_V1 manifest;
        TextAsset jsonTextAsset;

        List<Manifest_V1.Element> elementsList;

        Dictionary<int, bool> elementFoldoutStates = new Dictionary<int, bool>();
        Dictionary<string, bool> spriteFoldoutStates = new Dictionary<string, bool>();
        Dictionary<string, bool> bordersFoldoutStates = new Dictionary<string, bool>();

        [MenuItem("Tools/Theme Manifest Editor")]
        public static void ShowWindow() => GetWindow<ThemeManifestEditor>("Theme Manifest Editor");

        void OnGUI()
        {
            GUILayout.Space(10);

            EditorGUILayout.BeginVertical("box");
            {
                GUILayout.Label("JSON File Configuration", EditorStyles.boldLabel);

                EditorGUI.BeginChangeCheck();
                jsonTextAsset = (TextAsset)EditorGUILayout.ObjectField("JSON File", jsonTextAsset, typeof(TextAsset), false);
                if (EditorGUI.EndChangeCheck() && jsonTextAsset != null)
                {
                    jsonFilePath = AssetDatabase.GetAssetPath(jsonTextAsset);

                    LoadJSON();
                }

                EditorGUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("Load JSON"))
                        LoadJSONFromFile();

                    if (GUILayout.Button("New JSON"))
                        CreateNewJSON();

                    if (GUILayout.Button("Save JSON"))
                        SaveJSON();

                    if (GUILayout.Button("Save As..."))
                        SaveJSONAs();
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();

            if (manifest == null)
                return;

            GUILayout.Space(10);

            EditorGUILayout.BeginVertical("box");
            {
                GUILayout.Label("Theme Settings", EditorStyles.boldLabel);

                manifest.name = EditorGUILayout.TextField("Theme Name", manifest.name);
                manifest.version = EditorGUILayout.IntField("Version", manifest.version);
            }
            EditorGUILayout.EndVertical();

            GUILayout.Space(10);

            EditorGUILayout.BeginVertical("box");
            {
                GUILayout.Label("Elements", EditorStyles.boldLabel);

                if (elementsList == null)
                    elementsList = manifest.elements != null ? new List<Manifest_V1.Element>(manifest.elements) : new List<Manifest_V1.Element>();

                EditorGUILayout.BeginVertical(GUILayout.ExpandHeight(true));
                {
                    scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.ExpandHeight(true));

                    for (int i = 0; i < elementsList.Count; i++)
                        DrawElementInfo(i);

                    EditorGUILayout.EndScrollView();
                }
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("Add Element"))
                        elementsList.Add(new Manifest_V1.Element()
                        {
                            key = ElementType.Null.ToString(),

                            @base = CreateDefaultSprite(),
                            mask = CreateDefaultSprite(),
                            overlay = CreateDefaultSprite()
                        });

                    if (GUILayout.Button("Remove Last") && elementsList.Count > 0)
                        elementsList.RemoveAt(elementsList.Count - 1);

                    if (GUILayout.Button("Clear All"))
                        if (EditorUtility.DisplayDialog("Clear All", "Are you sure you want to remove all elements?", "Yes", "No"))
                            elementsList.Clear();
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
        }

        void DrawElementInfo(int index)
        {
            var element = elementsList[index];

            if (!elementFoldoutStates.ContainsKey(index))
                elementFoldoutStates[index] = false;

            EditorGUILayout.BeginVertical("box");
            {
                EditorGUILayout.BeginHorizontal();
                {
                    elementFoldoutStates[index] = EditorGUILayout.Foldout(elementFoldoutStates[index], $"Element {index + 1}: {element.key}", true);

                    GUILayout.FlexibleSpace();

                    if (GUILayout.Button("Remove", GUILayout.Width(80)))
                        if (EditorUtility.DisplayDialog("Remove Element", $"Are you sure you want to remove element {element.key}?", "Yes", "No"))
                            elementsList.RemoveAt(index);
                }
                EditorGUILayout.EndHorizontal();

                if (elementFoldoutStates[index])
                {
                    EditorGUILayout.Space();
                    var currentType = (ElementType)Enum.Parse(typeof(ElementType), element.key);
                    currentType = (ElementType)EditorGUILayout.EnumPopup("UI Element Type", currentType);
                    element.key = currentType.ToString();
                    EditorGUILayout.Space();

                    DrawSpriteFoldout(ref element.@base, "Base", $"{index}_base");
                    EditorGUILayout.Space();

                    DrawSpriteFoldout(ref element.mask, "Mask", $"{index}_mask");
                    EditorGUILayout.Space();

                    DrawSpriteFoldout(ref element.overlay, "Overlay", $"{index}_overlay");
                }
            }
            EditorGUILayout.EndVertical();

            GUILayout.Space(5);
        }
        void DrawSpriteFoldout(ref Manifest_V1.Element.Sprite sprite, string prefix, string uniqueKey)
        {
            if (sprite == null)
                sprite = CreateDefaultSprite();

            if (!spriteFoldoutStates.ContainsKey(uniqueKey))
                spriteFoldoutStates[uniqueKey] = false;
            if (!bordersFoldoutStates.ContainsKey(uniqueKey))
                bordersFoldoutStates[uniqueKey] = false;

            spriteFoldoutStates[uniqueKey] = EditorGUILayout.Foldout(spriteFoldoutStates[uniqueKey], $"{prefix} Sprite", true);

            if (spriteFoldoutStates[uniqueKey])
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.Space();

                var currentFilter = (FilterMode)sprite.filterMode;
                currentFilter = (FilterMode)EditorGUILayout.EnumPopup("Filter Mode", currentFilter);
                sprite.filterMode = (int)currentFilter;

                EditorGUILayout.BeginHorizontal();
                {
                    sprite.fileName = EditorGUILayout.TextField("File Name", sprite.fileName);

                    var dropArea = GUILayoutUtility.GetRect(0, 20, GUILayout.Width(60));
                    GUI.Box(dropArea, "Drag");

                    HandleDragAndDrop(dropArea, ref sprite);
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Select File", GUILayout.Width(100)))
                    {
                        var path = EditorUtility.OpenFilePanel($"Select {prefix.ToLower()} sprite file", Application.dataPath, "png,jpg,jpeg");
                        if (!string.IsNullOrEmpty(path))
                            sprite.fileName = Path.GetFileName(path);
                    }
                }
                EditorGUILayout.EndHorizontal();

                sprite.pixelPerUnit = EditorGUILayout.IntField("Pixel Per Unit", sprite.pixelPerUnit);

                EditorGUILayout.Space();

                bordersFoldoutStates[uniqueKey] = EditorGUILayout.Foldout(bordersFoldoutStates[uniqueKey], "Borders", true);

                if (bordersFoldoutStates[uniqueKey])
                {
                    EditorGUI.indentLevel++;

                    if (sprite.borders == null)
                        sprite.borders = new Manifest_V1.Borders();

                    EditorGUILayout.Space();

                    sprite.borders.left = EditorGUILayout.IntField("Left", sprite.borders.left);
                    sprite.borders.right = EditorGUILayout.IntField("Right", sprite.borders.right);
                    sprite.borders.top = EditorGUILayout.IntField("Top", sprite.borders.top);
                    sprite.borders.bottom = EditorGUILayout.IntField("Bottom", sprite.borders.bottom);

                    EditorGUI.indentLevel--;
                }

                EditorGUI.indentLevel--;
            }
        }
        void HandleDragAndDrop(Rect dropArea, ref Manifest_V1.Element.Sprite sprite)
        {
            var evt = Event.current;
            if (dropArea.Contains(evt.mousePosition))
            {
                if (evt.type == EventType.DragUpdated)
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    evt.Use();
                }
                else if (evt.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();

                    foreach (UnityEngine.Object draggedObject in DragAndDrop.objectReferences)
                    {
                        if (draggedObject is Texture2D)
                        {
                            sprite.fileName = draggedObject.name;
                            break;
                        }
                        else if (draggedObject is Sprite)
                        {
                            sprite.fileName = draggedObject.name;

                            var s = draggedObject as Sprite;
                            sprite.borders = new Manifest_V1.Borders
                            {
                                left = (int)s.border.x,
                                right = (int)s.border.y,
                                top = (int)s.border.z,
                                bottom = (int)s.border.w
                            };
                            break;
                        }
                    }

                    evt.Use();
                }
            }
        }
        void LoadJSONFromFile()
        {
            string path = EditorUtility.OpenFilePanel("Select JSON file", Application.dataPath, "json");
            if (!string.IsNullOrEmpty(path))
            {
                jsonFilePath = path;
                LoadJSON();
            }
        }
        void LoadJSON()
        {
            if (string.IsNullOrEmpty(jsonFilePath) || !File.Exists(jsonFilePath))
                return;

            var json = File.ReadAllText(jsonFilePath);
            manifest = Manifest.Cast(json);

            elementsList = manifest.elements != null ? new List<Manifest_V1.Element>(manifest.elements) : new List<Manifest_V1.Element>();

            foreach (var element in elementsList)
            {
                element.@base = EnsureSpriteBorders(element.@base);
                element.mask = EnsureSpriteBorders(element.mask);
                element.overlay = EnsureSpriteBorders(element.overlay);
            }

            Repaint();
        }
        void SaveJSON()
        {
            if (string.IsNullOrEmpty(jsonFilePath))
            {
                SaveJSONAs();

                return;
            }

            manifest.elements = elementsList.ToArray();

            var json = JsonUtility.ToJson(manifest, true);
            File.WriteAllText(jsonFilePath, json);
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Success", "JSON file saved successfully!", "OK");
        }
        void SaveJSONAs()
        {
            var path = EditorUtility.SaveFilePanel("Save JSON file", Application.dataPath, $"{manifest.name}_theme", "json");
            if (!string.IsNullOrEmpty(path))
            {
                jsonFilePath = path;

                SaveJSON();
            }
        }
        void CreateNewJSON()
        {
            manifest = new Manifest_V1
            {
                version = 1,
                name = "NewTheme",
                elements = new Manifest_V1.Element[0]
            };

            elementsList = new List<Manifest_V1.Element>();
            jsonFilePath = "";
            jsonTextAsset = null;
        }

        Manifest_V1.Element.Sprite CreateDefaultSprite() =>
            new Manifest_V1.Element.Sprite
            {
                pixelPerUnit = 100,
                filterMode = 1,
                borders = new Manifest_V1.Borders()
            };
        Manifest_V1.Element.Sprite EnsureSpriteBorders(Manifest_V1.Element.Sprite sprite)
        {
            if (sprite != null &&
                 sprite.borders == null)
                sprite.borders = new Manifest_V1.Borders();

            return sprite;
        }
    }
}
#endif