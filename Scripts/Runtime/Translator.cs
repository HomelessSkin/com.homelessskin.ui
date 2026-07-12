using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;


#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;

namespace UI
{
    public class Translator : MonoBehaviour
    {
        [SerializeField] int Index;
        [SerializeField] HorizontalScrollSelection Scroll;

        [Space]
        [SerializeField] List<TranslationPath> Translations = new List<TranslationPath>();

        [Space]
        [SerializeField] Translatable[] Translatables;

        Task Waiting;

        void Start()
        {
            Waiting = Task.Delay(0);
        }

        public async void SetIndex(int value)
        {
            Index = value;

            Scroll.Init(Translations[Index].Name);

            await Waiting;
            Waiting = Translate();
        }
        public void IncrementIndex()
        {
            if (!Waiting.IsCompleted || Scroll.IsJuggling())
                return;

            Index = (Index + 1) % Translations.Count;
            GetComponent<Savable>().SaveInt(Index);

            Scroll.ScrollLeft(Translations[Index].Name);

            Waiting = Translate();
        }
        public void DecrementIndex()
        {
            if (!Waiting.IsCompleted || Scroll.IsJuggling())
                return;

            Index = (Index + Translations.Count + 1) % Translations.Count;
            GetComponent<Savable>().SaveInt(Index);

            Scroll.ScrollRight(Translations[Index].Name);

            Waiting = Translate();
        }

        async Task Translate()
        {
            List<string> lines = null;
            var dict = new Dictionary<string, string>();
            if (Index >= 0 && Index < Translations.Count)
            {
                var translation = Translations[Index];
                if (translation.FromResources && !string.IsNullOrEmpty(translation.Path))
                    lines = Resources.Load<ScriptableTranslation>(translation.Path).Translation.Data;
                else if (File.Exists(translation.Path))
                {
                    var arr = await File.ReadAllLinesAsync(translation.Path);

                    lines = arr.ToList();
                }
            }

            if (lines != null)
                for (int l = 0; l < lines.Count; l++)
                {
                    var kv = lines[l].Split(";");
                    if (kv.Length == 2)
                        dict[kv[0].Trim()] = kv[1].Trim();
                }

            for (int t = 0; t < Translatables.Length; t++)
                Translatables[t].Translate(dict);
        }

        [Serializable]
        public class Translation
        {
            public List<string> Data;
        }

        [Serializable]
        class TranslationPath
        {
            public string Name;

            public bool FromResources;
            public string Path;
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            Translatables = GameObject.FindObjectsByType<Translatable>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        }

        [ContextMenu("Test")]
        async void Test() => await Translate();

        [ContextMenu("Reload")]
        void Reload()
        {
            if (Translatables == null || Translatables.Length == 0)
                return;

            var keys = new List<string>();
            for (int tt = 0; tt < Translatables.Length; tt++)
                keys.Add(Translatables[tt].GetKey());

            for (int tl = 0; tl < Translations.Count; tl++)
            {
                var translation = Resources.Load<ScriptableTranslation>(Translations[tl].Path);
                if (!translation)
                    continue;

                if (translation.Translation.Data == null)
                    translation.Translation.Data = new List<string>();

                for (int k = 0; k < keys.Count; k++)
                {
                    var key = keys[k];

                    var found = false;
                    for (int d = 0; d < translation.Translation.Data.Count; d++)
                    {
                        if (translation.Translation.Data[d].Contains(key))
                        {
                            found = true;

                            break;
                        }
                    }

                    if (!found)
                        translation.Translation.Data.Add(keys[k]);
                }

                EditorUtility.SetDirty(translation);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
#endif
    }
}