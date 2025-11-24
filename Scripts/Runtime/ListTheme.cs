using UnityEngine;

namespace UI
{
    public class ListTheme : ScrollItem
    {
        [SerializeField] MenuButton SelectButton;
        [Space]
        [SerializeField] Drawable[] Drawables;

        public override void Init(int index, IElementData data, UIManagerBase manager)
        {
            base.Init(index, data, manager);

            var theme = data as Theme;
            for (int d = 0; d < Drawables.Length; d++)
                Drawables[d].SetData(GetData(Drawables[d].GetKey()));

            SelectButton.AddListener(() => Manager.SelectTheme(index));
            SelectButton
                .targetGraphic
                .gameObject
                .GetComponent<Drawable>()
                .SetData(GetData("Menu_Button"));

            Drawable.DrawData GetData(string tag)
            {
                Drawable.DrawData data = null;
                if (theme.Sprites.TryGetValue(tag, out data))
                { }
                else if (Manager.TryGetData(tag, out data))
                { }

                return data;
            }
        }
    }
}