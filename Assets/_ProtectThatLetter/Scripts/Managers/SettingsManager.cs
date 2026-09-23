using UnityEngine;
using ProtectThatLetter.Managers;

/// <summary>
/// Manages game settings such as audio preferences and language selection.
/// Delegates audio muting directly to the AudioManager runtime state and updates LocalizationManager.
/// </summary>
public class SettingsManager : MonoBehaviour {
    #region Constants
    private const string BGM_KEY = "BGM_STATE";
    private const string SFX_KEY = "SFX_STATE";
    private const string LANG_KEY = "LANGUAGE_CODE";
    #endregion

    #region Instance
    private static SettingsManager instance;

    public static SettingsManager Instance {
        get {
            if (instance == null) {
                instance = FindAnyObjectByType<SettingsManager>();
                if (instance == null) {
                    Debug.LogError("[SettingsManager] There is no SettingsManager in Scene.");
                }
            }
            return instance;
        }
    }
    #endregion

    #region Properties
    public bool IsBGMOn { get; private set; } = true;
    public bool IsSFXOn { get; private set; } = true;
    public string CurrentLanguageCode { get; private set; } = LocalizationManager.VIETNAMESE;
    #endregion

    #region Lifecycle
    private void Awake() {
        if (instance != null && instance != this) {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        LoadSettings();
    }

    private void Start() {
        ApplyAudioSettings();
        ApplyLanguageSettings();
    }
    #endregion

    #region Public Methods
    public void SetBGM(bool isOn) {
        IsBGMOn = isOn;
        SaveSettings();
        ApplyAudioSettings();
        Debug.Log($"[SettingsManager] BGM State Changed: {isOn}");
    }

    public void SetSFX(bool isOn) {
        IsSFXOn = isOn;
        SaveSettings();
        ApplyAudioSettings();
        Debug.Log($"[SettingsManager] SFX State Changed: {isOn}");
    }

    public void ChangeLanguage(string langCode) {
        CurrentLanguageCode = langCode;
        SaveSettings();

        if (LocalizationManager.Instance != null) {
            LocalizationManager.Instance.SwitchLanguage(langCode);
        } else {
            Debug.LogWarning("[SettingsManager] LocalizationManager Instance not found.");
        }
    }

    public void ApplyAudioSettings() {
        if (AudioManager.Instance != null) {
            AudioManager.Instance.SetBGMMute(!IsBGMOn);
            AudioManager.Instance.SetSFXMute(!IsSFXOn);
        } else {
            Debug.LogWarning("[SettingsManager] AudioManager Instance not found to apply settings.");
        }
    }

    public void ApplyLanguageSettings() {
        if (LocalizationManager.Instance != null) {
            StartCoroutine(LocalizationManager.Instance.InitializeRoutine(CurrentLanguageCode));
        } else {
            Debug.LogWarning("[SettingsManager] LocalizationManager Instance not found to apply language.");
        }
    }
    #endregion

    #region Private Methods
    private void SaveSettings() {
        PlayerPrefs.SetInt(BGM_KEY, IsBGMOn ? 1 : 0);
        PlayerPrefs.SetInt(SFX_KEY, IsSFXOn ? 1 : 0);
        PlayerPrefs.SetString(LANG_KEY, CurrentLanguageCode);
        PlayerPrefs.Save();
    }

    private void LoadSettings() {
        IsBGMOn = PlayerPrefs.GetInt(BGM_KEY, 1) == 1;
        IsSFXOn = PlayerPrefs.GetInt(SFX_KEY, 1) == 1;
        CurrentLanguageCode = PlayerPrefs.GetString(LANG_KEY, LocalizationManager.VIETNAMESE);
    }
    #endregion
}