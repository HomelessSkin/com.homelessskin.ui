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
        Theme.Manifest manifest;
        TextAsset jsonTextAsset;

        List<Theme.Manifest.Sprite> spritesList;

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
            }
            EditorGUILayout.EndVertical();

            GUILayout.Space(10);

            EditorGUILayout.BeginVertical("box");
            {
                GUILayout.Label("Sprites", EditorStyles.boldLabel);

                if (spritesList == null)
                    spritesList = manifest.sprites != null ? new List<Theme.Manifest.Sprite>(manifest.sprites) : new List<Theme.Manifest.Sprite>();

                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(300));

                for (int i = 0; i < spritesList.Count; i++)
                    DrawSpriteInfo(i);

                EditorGUILayout.EndScrollView();

                EditorGUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("Add Sprite"))
                        spritesList.Add(new Theme.Manifest.Sprite()
                        {
                            key = UIElement.Type.Button.ToString(),
                            pixelPerUnit = 100,
                            borders = new Theme.Manifest.Sprite.Borders()
                        });

                    if (GUILayout.Button("Remove Last") && spritesList.Count > 0)
                        spritesList.RemoveAt(spritesList.Count - 1);

                    if (GUILayout.Button("Clear All"))
                        if (EditorUtility.DisplayDialog("Clear All", "Are you sure you want to remove all sprites?", "Yes", "No"))
                            spritesList.Clear();
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
        }

        void DrawSpriteInfo(int index)
        {
            var sprite = spritesList[index];

            EditorGUILayout.BeginVertical("box");
            {
                EditorGUILayout.LabelField($"Sprite {index + 1}", EditorStyles.boldLabel);

                var currentType = UIElement.Type.Null;
                if (!string.IsNullOrEmpty(sprite.key))
                {
                    try
                    {
                        currentType = (UIElement.Type)Enum.Parse(typeof(UIElement.Type), sprite.key);
                    }
                    catch
                    {

                    }
                }

                currentType = (UIElement.Type)EditorGUILayout.EnumPopup("UI Element Type", currentType);
                sprite.key = currentType.ToString();

                EditorGUILayout.BeginHorizontal();
                {
                    sprite.fileName = EditorGUILayout.TextField("File Name", sprite.fileName);

                    var dropArea = GUILayoutUtility.GetRect(0, 20, GUILayout.Width(60));
                    GUI.Box(dropArea, "Drag");

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
                                    sprite.borders = new Theme.Manifest.Sprite.Borders
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
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Select File", GUILayout.Width(100)))
                    {
                        var path = EditorUtility.OpenFilePanel("Select sprite file", Application.dataPath, "png,jpg,jpeg");
                        if (!string.IsNullOrEmpty(path))
                            sprite.fileName = Path.GetFileName(path);
                    }
                }
                EditorGUILayout.EndHorizontal();

                sprite.pixelPerUnit = EditorGUILayout.IntField("Pixel Per Unit", sprite.pixelPerUnit);

                EditorGUILayout.LabelField("Borders", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;

                if (sprite.borders == null)
                    sprite.borders = new Theme.Manifest.Sprite.Borders();

                sprite.borders.left = EditorGUILayout.IntField("Left", sprite.borders.left);
                sprite.borders.right = EditorGUILayout.IntField("Right", sprite.borders.right);
                sprite.borders.top = EditorGUILayout.IntField("Top", sprite.borders.top);
                sprite.borders.bottom = EditorGUILayout.IntField("Bottom", sprite.borders.bottom);

                EditorGUI.indentLevel--;

                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Remove This Sprite", GUILayout.Width(150)))
                        if (EditorUtility.DisplayDialog("Remove Sprite",
                            $"Are you sure you want to remove sprite {sprite.key}?", "Yes", "No"))
                            spritesList.RemoveAt(index);
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();

            GUILayout.Space(5);
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

            try
            {
                var json = File.ReadAllText(jsonFilePath);
                manifest = JsonUtility.FromJson<Theme.Manifest>(json);

                spritesList = manifest.sprites != null ? new List<Theme.Manifest.Sprite>(manifest.sprites) : new List<Theme.Manifest.Sprite>();

                foreach (var sprite in spritesList)
                    if (sprite.borders == null)
                        sprite.borders = new Theme.Manifest.Sprite.Borders();

                Repaint();
            }
            catch (Exception e)
            {
                EditorUtility.DisplayDialog("Error", $"Failed to load JSON: {e.Message}", "OK");
            }
        }
        void SaveJSON()
        {
            if (string.IsNullOrEmpty(jsonFilePath))
            {
                SaveJSONAs();

                return;
            }

            try
            {
                manifest.sprites = spritesList.ToArray();

                var json = JsonUtility.ToJson(manifest, true);
                File.WriteAllText(jsonFilePath, json);
                AssetDatabase.Refresh();
                EditorUtility.DisplayDialog("Success", "JSON file saved successfully!", "OK");
            }
            catch (Exception e)
            {
                EditorUtility.DisplayDialog("Error", $"Failed to save JSON: {e.Message}", "OK");
            }
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
            manifest = new Theme.Manifest
            {
                name = "NewTheme",
                sprites = new Theme.Manifest.Sprite[0]
            };
            spritesList = new List<Theme.Manifest.Sprite>();
            jsonFilePath = "";
            jsonTextAsset = null;
        }
    }
}
#endif