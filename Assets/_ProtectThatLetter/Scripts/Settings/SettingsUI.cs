using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsUI : MonoBehaviour
{
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

    private bool isBGMOn = true;
    private bool isSFXOn = true;

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

        // Sound On/Off Buttons
        if (bgmButton != null) bgmButton.onClick.AddListener(ToggleBGM);
        if (sfxButton != null) sfxButton.onClick.AddListener(ToggleSFX);

        // Language Buttons
        if (languageSelectorButton != null) languageSelectorButton.onClick.AddListener(OpenLanguagePopup);
        if (langCloseButton != null) langCloseButton.onClick.AddListener(CloseLanguagePopup);
        if (langOverlayCloseButton != null) langOverlayCloseButton.onClick.AddListener(CloseLanguagePopup);

        if (btnVietnamese != null) btnVietnamese.onClick.AddListener(() => OnSelectLanguage("vi"));
        if (btnEnglish != null) btnEnglish.onClick.AddListener(() => OnSelectLanguage("en"));

        CloseLanguagePopup();
        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        LocalizationManager.OnLanguageChanged += UpdateLanguageCheckmarks;

        SyncSoundUI();
        UpdateLanguageCheckmarks();
    }

    private void OnDisable()
    {
        LocalizationManager.OnLanguageChanged -= UpdateLanguageCheckmarks;
    }

    private void OpenPopup()
    {
        gameObject.SetActive(true);
        SyncSoundUI();
        CloseLanguagePopup();
    }

    private void ClosePopup()
    {
        Debug.Log("[SettingsUI] ClosePopup triggered!");
        CloseLanguagePopup();
        gameObject.SetActive(false);
    }

    private void OpenLanguagePopup()
    {
        if (langDropdownPanel != null)
        {
            langDropdownPanel.SetActive(true);
            UpdateLanguageCheckmarks();
        }
    }

    private void CloseLanguagePopup()
    {
        Debug.Log("[SettingsUI] CloseLanguagePopup triggered!");
        if (langDropdownPanel != null)
        {
            langDropdownPanel.SetActive(false);
        }
    }

    // Toggle BGM
    private void ToggleBGM()
    {
        isBGMOn = !isBGMOn;
        UpdateBGMVisual();

        if (controller != null)
        {
            controller.SetBGM(isBGMOn);
        }
    }

    // Toggle SFX
    private void ToggleSFX()
    {
        isSFXOn = !isSFXOn;
        UpdateSFXVisual();

        if (controller != null)
        {
            controller.SetSFX(isSFXOn);
        }
    }

    private void SyncSoundUI()
    {
        if (controller != null)
        {
             isBGMOn = controller.IsBGMOn;
             isSFXOn = controller.IsSFXOn;
        }

        UpdateBGMVisual();
        UpdateSFXVisual();
    }

    private void UpdateBGMVisual()
    {
        if (bgmButton != null && bgmButton.image != null)
        {
            bgmButton.image.sprite = isBGMOn ? bgmOnSprite : bgmOffSprite;
        }
    }

    private void UpdateSFXVisual()
    {
        if (sfxButton != null && sfxButton.image != null)
        {
            sfxButton.image.sprite = isSFXOn ? sfxOnSprite : sfxOffSprite;
        }
    }

    private void OnSelectLanguage(string langCode)
    {
        if (controller != null)
        {
            controller.ChangeLanguage(langCode);
        }
        CloseLanguagePopup();
    }

    private void UpdateLanguageCheckmarks()
    {
        if (LocalizationManager.Instance == null) return;

        string currentLang = LocalizationManager.Instance.CurrentLanguage;

        if (viCheckMark != null)
        {
            viCheckMark.SetActive(currentLang == "vi");
        }

        if (enCheckMark != null)
        {
            enCheckMark.SetActive(currentLang == "en");
        }
    }
}