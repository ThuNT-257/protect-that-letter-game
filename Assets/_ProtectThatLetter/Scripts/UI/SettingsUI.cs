using ProtectThatLetter.Definitions;
using ProtectThatLetter.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace ProtectThatLetter.UI {
    /// <summary>
    /// Pure UI View for Settings and Language sub-panels.
    /// Handles visual states and passes user interactions to SettingsManager.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class SettingsUI : MonoBehaviour {
        #region Serialized Fields
        [Header("Controller References")]
        [SerializeField] private SettingsManager controller;

        [Header("Hierarchy Group Panels")]
        [SerializeField] private GameObject settingsPart;   // Main settings panel
        [SerializeField] private GameObject languagePart;   // Language selection panel

        [Header("Main Settings UI Buttons")]
        [SerializeField] private Button settingsButton;             // Open settings
        [SerializeField] private Button settingsCloseButton;        // Close settings (button)
        [SerializeField] private Button settingsCloseOverlayButton; // Close settings (overlay)

        [Header("Sound On/Off Buttons")]
        [SerializeField] private Button bgmButton;       // BGM toggle
        [SerializeField] private Sprite bgmOnSprite;     // BGM ON sprite
        [SerializeField] private Sprite bgmOffSprite;    // BGM OFF sprite

        [SerializeField] private Button sfxButton;       // SFX toggle
        [SerializeField] private Sprite sfxOnSprite;     // SFX ON sprite
        [SerializeField] private Sprite sfxOffSprite;    // SFX OFF sprite

        [Header("Language Selector")]
        [SerializeField] private Button languageButton;  // Opens language popup

        [Header("Language Popup UI")]
        [SerializeField] private Button langCloseButton;         // Close language popup
        [SerializeField] private Button languageOverlayButton;   // Close language popup (overlay)
        [SerializeField] private Button btnVietnamese;           // Vietnamese option
        [SerializeField] private Button btnEnglish;              // English option

        [Header("Language Checkmarks")]
        [SerializeField] private GameObject viCheckMark; // Vietnamese checkmark
        [SerializeField] private GameObject enCheckMark; // English checkmark
        #endregion

        #region Private Fields
        private CanvasGroup canvasGroup;   // Cached CanvasGroup reference
        private bool isBGMOn = true;       // Cached BGM state
        private bool isSFXOn = true;       // Cached SFX state
        #endregion

        #region Unity Lifecycle
        /// <summary>
        /// Caches CanvasGroup and registers button listeners
        /// </summary>
        private void Awake() {
            canvasGroup = GetComponent<CanvasGroup>();
            RegisterButtonListeners();
        }

        /// <summary>
        /// Subscribes to language events and syncs initial UI state
        /// </summary>
        private void OnEnable() {
            // Fallback to singleton if not assigned
            if (controller == null) {
                controller = SettingsManager.Instance;
            }

            LocalizationManager.OnLanguageChanged += OnLanguageChanged;

            SetOverlayVisible(false);
            SyncSoundUI();
            UpdateLanguageCheckmarks();
        }

        /// <summary>
        /// Unsubscribes from language events
        /// </summary>
        private void OnDisable() {
            LocalizationManager.OnLanguageChanged -= OnLanguageChanged;
        }

        /// <summary>
        /// Removes button listeners to prevent memory leaks
        /// </summary>
        private void OnDestroy() {
            UnregisterButtonListeners();
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Opens the settings popup
        /// </summary>
        public void OpenPopup() {
            PlayButtonClickSFX();
            SetOverlayVisible(true);
            ShowSettingsPart();
            SyncSoundUI();
        }

        /// <summary>
        /// Closes the settings popup
        /// </summary>
        public void ClosePopup() {
            PlayButtonClickSFX();
            HideAllParts();
            SetOverlayVisible(false);
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Registers all button listeners
        /// </summary>
        private void RegisterButtonListeners() {
            if (settingsButton != null) settingsButton.onClick.AddListener(OpenPopup);
            if (settingsCloseButton != null) settingsCloseButton.onClick.AddListener(ClosePopup);
            if (settingsCloseOverlayButton != null) settingsCloseOverlayButton.onClick.AddListener(ClosePopup);

            if (bgmButton != null) bgmButton.onClick.AddListener(ToggleBGM);
            if (sfxButton != null) sfxButton.onClick.AddListener(ToggleSFX);

            if (languageButton != null) languageButton.onClick.AddListener(OpenLanguagePopup);
            if (langCloseButton != null) langCloseButton.onClick.AddListener(CloseLanguagePopup);
            if (languageOverlayButton != null) languageOverlayButton.onClick.AddListener(CloseLanguagePopup);

            if (btnVietnamese != null) btnVietnamese.onClick.AddListener(() => OnSelectLanguage(GameDefinitions.Languages.VIETNAMESE));
            if (btnEnglish != null) btnEnglish.onClick.AddListener(() => OnSelectLanguage(GameDefinitions.Languages.ENGLISH));
        }

        /// <summary>
        /// Removes all button listeners
        /// </summary>
        private void UnregisterButtonListeners() {
            if (settingsButton != null) settingsButton.onClick.RemoveListener(OpenPopup);
            if (settingsCloseButton != null) settingsCloseButton.onClick.RemoveListener(ClosePopup);
            if (settingsCloseOverlayButton != null) settingsCloseOverlayButton.onClick.RemoveListener(ClosePopup);

            if (bgmButton != null) bgmButton.onClick.RemoveListener(ToggleBGM);
            if (sfxButton != null) sfxButton.onClick.RemoveListener(ToggleSFX);

            if (languageButton != null) languageButton.onClick.RemoveListener(OpenLanguagePopup);
            if (langCloseButton != null) langCloseButton.onClick.RemoveListener(CloseLanguagePopup);
            if (languageOverlayButton != null) languageOverlayButton.onClick.RemoveListener(CloseLanguagePopup);

            // Lambda listeners need RemoveAllListeners
            if (btnVietnamese != null) btnVietnamese.onClick.RemoveAllListeners();
            if (btnEnglish != null) btnEnglish.onClick.RemoveAllListeners();
        }

        /// <summary>
        /// Shows the main settings panel, hides the language panel
        /// </summary>
        private void ShowSettingsPart() {
            if (settingsPart != null) settingsPart.SetActive(true);
            if (languagePart != null) languagePart.SetActive(false);
        }

        /// <summary>
        /// Shows the language selection panel
        /// </summary>
        private void OpenLanguagePopup() {
            PlayButtonClickSFX();
            if (settingsPart != null) settingsPart.SetActive(false);
            if (languagePart != null) {
                languagePart.SetActive(true);
                UpdateLanguageCheckmarks();
            }
        }

        /// <summary>
        /// Closes the language popup and returns to the settings panel
        /// </summary>
        private void CloseLanguagePopup() {
            PlayButtonClickSFX();
            ShowSettingsPart();
        }

        /// <summary>
        /// Hides both settings and language panels
        /// </summary>
        private void HideAllParts() {
            if (settingsPart != null) settingsPart.SetActive(false);
            if (languagePart != null) languagePart.SetActive(false);
        }

        /// <summary>
        /// Controls visibility safely using CanvasGroup without deactivating the GameObject
        /// </summary>
        private void SetOverlayVisible(bool isVisible) {
            if (canvasGroup != null) {
                canvasGroup.alpha = isVisible ? 1f : 0f;
                canvasGroup.interactable = isVisible;
                canvasGroup.blocksRaycasts = isVisible;
            }
        }

        /// <summary>
        /// Plays the normal button click SFX
        /// </summary>
        private void PlayButtonClickSFX() {
            if (AudioManager.Instance != null) {
                AudioManager.Instance.PlaySFX(GameDefinitions.SfxNames.SFX_NORMAL_BUTTON_CLICKED);
            }
        }

        /// <summary>
        /// Toggles BGM and pushes the change to SettingsManager
        /// </summary>
        private void ToggleBGM() {
            PlayButtonClickSFX();
            isBGMOn = !isBGMOn;
            UpdateBGMVisual();

            SettingsManager target = controller != null ? controller : SettingsManager.Instance;
            if (target != null) target.SetBGM(isBGMOn);
        }

        /// <summary>
        /// Toggles SFX and pushes the change to SettingsManager
        /// </summary>
        private void ToggleSFX() {
            PlayButtonClickSFX();
            isSFXOn = !isSFXOn;
            UpdateSFXVisual();

            SettingsManager target = controller != null ? controller : SettingsManager.Instance;
            if (target != null) target.SetSFX(isSFXOn);
        }

        /// <summary>
        /// Syncs cached sound state with SettingsManager
        /// </summary>
        private void SyncSoundUI() {
            SettingsManager target = controller != null ? controller : SettingsManager.Instance;
            if (target != null) {
                isBGMOn = target.IsBGMOn;
                isSFXOn = target.IsSFXOn;
            }

            UpdateBGMVisual();
            UpdateSFXVisual();
        }

        /// <summary>
        /// Updates the BGM button sprite
        /// </summary>
        private void UpdateBGMVisual() {
            if (bgmButton != null && bgmButton.image != null) {
                bgmButton.image.sprite = isBGMOn ? bgmOnSprite : bgmOffSprite;
            }
        }

        /// <summary>
        /// Updates the SFX button sprite
        /// </summary>
        private void UpdateSFXVisual() {
            if (sfxButton != null && sfxButton.image != null) {
                sfxButton.image.sprite = isSFXOn ? sfxOnSprite : sfxOffSprite;
            }
        }

        /// <summary>
        /// Handles language selection (pushes to SettingsManager)
        /// </summary>
        private void OnSelectLanguage(string langCode) {
            PlayButtonClickSFX();
            SettingsManager target = controller != null ? controller : SettingsManager.Instance;

            if (target != null) {
                target.ChangeLanguage(langCode);
            }

            CloseLanguagePopup();
        }

        /// <summary>
        /// Updates language checkmarks to reflect the current language
        /// </summary>
        private void UpdateLanguageCheckmarks() {
            if (LocalizationManager.Instance == null) return;

            string currentLang = LocalizationManager.Instance.CurrentLanguageCode;

            if (viCheckMark != null) {
                viCheckMark.SetActive(currentLang == GameDefinitions.Languages.VIETNAMESE);
            }

            if (enCheckMark != null) {
                enCheckMark.SetActive(currentLang == GameDefinitions.Languages.ENGLISH);
            }
        }
        #endregion

        #region Event Handlers
        /// <summary>
        /// Refreshes checkmarks when the language changes externally
        /// </summary>
        private void OnLanguageChanged(string newLanguageCode) {
            UpdateLanguageCheckmarks();
        }
        #endregion
    }
}