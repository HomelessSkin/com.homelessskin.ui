using TMPro;

using UnityEngine;

namespace UI
{
    public class ListTheme : ScrollItem
    {
        [SerializeField] MenuButton SelectButton;
        [Space]
        [SerializeField] Drawable[] Drawables;

        public override void Init(int index, IInitData data, UIManagerBase manager)
        {
            base.Init(index, data, manager);

            var theme = (Theme)data;
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