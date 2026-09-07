using UnityEngine;

/// <summary>
/// Manages game settings such as audio preferences and language selection.
/// Delegates audio muting directly to the AudioManager runtime state.
/// </summary>
public class SettingsController : MonoBehaviour {
    #region Instance
    private static SettingsController instance;

    public static SettingsController Instance {
        get {
            if (instance == null) {
                instance = FindAnyObjectByType<SettingsController>();
                if (instance == null) {
                    Debug.LogError("[SettingsController] There is no SettingsController in Scene.");
                }
            }
            return instance;
        }
    }
    #endregion

    #region Properties
    public bool IsBGMOn { get; private set; } = true;
    public bool IsSFXOn { get; private set; } = true;
    #endregion

    #region Lifecycle
    private void Awake() {
        if (instance != null && instance != this) {
            Destroy(this.gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start() {
        ApplyAudioSettings();
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Sets the BGM (Background Music) state and applies it directly to AudioManager.
    /// </summary>
    /// <param name="isOn">True to enable BGM, false to disable</param>
    public void SetBGM(bool isOn) {
        IsBGMOn = isOn;
        ApplyAudioSettings();
        Debug.Log($"[SettingsController] BGM State Changed: {isOn}");
    }

    /// <summary>
    /// Sets the SFX (Sound Effects) state and applies it directly to AudioManager.
    /// </summary>
    /// <param name="isOn">True to enable SFX, false to disable</param>
    public void SetSFX(bool isOn) {
        IsSFXOn = isOn;
        ApplyAudioSettings();
        Debug.Log($"[SettingsController] SFX State Changed: {isOn}");
    }

    /// <summary>
    /// Changes the application's language using the LocalizationManager.
    /// </summary>
    /// <param name="langCode">Language code (e.g., "vi", "en", "ja")</param>
    public void ChangeLanguage(string langCode) {
        if (LocalizationManager.Instance != null) {
            LocalizationManager.Instance.SwitchLanguage(langCode);
        }
    }
    #endregion

    #region Private Methods
    /// <summary>
    /// Applies the current audio settings directly to AudioManager.
    /// Note: AudioSource.mute requires 'true' to mute, so we invert the 'isOn' state (!IsBGMOn).
    /// </summary>
    private void ApplyAudioSettings() {
        if (AudioManager.Instance != null) {
            AudioManager.Instance.SetBGMMute(!IsBGMOn);
            AudioManager.Instance.SetSFXMute(!IsSFXOn);
        } else {
            Debug.LogWarning("[SettingsController] AudioManager Instance not found to apply settings.");
        }
    }
    #endregion
}