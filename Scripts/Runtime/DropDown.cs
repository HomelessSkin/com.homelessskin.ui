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
            Selection.SetLabel(Values[0]);
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
        public void SetValue(int value) => Value = value;

        void Open()
        {
            if (IsOpenned)
            {
                Close();

                return;
            }

            Items = Instantiate(ContentPrefab, Selection.transform).transform;
            for (int v = 0; v < Values.Length; v++)
                CreateItem(v);

            IsOpenned = true;
        }
        void Close()
        {
            IsOpenned = false;

            Destroy(Items.gameObject);
        }
        void CreateItem(int index) =>
            Instantiate(ItemPrefab, Items)
            .GetComponent<MenuButton>()
            .InitAsDropItem(InvChanged, index, Values[index]);
        void InvChanged(int index)
        {
            Close();
            SetValue(index);

            Selection.SetLabel(Values[index]);
            OnValueChanged.Invoke(index);
        }
    }
}