using ProtectThatLetter.Definitions;
using System;
using UnityEngine;

namespace ProtectThatLetter.Managers 
{
    /// <summary>
    /// Holds runtime game settings (audio, language) and broadcasts changes.
    /// No persistence — game is single-playthrough.
    /// </summary>
    [DisallowMultipleComponent]
    public class SettingsManager : MonoBehaviour 
    {
        #region Instance
        // Singleton instance with public getter and private setter
        public static SettingsManager Instance { get; private set; }
        #endregion

        #region Properties
        // Current settings (default: ON / Vietnamese)
        public bool IsBGMOn { get; private set; } = true;
        public bool IsSFXOn { get; private set; } = true;
        public string CurrentLanguageCode { get; private set; } = GameDefinitions.Languages.DEFAULT_LANGUAGE;
        #endregion

        #region Events
        // Broadcast when each setting changes (consumers react to these)
        public static event Action<bool> OnBGMSettingChanged;
        public static event Action<bool> OnSFXSettingChanged;
        public static event Action<string> OnLanguageSettingChanged;
        #endregion

        #region Unity Lifecycle
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
        /// Clears the singleton reference when destroyed
        /// </summary>
        private void OnDestroy() {
            if (Instance == this) {
                Instance = null;
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Sets the BGM state and broadcasts the change (skips if unchanged)
        /// </summary>
        public void SetBGM(bool isOn) {
            if (IsBGMOn == isOn) return; // Skip if unchanged
            IsBGMOn = isOn;
            OnBGMSettingChanged?.Invoke(IsBGMOn);
        }

        /// <summary>
        /// Sets the SFX state and broadcasts the change (skips if unchanged)
        /// </summary>
        public void SetSFX(bool isOn) {
            if (IsSFXOn == isOn) return; // Skip if unchanged
            IsSFXOn = isOn;
            OnSFXSettingChanged?.Invoke(IsSFXOn);
        }

        /// <summary>
        /// Changes the language and broadcasts the change (skips if unchanged)
        /// </summary>
        /// <param name="langCode">Language code (e.g., "vi", "en")</param>
        public void ChangeLanguage(string langCode) {
            if (CurrentLanguageCode == langCode) return; // Skip if unchanged
            CurrentLanguageCode = langCode;
            OnLanguageSettingChanged?.Invoke(CurrentLanguageCode);
        }
        #endregion
    }
}