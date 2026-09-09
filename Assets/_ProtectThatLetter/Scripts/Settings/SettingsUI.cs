using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

/// <summary>
/// Manages the Settings UI with SettingsPart and LanguagePart.
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class SettingsUI : MonoBehaviour {
    #region Serialized Fields
    [Header("Controller References")]
    [SerializeField] private SettingsManager controller;

    [Header("Hierarchy Group Panels")]
    [SerializeField] private GameObject settingsPart;       
    [SerializeField] private GameObject languagePart;       

    [Header("Main Settings UI Buttons")]
    [SerializeField] private Button settingsButton;            
    [SerializeField] private Button settingsCloseButton;       
    [SerializeField] private Button settingsCloseOverlayButton;

    [Header("Sound On/Off Buttons")]
    [SerializeField] private Button bgmButton;
    [SerializeField] private Sprite bgmOnSprite;
    [SerializeField] private Sprite bgmOffSprite;

    [SerializeField] private Button sfxButton;
    [SerializeField] private Sprite sfxOnSprite;
    [SerializeField] private Sprite sfxOffSprite;

    [Header("Language Selector")]
    [SerializeField] private Button languageButton;           

    [Header("Language Popup UI")]
    [SerializeField] private Button langCloseButton;           
    [SerializeField] private Button languageOverlayButton;     
    [SerializeField] private Button btnVietnamese;
    [SerializeField] private Button btnEnglish;

    [Header("Language Checkmarks")]
    [SerializeField] private GameObject viCheckMark;
    [SerializeField] private GameObject enCheckMark;
    #endregion

    #region Private Fields
    private CanvasGroup canvasGroup;
    private bool isBGMOn = true;
    private bool isSFXOn = true;
    #endregion

    #region Lifecycle
    private void Awake() {
        canvasGroup = GetComponent<CanvasGroup>();

        if (controller == null) {
            controller = SettingsManager.Instance;
        }

        // 1. SettingsPart Events
        if (settingsButton != null) settingsButton.onClick.AddListener(OnSettingsButtonClicked);
        if (settingsCloseButton != null) settingsCloseButton.onClick.AddListener(ClosePopup);
        if (settingsCloseOverlayButton != null) settingsCloseOverlayButton.onClick.AddListener(ClosePopup);

        // Sound Toggle Events
        if (bgmButton != null) bgmButton.onClick.AddListener(ToggleBGM);
        if (sfxButton != null) sfxButton.onClick.AddListener(ToggleSFX);

        // 2. LanguagePart Events
        if (languageButton != null) languageButton.onClick.AddListener(OpenLanguagePopup);
        if (langCloseButton != null) langCloseButton.onClick.AddListener(CloseLanguagePopup);
        if (languageOverlayButton != null) languageOverlayButton.onClick.AddListener(CloseLanguagePopup);

        if (btnVietnamese != null) btnVietnamese.onClick.AddListener(() => OnSelectLanguage(LocalizationManager.VIETNAMESE));
        if (btnEnglish != null) btnEnglish.onClick.AddListener(() => OnSelectLanguage(LocalizationManager.ENGLISH));

        HideAllParts();
        SetOverlayVisible(false);
    }

    private void OnEnable() {
        if (controller == null) {
            controller = SettingsManager.Instance;
        }

        LocalizationManager.OnLanguageChanged += OnLanguageChanged;

        SyncSoundUI();
        UpdateLanguageCheckmarks();
    }

    private void OnDisable() {
        LocalizationManager.OnLanguageChanged -= OnLanguageChanged;
    }
    #endregion

    #region Public Methods
    public void OnSettingsButtonClicked() {
        if(AudioManager.Instance != null) {
            AudioManager.Instance.PlaySFX("settings_button_click");
        }
        OpenPopup();
    }

    public void OpenPopup() {
        SetOverlayVisible(true);
        ShowSettingsPart();
        SyncSoundUI();
    }

    public void ClosePopup() {
        HideAllParts();
        SetOverlayVisible(false);
    }
    #endregion

    #region Private UI Logic
    private void ShowSettingsPart() {
        if (settingsPart != null) settingsPart.SetActive(true);
        if (languagePart != null) languagePart.SetActive(false);
    }

    private void OpenLanguagePopup() {
        if (AudioManager.Instance != null) {
            AudioManager.Instance.PlaySFX("button_click");
        }

        if (settingsPart != null) settingsPart.SetActive(false);
        if (languagePart != null) {
            languagePart.SetActive(true);
            UpdateLanguageCheckmarks();
        }
    }

    private void CloseLanguagePopup() {
        ShowSettingsPart();
    }

    private void HideAllParts() {
        if (settingsPart != null) settingsPart.SetActive(false);
        if (languagePart != null) languagePart.SetActive(false);
    }

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
    private void ToggleBGM() {
        if(AudioManager.Instance != null) {
            AudioManager.Instance.PlaySFX("button_click");
        }
        isBGMOn = !isBGMOn;
        UpdateBGMVisual();

        if (controller != null) controller.SetBGM(isBGMOn);
    }

    private void ToggleSFX() {
        if (AudioManager.Instance != null) {
            AudioManager.Instance.PlaySFX("button_click");
        }

        isSFXOn = !isSFXOn;
        UpdateSFXVisual();

        if (controller != null) controller.SetSFX(isSFXOn);
    }

    private void SyncSoundUI() {
        if (SettingsManager.Instance != null) {
            isBGMOn = SettingsManager.Instance.IsBGMOn;
            isSFXOn = SettingsManager.Instance.IsSFXOn;
        }

        UpdateBGMVisual();
        UpdateSFXVisual();
    }

    private void UpdateBGMVisual() {
        if (bgmButton != null && bgmButton.image != null) {
            bgmButton.image.sprite = isBGMOn ? bgmOnSprite : bgmOffSprite;
        }
    }

    private void UpdateSFXVisual() {
        if (sfxButton != null && sfxButton.image != null) {
            sfxButton.image.sprite = isSFXOn ? sfxOnSprite : sfxOffSprite;
        }
    }

    private void OnSelectLanguage(string langCode) {
        SettingsManager targetController = controller != null ? controller : SettingsManager.Instance;

        if (targetController != null) {
            targetController.ChangeLanguage(langCode);
        } else {
            Debug.LogError("[SettingsUI] SettingsController.Instance is NULL!");
        }

        CloseLanguagePopup();
    }

    private void OnLanguageChanged(Locale newLocale) {
        UpdateLanguageCheckmarks();
    }

    private void UpdateLanguageCheckmarks() {
        if (LocalizationManager.Instance == null) return;

        string currentLang = LocalizationManager.Instance.CurrentLanguageCode;

        if (viCheckMark != null) {
            viCheckMark.SetActive(currentLang == LocalizationManager.VIETNAMESE);
        }

        if (enCheckMark != null) {
            enCheckMark.SetActive(currentLang == LocalizationManager.ENGLISH);
        }
    }
    #endregion
}