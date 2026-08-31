using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages localization (multi-language support) for the game.
/// </summary>
public class LocalizationManager : MonoBehaviour
{
    #region Constants
    public const string VIETNAMESE = "vi";
    public const string ENGLISH = "en";

    private const string LANGUAGE_KEY = "SelectedLanguage";
    #endregion

    #region Instance
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
    #endregion

    #region Private Fields
    //dictionary to store json key-value pairs for localized text
    private Dictionary<string, string> localizedText = new Dictionary<string, string>();
    #endregion

    #region Properties
    //currently active language (default is Vietnamese)
    public string CurrentLanguage { get; private set; } = LocalizationManager.VIETNAMESE;
    #endregion

    #region Events
    //trigger whenever the language changes
    public static event Action OnLanguageChanged;
    #endregion

    #region Lifecycle
    /// <summary>
    /// Ensures to loads the default language.
    /// </summary>
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        instance = this;

        string savedLang = PlayerPrefs.GetString(LANGUAGE_KEY, VIETNAMESE);
        LoadLanguage(savedLang);

        DontDestroyOnLoad(gameObject);
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Get the translated text by given key.
    /// </summary>
    /// <param name="key">The identifier key</param>
    /// <returns>Translated text, or an error message if the key is not found(in case)</returns>
    public string GetText(string key)
    {
        if (localizedText.TryGetValue(key, out string text))
        {
            return text;
        }
        return $"[LocalizationManager] - Get Text - {key}";
    }

    /// <summary>
    /// Switches the current language.
    /// Only reloads if the new language differs from the current one.
    /// </summary>
    /// <param name="langCode">New language code</param>
    public void SwitchLanguage(string langCode)
    {
        if(CurrentLanguage != langCode)
        {
            LoadLanguage(langCode);
        }
    }
    #endregion

    #region Private Methods
    /// <summary>
    /// Loads localization data from JSON file located in Resources/Localization/
    /// </summary>
    /// <param name="langCode">Language code (e.g., "vi", "en")</param>
    private void LoadLanguage(string langCode) 
    {
        CurrentLanguage = langCode;

        //save to PlayerPrefs
        PlayerPrefs.SetString(LANGUAGE_KEY, langCode);
        PlayerPrefs.Save();

        //load JSON file from folder
        TextAsset textAsset = Resources.Load<TextAsset>($"Localization/{langCode}");
        if (textAsset != null) 
        {
            //deserialize into object
            LocalizationData data = JsonUtility.FromJson<LocalizationData>(textAsset.text);

            localizedText.Clear();

            //match the pair
            if (data != null && data.items != null) 
            {
                foreach (LocalizationItem item in data.items) 
                {
                    localizedText[item.key] = item.value;
                }
            }

            //notify change language event
            OnLanguageChanged?.Invoke();
        } 
        else 
        {
            Debug.LogError($"[LocalizationManager] - Load Language - JSON File Not Found in Resources/Localization/{langCode}");
        }
    }
    #endregion

    #region Nested Classes
    /// Represents a single localization entry (key-value pair)
    [Serializable]
    private class LocalizationItem
    {
        public string key;
        public string value;
    }

    /// Represents list of localization items
    [Serializable]
    private class LocalizationData
    {
        public List<LocalizationItem> items;
    }
    #endregion
}
