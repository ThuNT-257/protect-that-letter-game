using ProtectThatLetter.Definitions;
using ProtectThatLetter.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages the Settings UI with SettingsPart and LanguagePart.
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class SettingsUI : MonoBehaviour {
    #region Serialized Fields
    [Header("Controller References")]
    [SerializeField] private SettingsManager controller; // Reference to SettingsManager

    [Header("Hierarchy Group Panels")]
    [SerializeField] private GameObject settingsPart;   // Main settings panel
    [SerializeField] private GameObject languagePart;   // Language selection panel

    [Header("Main Settings UI Buttons")]
    [SerializeField] private Button settingsButton;             // Open settings
    [SerializeField] private Button settingsCloseButton;        // Close settings (button)
    [SerializeField] private Button settingsCloseOverlayButton; // Close settings (overlay click)

    [Header("Sound On/Off Buttons")]
    [SerializeField] private Button bgmButton;           // BGM toggle
    [SerializeField] private Sprite bgmOnSprite;         // BGM ON sprite
    [SerializeField] private Sprite bgmOffSprite;        // BGM OFF sprite

    [SerializeField] private Button sfxButton;           // SFX toggle
    [SerializeField] private Sprite sfxOnSprite;         // SFX ON sprite
    [SerializeField] private Sprite sfxOffSprite;        // SFX OFF sprite

    [Header("Language Selector")]
    [SerializeField] private Button languageButton;      // Opens language popup

    [Header("Language Popup UI")]
    [SerializeField] private Button langCloseButton;         // Close language popup
    [SerializeField] private Button languageOverlayButton;   // Close language popup (overlay)
    [SerializeField] private Button btnVietnamese;           // Vietnamese option
    [SerializeField] private Button btnEnglish;              // English option

    [Header("Language Checkmarks")]
    [SerializeField] private GameObject viCheckMark;     // Vietnamese checkmark
    [SerializeField] private GameObject enCheckMark;     // English checkmark
    #endregion

    #region Private Fields
    private CanvasGroup canvasGroup;     // Cached CanvasGroup reference
    private bool isBGMOn = true;         // Cached BGM state
    private bool isSFXOn = true;         // Cached SFX state
    #endregion

    #region Lifecycle
    /// <summary>
    /// Initializes references, sets up listeners, and hides panels
    /// </summary>
    private void Awake() {
        canvasGroup = GetComponent<CanvasGroup>();

        // Auto-find controller if not assigned
        if (controller == null) {
            controller = SettingsManager.Instance;
        }

        // --- SettingsPart Events ---
        if (settingsButton != null) settingsButton.onClick.AddListener(OnSettingsButtonClicked);
        if (settingsCloseButton != null) settingsCloseButton.onClick.AddListener(ClosePopup);
        if (settingsCloseOverlayButton != null) settingsCloseOverlayButton.onClick.AddListener(ClosePopup);

        // Sound toggle events
        if (bgmButton != null) bgmButton.onClick.AddListener(ToggleBGM);
        if (sfxButton != null) sfxButton.onClick.AddListener(ToggleSFX);

        // --- LanguagePart Events ---
        if (languageButton != null) languageButton.onClick.AddListener(OpenLanguagePopup);
        if (langCloseButton != null) langCloseButton.onClick.AddListener(CloseLanguagePopup);
        if (languageOverlayButton != null) languageOverlayButton.onClick.AddListener(CloseLanguagePopup);

        // Language options use GameDefinitions constants (no magic strings)
        if (btnVietnamese != null) btnVietnamese.onClick.AddListener(() => OnSelectLanguage(GameDefinitions.Languages.VIETNAMESE));
        if (btnEnglish != null) btnEnglish.onClick.AddListener(() => OnSelectLanguage(GameDefinitions.Languages.ENGLISH));

        // Initialize as hidden
        HideAllParts();
        SetOverlayVisible(false);
    }

    /// <summary>
    /// Subscribes to language changes and syncs UI on enable
    /// </summary>
    private void OnEnable() {
        if (controller == null) {
            controller = SettingsManager.Instance;
        }

        LocalizationManager.OnLanguageChanged += OnLanguageChanged;

        SyncSoundUI();
        UpdateLanguageCheckmarks();
    }

    /// <summary>
    /// Unsubscribes from events to prevent memory leaks
    /// </summary>
    private void OnDisable() {
        LocalizationManager.OnLanguageChanged -= OnLanguageChanged;
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Handles the settings button click (plays SFX, opens popup)
    /// </summary>
    public void OnSettingsButtonClicked() {
        if (AudioManager.Instance != null) {
            AudioManager.Instance.PlaySFX("settings_button_click");
        }
        OpenPopup();
    }

    /// <summary>
    /// Opens the main settings popup
    /// </summary>
    public void OpenPopup() {
        SetOverlayVisible(true);
        ShowSettingsPart();
        SyncSoundUI();
    }

    /// <summary>
    /// Closes the settings popup
    /// </summary>
    public void ClosePopup() {
        HideAllParts();
        SetOverlayVisible(false);
    }
    #endregion

    #region Private UI Logic
    /// <summary>
    /// Shows the settings panel and hides the language panel
    /// </summary>
    private void ShowSettingsPart() {
        if (settingsPart != null) settingsPart.SetActive(true);
        if (languagePart != null) languagePart.SetActive(false);
    }

    /// <summary>
    /// Shows the language selection panel
    /// </summary>
    private void OpenLanguagePopup() {
        if (AudioManager.Instance != null) {
            AudioManager.Instance.PlaySFX("button_click");
        }

        if (settingsPart != null) settingsPart.SetActive(false);
        if (languagePart != null) {
            languagePart.SetActive(true);
            UpdateLanguageCheckmarks(); // Refresh checkmarks when opening
        }
    }

    /// <summary>
    /// Closes the language popup and returns to the main settings panel
    /// </summary>
    private void CloseLanguagePopup() {
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
    /// Shows or hides the entire popup via CanvasGroup and GameObject state
    /// </summary>
    private void SetOverlayVisible(bool isVisible) {
        if (canvasGroup != null) {
            canvasGroup.alpha = isVisible ? 1f : 0f;
            canvasGroup.interactable = isVisible;
            canvasGroup.blocksRaycasts = isVisible;
        }

        gameObject.SetActive(isVisible);
    }
    #endregion

    #region Sound & Language Handlers
    /// <summary>
    /// Toggles BGM and pushes the change to SettingsManager
    /// </summary>
    private void ToggleBGM() {
        if (AudioManager.Instance != null) {
            AudioManager.Instance.PlaySFX("button_click");
        }
        isBGMOn = !isBGMOn;
        UpdateBGMVisual();

        if (controller != null) controller.SetBGM(isBGMOn);
    }

    /// <summary>
    /// Toggles SFX and pushes the change to SettingsManager
    /// </summary>
    private void ToggleSFX() {
        if (AudioManager.Instance != null) {
            AudioManager.Instance.PlaySFX("button_click");
        }

        isSFXOn = !isSFXOn;
        UpdateSFXVisual();

        if (controller != null) controller.SetSFX(isSFXOn);
    }

    /// <summary>
    /// Syncs cached sound state with SettingsManager
    /// </summary>
    private void SyncSoundUI() {
        if (SettingsManager.Instance != null) {
            isBGMOn = SettingsManager.Instance.IsBGMOn;
            isSFXOn = SettingsManager.Instance.IsSFXOn;
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
        SettingsManager targetController = controller != null ? controller : SettingsManager.Instance;

        if (targetController != null) {
            targetController.ChangeLanguage(langCode);
        } else {
            Debug.LogError("[SettingsUI] SettingsController.Instance is NULL!");
        }

        CloseLanguagePopup();
    }

    /// <summary>
    /// Refreshes checkmarks when language changes externally
    /// </summary>
    private void OnLanguageChanged(string newLanguageCode) {
        UpdateLanguageCheckmarks();
    }

    /// <summary>
    /// Updates the language checkmarks to reflect the current language
    /// </summary>
    private void UpdateLanguageCheckmarks() {
        if (LocalizationManager.Instance == null) return;

        string currentLang = LocalizationManager.Instance.CurrentLanguageCode;

        // Show checkmark for the active language
        if (viCheckMark != null) {
            viCheckMark.SetActive(currentLang == GameDefinitions.Languages.VIETNAMESE);
        }

        if (enCheckMark != null) {
            enCheckMark.SetActive(currentLang == GameDefinitions.Languages.ENGLISH);
        }
    }
    #endregion
}