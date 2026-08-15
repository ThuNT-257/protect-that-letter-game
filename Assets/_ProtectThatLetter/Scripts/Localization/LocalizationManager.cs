using System;
using System.Collections.Generic;
using UnityEngine;

public class LocalizationManager : MonoBehaviour
{
    private static LocalizationManager instance;

    public static LocalizationManager Instance
    {
        get
        {
            if(instance == null)
            {
                instance = FindAnyObjectByType<LocalizationManager>();
                if (instance == null)
                {
                    Debug.LogError("There is no LocalizationManager in Scene.");
                }
            }
            return instance;
        }
    }

    private Dictionary<string, string> localizedText = new Dictionary<string, string>();

    public string CurrentLanguage { get; private set; } = "vi";

    public static event Action OnLanguageChanged;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        instance = this;
        LoadLanguage(CurrentLanguage);
    }

    public void LoadLanguage(string langCode)
    {
        CurrentLanguage = langCode;
        TextAsset textAsset = Resources.Load<TextAsset>($"Localization/{langCode}");
        if (textAsset != null)
        {
            LocalizationData data = JsonUtility.FromJson<LocalizationData>(textAsset.text);

            localizedText.Clear();
            if(data != null && data.items != null)
            {
                foreach(LocalizationItem item in data.items)
                {
                    localizedText[item.key] = item.value;
                }
            }

            OnLanguageChanged?.Invoke();
        } else
        {
            Debug.LogError($"[LocalizationManager] - Load Language - JSON File Not Found in Resources/Localization/{langCode}");
        }

    }

    public string GetText(string key)
    {
        if (localizedText.TryGetValue(key, out string text))
        {
            return text;
        }
        return $"[LocalizationManager] - Get Text - {key}";
    }

    public void SwitchLanguage(string langCode)
    {
        if(CurrentLanguage != langCode)
        {
            LoadLanguage(langCode);
        }
    }

    [Serializable]
    private class LocalizationItem
    {
        public string key;
        public string value;
    }

    [Serializable]
    private class LocalizationData
    {
        public List<LocalizationItem> items;
    }


}
