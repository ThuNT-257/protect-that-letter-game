using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

/// <summary>
/// Wrapper Manager class for Unity's Localization Package.
/// Handles language switching, table constants, and localized string fetching.
/// </summary>
public class LocalizationManager : MonoBehaviour {
    #region Constants
    // Language code constants for easy reference
    public const string VIETNAMESE = "vi";
    public const string ENGLISH = "en";

    // Name of the string table in the Localization Package
    public const string STRING_TABLE_NAME = "PTL_String_Tables";
    #endregion

    #region Instance
    // Singleton instance
    private static LocalizationManager instance;

    public static LocalizationManager Instance {
        get {
            if (instance == null) {
                instance = FindAnyObjectByType<LocalizationManager>();
                if (instance == null) {
                    Debug.LogError("[LocalizationManager] No instance found in scene.");
                }
            }
            return instance;
        }
    }
    #endregion

    #region Events
    // Triggered whenever the language/locale changes
    public static event Action<Locale> OnLanguageChanged;
    #endregion

    #region Properties
    // Gets the current language code from the Localization Package
    public string CurrentLanguageCode {
        get {
            var locale = LocalizationSettings.SelectedLocale;
            return locale != null ? locale.Identifier.Code : VIETNAMESE;
        }
    }
    #endregion

    #region Lifecycle
    /// <summary>
    /// Ensures singleton integrity and sets up the default locale
    /// </summary>
    private void Awake() {
        // Destroy duplicate instances
        if (instance != null && instance != this) {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        // Ensure the default locale is set after initialization
        StartCoroutine(EnsureDefaultLocale());
    }

    /// <summary>
    /// Subscribes to locale change events when enabled
    /// </summary>
    private void OnEnable() {
        LocalizationSettings.SelectedLocaleChanged += HandleLocaleChanged;
    }

    /// <summary>
    /// Unsubscribes from locale change events to prevent memory leaks
    /// </summary>
    private void OnDisable() {
        LocalizationSettings.SelectedLocaleChanged -= HandleLocaleChanged;
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Switches the current language to the specified code
    /// </summary>
    /// <param name="langCode">Language code (e.g., "vi", "en")</param>
    public void SwitchLanguage(string langCode) {
        StartCoroutine(SetLocaleRoutine(langCode));
    }

    /// <summary>
    /// Gets a localized string asynchronously from the string table
    /// </summary>
    /// <param name="key">The key to look up</param>
    /// <param name="onCompleted">Callback invoked when the string is ready</param>
    /// <param name="tableName">Name of the string table (defaults to STRING_TABLE_NAME)</param>
    public void GetLocalizedString(string key, Action<string> onCompleted, string tableName = STRING_TABLE_NAME) {
        var handle = LocalizationSettings.StringDatabase.GetLocalizedStringAsync(tableName, key);

        if (handle.IsDone) {
            onCompleted?.Invoke(handle.Result);
        } else {
            handle.Completed += (op) => onCompleted?.Invoke(op.Result);
        }
    }
    #endregion

    #region Private Methods
    /// <summary>
    /// Ensures the default locale is set to Vietnamese after initialization
    /// </summary>
    private IEnumerator EnsureDefaultLocale() {
        // Wait for the Localization system to initialize
        yield return LocalizationSettings.InitializationOperation;

        // Log all available locales for debugging
        foreach (var locale in LocalizationSettings.AvailableLocales.Locales) {
            Debug.Log($"[LocalizationCheck] Available Locale: {locale.Identifier.Code} ({locale.LocaleName})");
        }

        // Force default locale to Vietnamese if available
        Locale defaultLocale = LocalizationSettings.AvailableLocales.GetLocale(VIETNAMESE);
        if (defaultLocale != null && LocalizationSettings.SelectedLocale != defaultLocale) {
            LocalizationSettings.SelectedLocale = defaultLocale;
            Debug.Log("[LocalizationManager] Forced Default Locale to Vietnamese (vi)");
        } else {
            Debug.LogWarning("[LocalizationManager] Not found 'vi' or already set");
        }
    }

    /// <summary>
    /// Coroutine that changes the locale after the Localization system is initialized
    /// </summary>
    /// <param name="langCode">Language code to switch to</param>
    private IEnumerator SetLocaleRoutine(string langCode) {
        // Wait for the Localization system to initialize
        yield return LocalizationSettings.InitializationOperation;

        // Find and apply the target locale
        Locale targetLocale = LocalizationSettings.AvailableLocales.GetLocale(langCode);
        if (targetLocale != null) {
            LocalizationSettings.SelectedLocale = targetLocale;
            Debug.Log($"[LocalizationManager] Changed language to: {langCode}");
        } else {
            Debug.LogWarning($"[LocalizationManager] Locale code '{langCode}' not found.");
        }
    }

    /// <summary>
    /// Handles locale change events from the Localization Package
    /// </summary>
    /// <param name="newLocale">The new locale that was selected</param>
    private void HandleLocaleChanged(Locale newLocale) {
        OnLanguageChanged?.Invoke(newLocale);
    }
    #endregion
}