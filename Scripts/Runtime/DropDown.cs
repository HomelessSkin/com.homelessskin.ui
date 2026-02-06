#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;
using UnityEngine.Events;

namespace UI
{
    public class DropDown : MonoBehaviour
    {
        [SerializeField] string[] Values;
        [Space]
        [SerializeField] MenuButton Selection;
        [Space]
        [SerializeField] GameObject ContentPrefab;
        [SerializeField] GameObject ItemPrefab;
        [Space]
        [SerializeField] UnityEvent<int> OnValueChanged;

        bool IsOpenned;
        int Value = 0;
        Transform Items;

        void Start()
        {
            Selection.AddListener(Open);
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            if (Selection &&
                 PrefabUtility.IsPartOfPrefabAsset(Selection))
            {
                Debug.LogError("Put scene object here!");

                Selection = null;
            }
        }
#endif

        public int GetValue() => Value;
        public void SetValue(int value) => InvChanged(value);

        void Open()
        {
            if (IsOpenned)
            {
                Close();

                return;
            }

            var st = Selection.transform as RectTransform;

            Items = Instantiate(ContentPrefab, Selection.transform).transform;
            (Items.transform as RectTransform)
                .SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, st.rect.width);

            for (int v = 0; v < Values.Length; v++)
                CreateItem(v, st);

            IsOpenned = true;
        }
        void Close()
        {
            IsOpenned = false;

            if (Items)
                Destroy(Items.gameObject);
        }
        void CreateItem(int index, RectTransform st)
        {
            var item = Instantiate(ItemPrefab, Items);

            item
                .GetComponent<MenuButton>()
                .InitAsDropItem(InvChanged, index, Values[index]);

            (item.transform as RectTransform)
                .SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, st.rect.height);
        }
        void InvChanged(int index)
        {
            Close();

            Value = index;
            Selection.SetLabel(Values[index]);
            OnValueChanged.Invoke(index);
        }
    }
}