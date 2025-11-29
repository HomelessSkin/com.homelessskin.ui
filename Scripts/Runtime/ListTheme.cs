using UnityEngine;

namespace UI
{
    public class ListTheme : ScrollItem
    {
        [SerializeField] MenuButton SelectButton;
        [Space]
        [SerializeField] Drawable[] Drawables;

        public override void Init(int index, Storage.Data data, UIManagerBase manager)
        {
            base.Init(index, data, manager);

            var container = data as PersonalizedStorage.Container;

            for (int d = 0; d < Drawables.Length; d++)
                Drawables[d].SetData(GetData(Drawables[d].GetKey()));

            SelectButton.AddListener(() => Manager.SelectTheme(index));

            var drawable = SelectButton.targetGraphic.gameObject.GetComponent<Drawable>();
            drawable.SetData(GetData(drawable.GetKey()));

            Element.Data GetData(string tag)
            {
                Element.Data init = null;
                if (container.Map.TryGetValue(tag, out init))
                { }
                else if (Manager.TryGetDrawerData(tag, out init))
                { }

                return init;
            }
        }
    }
}