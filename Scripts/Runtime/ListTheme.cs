using TMPro;

using UnityEngine;

namespace UI
{
    public class ListTheme : MonoBehaviour
    {
        [SerializeField] TMP_Text Name;
        [SerializeField] MenuButton SelectButton;
        [Space]
        [SerializeField] Drawable[] Drawables;

        UIManagerBase Manager;

        public void Init(int index, Theme theme, UIManagerBase manager)
        {
            Manager = manager;
            Name.text = theme.Name;

            for (int d = 0; d < Drawables.Length; d++)
                Drawables[d].SetValue(GetData(Drawables[d].GetKey()));

            SelectButton.AddListener(() => Manager.SelectTheme(index));
            SelectButton
                .targetGraphic
                .gameObject
                .GetComponent<Drawable>()
                .SetValue(GetData("Menu_Button"));

            Drawable.Data GetData(string tag)
            {
                Drawable.Data data = null;
                if (theme.Sprites.TryGetValue(tag, out data))
                { }
                else if (Manager.TryGetData(tag, out data))
                { }

                return data;
            }
        }
    }
}