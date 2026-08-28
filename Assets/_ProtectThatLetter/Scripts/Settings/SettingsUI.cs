using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages the Settings UI including audio toggles and language selection
/// </summary>
public class SettingsUI : MonoBehaviour
{
    #region Serialized Fields
    [Header("Controller References")]
    [SerializeField] private SettingsController controller;

    [Header("Main Settings UI")]
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button settingsCloseButton;
    [SerializeField] private Button settingsOverlayCloseButton;

    [Header("Sound On/Off Buttons")]
    [SerializeField] private Button bgmButton;
    [SerializeField] private Sprite bgmOnSprite;
    [SerializeField] private Sprite bgmOffSprite;

    [SerializeField] private Button sfxButton;
    [SerializeField] private Sprite sfxOnSprite;
    [SerializeField] private Sprite sfxOffSprite;

    [Header("Language Selector")]
    [SerializeField] private Button languageSelectorButton;

    [Header("Language Dropdown Popup")]
    [SerializeField] private GameObject langDropdownPanel;
    [SerializeField] private Button langCloseButton;
    [SerializeField] private Button langOverlayCloseButton;
    [SerializeField] private Button btnVietnamese;
    [SerializeField] private Button btnEnglish;

    [Header("Language Checkmarks")]
    [SerializeField] private GameObject viCheckMark;
    [SerializeField] private GameObject enCheckMark;
    #endregion

    #region Private Fields
    private bool isBGMOn = true;
    private bool isSFXOn = true;
    #endregion

    #region Lifecycle
    /// <summary>
    /// Initializes the UI by setting up button listeners and hiding panels
    /// </summary>
    private void Awake()
    {
        if (controller == null)
        {
            controller = GetComponent<SettingsController>();
        }

        // Main Settings UI
        if (settingsButton != null) settingsButton.onClick.AddListener(OpenPopup);
        if (settingsCloseButton != null) settingsCloseButton.onClick.AddListener(ClosePopup);
        if (settingsOverlayCloseButton != null) settingsOverlayCloseButton.onClick.AddListener(ClosePopup);

        // Sound toggle button listeners
        if (bgmButton != null) bgmButton.onClick.AddListener(ToggleBGM);
        if (sfxButton != null) sfxButton.onClick.AddListener(ToggleSFX);

        // Language Buttons
        if (languageSelectorButton != null) languageSelectorButton.onClick.AddListener(OpenLanguagePopup);
        if (langCloseButton != null) langCloseButton.onClick.AddListener(CloseLanguagePopup);
        if (langOverlayCloseButton != null) langOverlayCloseButton.onClick.AddListener(CloseLanguagePopup);

        if (btnVietnamese != null) btnVietnamese.onClick.AddListener(() => OnSelectLanguage(LocalizationManager.VIETNAMESE));
        if (btnEnglish != null) btnEnglish.onClick.AddListener(() => OnSelectLanguage(LocalizationManager.ENGLISH));

        CloseLanguagePopup();
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Subscribes to events and syncs UI when enabled
    /// </summary>
    private void OnEnable()
    {
        LocalizationManager.OnLanguageChanged += UpdateLanguageCheckmarks;

        SyncSoundUI();
        UpdateLanguageCheckmarks();
    }

    /// <summary>
    /// Unsubscribes from events
    /// </summary>
    private void OnDisable()
    {
        LocalizationManager.OnLanguageChanged -= UpdateLanguageCheckmarks;
    }
    #endregion

    #region Private Methods
    /// <summary>
    /// Opens the main settings popup
    /// </summary>
    private void OpenPopup()
    {
        gameObject.SetActive(true);
        SyncSoundUI();
        CloseLanguagePopup();
    }

    /// <summary>
    /// Closes the main settings popup
    /// </summary
    private void ClosePopup()
    {
        CloseLanguagePopup();
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Opens the language popup
    /// </summary>
    private void OpenLanguagePopup()
    {
        if (langDropdownPanel != null)
        {
            langDropdownPanel.SetActive(true);
            UpdateLanguageCheckmarks();
        }
    }

    /// <summary>
    /// Closes the language popup
    /// </summary>
    private void CloseLanguagePopup()
    {
        if (langDropdownPanel != null)
        {
            langDropdownPanel.SetActive(false);
        }
    }

    /// <summary>
    /// Toggles BGM on/off
    /// </summary>
    private void ToggleBGM()
    {
        isBGMOn = !isBGMOn;
        UpdateBGMVisual();

        if (controller != null)
        {
            controller.SetBGM(isBGMOn);
        }
    }

    /// <summary>
    /// Toggles SFX on/off
    /// </summary>
    private void ToggleSFX()
    {
        isSFXOn = !isSFXOn;
        UpdateSFXVisual();

        if (controller != null)
        {
            controller.SetSFX(isSFXOn);
        }
    }

    /// <summary>
    /// Syncs sound UI state with the controller
    /// </summary>
    private void SyncSoundUI()
    {
        if (controller != null)
        {
            isBGMOn = SettingsController.Instance.IsBGMOn;
            isSFXOn = SettingsController.Instance.IsSFXOn;
        }

        UpdateBGMVisual();
        UpdateSFXVisual();
    }

    /// <summary>
    /// Updates the BGM button sprite
    /// </summary>
    private void UpdateBGMVisual()
    {
        if (bgmButton != null && bgmButton.image != null)
        {
            bgmButton.image.sprite = isBGMOn ? bgmOnSprite : bgmOffSprite;
        }
    }

    /// <summary>
    /// Updates the SFX button sprite
    /// </summary>
    private void UpdateSFXVisual()
    {
        if (sfxButton != null && sfxButton.image != null)
        {
            sfxButton.image.sprite = isSFXOn ? sfxOnSprite : sfxOffSprite;
        }
    }

    /// <summary>
    /// Handles language selection
    /// </summary>
    private void OnSelectLanguage(string langCode)
    {
        if (controller != null)
        {
            controller.ChangeLanguage(langCode);
        }
        CloseLanguagePopup();
    }

    /// <summary>
    /// Updates language checkmarks based on current language
    /// </summary>
    private void UpdateLanguageCheckmarks()
    {
        if (LocalizationManager.Instance == null) return;

        string currentLang = LocalizationManager.Instance.CurrentLanguage;

        if (viCheckMark != null)
        {
            viCheckMark.SetActive(currentLang == LocalizationManager.VIETNAMESE);
        }

        if (enCheckMark != null)
        {
            enCheckMark.SetActive(currentLang == LocalizationManager.ENGLISH);
        }
    }
    #endregion
}