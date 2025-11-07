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

        void Open()
        {
            if (IsOpenned)
                return;

            Items = Instantiate(ContentPrefab, Selection.transform).transform;
            for (int v = 0; v < Values.Length; v++)
                CreateItem(v);

            IsOpenned = true;
        }
        void CreateItem(int index) =>
            Instantiate(ItemPrefab, Items)
            .GetComponent<MenuButton>()
            .InitAsDropItem(InvChanged, index, Values[index]);
        void InvChanged(int index)
        {
            IsOpenned = false;

            Destroy(Items.gameObject);

            Selection.SetLabel(Values[index]);
            OnValueChanged.Invoke(index);
        }
    }
}