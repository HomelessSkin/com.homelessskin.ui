using TMPro;

using UnityEngine;
using UnityEngine.UI;

namespace Core.UI
{
    public class PlayerInfo : MonoBehaviour
    {
        [SerializeField] Image Avatar;
        [SerializeField] Image Highlight;

        [SerializeField] TMP_Text Rank;
        [SerializeField] TMP_Text Name;
        [SerializeField] TMP_Text Score;

        public Image GetAvatar() => Avatar;
    }
}