using UnityEngine;

namespace UI
{
    public class ListLocalization : ScrollItem
    {
        [SerializeField] MenuButton SelectButton;

        public override void Init(int index, Storage.Data data, UIManagerBase manager)
        {
            base.Init(index, data, manager);

            SelectButton.SetLabel(data.Name);
            SelectButton.AddListener(() => manager.SelectLanguage(data.Name));
        }
    }
}