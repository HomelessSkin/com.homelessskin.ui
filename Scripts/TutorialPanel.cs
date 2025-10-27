using TMPro;

using UnityEngine;

namespace Core.UI
{
    public class TutorialPanel : MonoBehaviour
    {
        [SerializeField] LocalizationManager Localizator;
        [SerializeField] TMP_Text TutorialText;
        [SerializeField] string TutorialKey;
        [SerializeField] string TutorialValue;

        public void Show()
        {
            gameObject.SetActive(true);
            var variant = Localizator.GetString(TutorialKey);
            TutorialText.text = variant;
        }
        public void Hide()
        {
            gameObject.SetActive(false);
        }

#if UNITY_EDITOR
        public string GetKey() => TutorialKey;
        public string GetDefault() => TutorialValue;
#endif
    }
}