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
    public const string VIETNAMESE = "vi";
    public const string ENGLISH = "en";

    public const string STRING_TABLE_NAME = "PTL_String_Tables";
    #endregion

    #region Instance
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
    public static event Action<Locale> OnLanguageChanged;
    #endregion

    #region Properties
    public string CurrentLanguageCode {
        get {
            var locale = LocalizationSettings.SelectedLocale;
            return locale != null ? locale.Identifier.Code : VIETNAMESE;
        }
    }
    #endregion

    #region Lifecycle
    private void Awake() {
        if (instance != null && instance != this) {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        StartCoroutine(EnsureDefaultLocale());
    }

    private void OnEnable() {
        LocalizationSettings.SelectedLocaleChanged += HandleLocaleChanged;
    }

    private void OnDisable() {
        LocalizationSettings.SelectedLocaleChanged -= HandleLocaleChanged;
    }
    #endregion

    #region Public Methods
    public void SwitchLanguage(string langCode) {
        StartCoroutine(SetLocaleRoutine(langCode));
    }

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
    private IEnumerator EnsureDefaultLocale() {
        yield return LocalizationSettings.InitializationOperation;

        foreach (var locale in LocalizationSettings.AvailableLocales.Locales) {
            Debug.Log($"[LocalizationCheck] Available Locale: {locale.Identifier.Code} ({locale.LocaleName})");
        }

        Locale defaultLocale = LocalizationSettings.AvailableLocales.GetLocale(VIETNAMESE);
        if (defaultLocale != null && LocalizationSettings.SelectedLocale != defaultLocale) {
            LocalizationSettings.SelectedLocale = defaultLocale;
            Debug.Log("[LocalizationManager] Forced Default Locale to Vietnamese (vi)");
        } else {
            Debug.LogWarning("[LocalizationManager] Not found 'vi' or already set");
        }
    }

    private IEnumerator SetLocaleRoutine(string langCode) {
        yield return LocalizationSettings.InitializationOperation;

        Locale targetLocale = LocalizationSettings.AvailableLocales.GetLocale(langCode);
        if (targetLocale != null) {
            LocalizationSettings.SelectedLocale = targetLocale;
            Debug.Log($"[LocalizationManager] Changed language to: {langCode}");
        } else {
            Debug.LogWarning($"[LocalizationManager] Locale code '{langCode}' not found.");
        }
    }

    private void HandleLocaleChanged(Locale newLocale) {
        OnLanguageChanged?.Invoke(newLocale);
    }
    #endregion
}