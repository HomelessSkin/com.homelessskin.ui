using System;

namespace UI
{
    public class Localization : PersonalizedStorage.Container
    {
        public Localization(string lang)
        {
            Name = lang;
        }
        public Localization(Data data)
        {
            Name = data.name;

            for (int k = 0; k < data.dictionary.Length; k++)
            {
                var kvp = data.dictionary[k];

                Map[kvp.key] = new Localizable.LocalData { Text = kvp.value };
            }
        }

        [Serializable]
        public class Data
        {
            public string name;
            public KVP[] dictionary;

            [Serializable]
            public class KVP
            {
                public string key;
                public string value;
            }
        }
    }
}