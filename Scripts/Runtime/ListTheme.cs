using UnityEngine;

namespace UI
{
    public class ListTheme : ScrollItem
    {
        [SerializeField] MenuButton SelectButton;
        [Space]
        [SerializeField] Drawable[] Drawables;

        public override void Init(int index, ScrollBase.Container data, UIManagerBase manager)
        {
            base.Init(index, data, manager);

            for (int d = 0; d < Drawables.Length; d++)
                Drawables[d].SetData(GetData(Drawables[d].GetKey()));

            SelectButton.AddListener(() => Manager.SelectTheme(index));

            var drawable = SelectButton.targetGraphic.gameObject.GetComponent<Drawable>();
            drawable.SetData(GetData(drawable.GetKey()));

            Element.Data GetData(string tag)
            {
                Element.Data init = null;
                if (data.Map.TryGetValue(tag, out init))
                { }
                else if (Manager.TryGetDrawData(tag, out init))
                { }

                return init;
            }
        }
    }
}