using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using ProtectThatLetter.Definitions;

namespace ProtectThatLetter.Managers {
    /// <summary>
    /// Wrapper Manager for Unity's Localization Package.
    /// Handles language switching, string fetching, and locale events.
    /// </summary>
    [DisallowMultipleComponent]
    public class LocalizationManager : MonoBehaviour {
        #region Singleton
        // Singleton instance with public getter and private setter
        public static LocalizationManager Instance { get; private set; }
        #endregion

        #region Properties
        // Gets the current language code from the Localization Package (null-safe)
        public string CurrentLanguageCode {
            get {
                if (!LocalizationSettings.HasSettings)
                    return GameDefinitions.Languages.DEFAULT_LANGUAGE;

                Locale locale = LocalizationSettings.SelectedLocale;
                return locale != null ? locale.Identifier.Code : GameDefinitions.Languages.DEFAULT_LANGUAGE;
            }
        }

        // Returns true if the Localization Package has finished initializing (null-safe)
        public bool IsInitialized {
            get {
                if (!LocalizationSettings.HasSettings) return false;

                var handle = LocalizationSettings.InitializationOperation;
                return handle.IsValid() && handle.IsDone;
            }
        }
        #endregion

        #region Events
        // Triggered whenever the language/locale changes (passes the language code)
        public static event Action<string> OnLanguageChanged;
        #endregion

        #region Lifecycle
        /// <summary>
        /// Ensures singleton integrity and makes the object persistent
        /// </summary>
        private void Awake() {
            // Destroy duplicate instances
            if (Instance != null && Instance != this) {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist across scenes
        }

        /// <summary>
        /// Clears the singleton reference and static event subscribers
        /// </summary>
        private void OnDestroy() {
            if (Instance == this) {
                OnLanguageChanged = null; // Clear static event subscribers to prevent memory leaks
                Instance = null;
            }
        }

        /// <summary>
        /// Subscribes to locale and settings events when enabled
        /// </summary>
        private void OnEnable() {
            LocalizationSettings.SelectedLocaleChanged += HandleLocaleChanged;
            SettingsManager.OnLanguageSettingChanged += SwitchLanguage;
        }

        /// <summary>
        /// Unsubscribes from events to prevent memory leaks
        /// </summary>
        private void OnDisable() {
            LocalizationSettings.SelectedLocaleChanged -= HandleLocaleChanged;
            SettingsManager.OnLanguageSettingChanged -= SwitchLanguage;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Initializes the Localization system and sets the default locale
        /// </summary>
        /// <param name="defaultLangCode">Default language code (defaults from GameDefinitions)</param>
        public IEnumerator InitializeRoutine(string defaultLangCode = GameDefinitions.Languages.DEFAULT_LANGUAGE) {
            // Wait for the Localization Package to initialize (with timeout)
            yield return WaitForInitializationRoutine();

            // Validate AvailableLocales before using it
            if (LocalizationSettings.AvailableLocales == null) {
                Debug.LogWarning($"[{nameof(LocalizationManager)}] AvailableLocales is null.");
                yield break;
            }

            // Apply the default locale if available and not already selected
            Locale defaultLocale = LocalizationSettings.AvailableLocales.GetLocale(defaultLangCode);
            if (defaultLocale != null && LocalizationSettings.SelectedLocale != defaultLocale) {
                LocalizationSettings.SelectedLocale = defaultLocale;
            }
        }

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
        /// <param name="tableName">Name of the string table</param>
        public void GetLocalizedString(string key, Action<string> onCompleted, string tableName = GameDefinitions.Localization.STRING_TABLE_NAME) {
            // Fallback: if LocalizationSettings is unavailable, return the key itself
            if (!LocalizationSettings.HasSettings) {
                Debug.LogWarning($"[{nameof(LocalizationManager)}] LocalizationSettings unavailable. Callback returned key.");
                onCompleted?.Invoke(key);
                return;
            }

            var handle = LocalizationSettings.StringDatabase.GetLocalizedStringAsync(tableName, key);

            // If already done, invoke immediately; otherwise, subscribe to completion
            if (handle.IsDone) {
                onCompleted?.Invoke(handle.Result);
            } else {
                handle.Completed += (op) => onCompleted?.Invoke(op.Result);
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Helper coroutine to safely wait for LocalizationSettings initialization (with timeout)
        /// </summary>
        private IEnumerator WaitForInitializationRoutine() {
            float timeout = 10f;
            float timer = 0f;

            // Wait until LocalizationSettings is available
            while (!LocalizationSettings.HasSettings && timer < timeout) {
                timer += Time.unscaledDeltaTime;
                yield return null;
            }

            // Timeout: LocalizationSettings was never available
            if (!LocalizationSettings.HasSettings) {
                Debug.LogError($"[{nameof(LocalizationManager)}] Timeout waiting for LocalizationSettings.");
                yield break;
            }

            // Wait for the initialization operation to complete
            var handle = LocalizationSettings.InitializationOperation;
            timer = 0f;
            while ((!handle.IsValid() || !handle.IsDone) && timer < timeout) {
                timer += Time.unscaledDeltaTime;
                yield return null;
                handle = LocalizationSettings.InitializationOperation; // Re-fetch in case it changed
            }
        }

        /// <summary>
        /// Handles locale change events and broadcasts the language code
        /// </summary>
        private void HandleLocaleChanged(Locale newLocale) {
            string code = newLocale != null ? newLocale.Identifier.Code : CurrentLanguageCode;
            OnLanguageChanged?.Invoke(code);
        }

        /// <summary>
        /// Coroutine that changes the locale after ensuring initialization
        /// </summary>
        /// <param name="langCode">Language code to switch to</param>
        private IEnumerator SetLocaleRoutine(string langCode) {
            // Wait for initialization if not already done
            if (!IsInitialized) {
                yield return WaitForInitializationRoutine();
            }

            // Validate state before switching
            if (!LocalizationSettings.HasSettings || LocalizationSettings.AvailableLocales == null) {
                yield break;
            }

            // Find and apply the target locale
            Locale targetLocale = LocalizationSettings.AvailableLocales.GetLocale(langCode);
            if (targetLocale != null) {
                LocalizationSettings.SelectedLocale = targetLocale;
                Debug.Log($"[{nameof(LocalizationManager)}] Changed language to: {langCode}");
            } else {
                Debug.LogWarning($"[{nameof(LocalizationManager)}] Locale code '{langCode}' not found.");
            }
        }
        #endregion
    }
}