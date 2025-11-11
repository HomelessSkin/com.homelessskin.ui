using TMPro;

using UI;

using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class ListTheme : MonoBehaviour
    {
        [SerializeField] TMP_Text Name;
        [SerializeField] Image TextPreview;
        [SerializeField] Image DarkFieldPreview;
        [SerializeField] Image LightFieldPreview;
        [SerializeField] Image ChatMessagePreview;
        [SerializeField] MenuButton Button;

        UIManagerBase Manager;

        public void Init(int index, Theme theme, UIManagerBase manager)
        {
            Manager = manager;

            Name.text = theme.Name;
            TextPreview.sprite = GetSprite("Text");
            DarkFieldPreview.sprite = GetSprite("Field_Dark");
            LightFieldPreview.sprite = GetSprite("Field_Light");
            ChatMessagePreview.sprite = GetSprite("Chat_Message");
            Button.image.sprite = GetSprite("Menu_Button");

            Button.AddListener(() => Manager.SelectTheme(index));

            Sprite GetSprite(string tag)
            {
                Sprite sprite = null;
                if (theme.Sprites.TryGetValue(tag, out sprite))
                { }
                else if (Manager.TryGetSprite(tag, out sprite))
                { }

                return sprite;
            }
        }
    }
}