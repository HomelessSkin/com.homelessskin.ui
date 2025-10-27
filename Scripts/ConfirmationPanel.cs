using TMPro;

using UnityEngine;
using UnityEngine.Events;

namespace Core.UI
{
    public class ConfirmationPanel : MonoBehaviour
    {
        [SerializeField] LocalizationManager Localizator;
        [SerializeField] TMP_Text TargetText;
        [SerializeField] MenuButton ConfirmButton;
        [SerializeField] MenuButton DeclineButton;
        [SerializeField] string[] Keys;

        UnityAction CurrentAction;

        public void Init(int key, UnityAction action)
        {
            TargetText.text = Localizator.GetString(Keys[key]);
            CurrentAction = action;

            ConfirmButton.AddListener(action);
            ConfirmButton.AddListener(Clear);
            DeclineButton.AddListener(Clear);
            gameObject.SetActive(true);
        }
        public void Clear()
        {
            TargetText.text = "";
            CurrentAction = null;

            ConfirmButton.RemoveAllListeners();
            DeclineButton.RemoveAllListeners();
            gameObject.SetActive(false);
        }

#if UNITY_EDITOR
        public string[] GetKeys() => Keys;
#endif
    }
}