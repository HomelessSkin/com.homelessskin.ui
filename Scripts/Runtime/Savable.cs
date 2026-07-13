using UnityEngine;
using UnityEngine.Events;

namespace UI
{
    public class Savable : MonoBehaviour
    {
        [SerializeField] string PrefName;
        [SerializeField] string DefaultValue;

        [Space]
        [SerializeField] UnityEvent<bool> OnLoadBool;
        [SerializeField] UnityEvent<int> OnLoadInt;
        [SerializeField] UnityEvent<float> OnLoadFloat;
        [SerializeField] UnityEvent<string> OnLoadString;

        public void Load()
        {
            if (OnLoadBool != null &&
                 OnLoadBool.GetPersistentEventCount() > 0)
            {
                if (!PlayerPrefs.HasKey(PrefName) && int.TryParse(DefaultValue, out var result))
                    OnLoadBool.Invoke(result == 1);
                else
                    OnLoadBool.Invoke(PlayerPrefs.GetInt(PrefName) == 1);
            }

            if (OnLoadInt != null &&
                 OnLoadInt.GetPersistentEventCount() > 0)
            {
                if (!PlayerPrefs.HasKey(PrefName) && int.TryParse(DefaultValue, out var result))
                    OnLoadInt.Invoke(result);
                else
                    OnLoadInt.Invoke(PlayerPrefs.GetInt(PrefName));
            }

            if (OnLoadFloat != null &&
                 OnLoadFloat.GetPersistentEventCount() > 0)
            {
                if (!PlayerPrefs.HasKey(PrefName) && float.TryParse(DefaultValue, out var result))
                    OnLoadFloat.Invoke(result);
                else
                    OnLoadFloat.Invoke(PlayerPrefs.GetFloat(PrefName));
            }

            if (OnLoadString != null &&
                 OnLoadString.GetPersistentEventCount() > 0)
            {
                if (!PlayerPrefs.HasKey(PrefName))
                    OnLoadString.Invoke(DefaultValue);
                else
                    OnLoadString.Invoke(PlayerPrefs.GetString(PrefName));
            }
        }

        public void SaveBool(bool value) => PlayerPrefs.SetInt(PrefName, value ? 1 : 0);
        public void SwitchBool()
        {
            var value = false;
            if (PlayerPrefs.HasKey(PrefName))
                value = PlayerPrefs.GetInt(PrefName) == 1;
            else if (int.TryParse(DefaultValue, out var result))
                value = result == 1;

            PlayerPrefs.SetInt(PrefName, value ? 0 : 1);
        }
        public void SaveInt(int value) => PlayerPrefs.SetInt(PrefName, value);
        public void SaveFloat(float value) => PlayerPrefs.SetFloat(PrefName, value);
        public void SaveString(string value) => PlayerPrefs.SetString(PrefName, value);
    }
}