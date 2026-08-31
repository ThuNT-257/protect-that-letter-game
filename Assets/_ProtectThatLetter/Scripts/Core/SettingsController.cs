using UnityEngine;

/// <summary>
/// Manages game settings such as audio preferences and language selection.
/// Saves settings persistently using PlayerPrefs.
/// </summary>
public class SettingsController : MonoBehaviour
{
    #region Constants
    private const string BGM_KEY = "Settings_BGM";
    private const string SFX_KEY = "Settings_SFX";
    #endregion

    #region Instance
    private static SettingsController instance;

    public static SettingsController Instance 
    {
        get 
        {
            if (instance == null) 
            {
                instance = FindAnyObjectByType<SettingsController>();
                if (instance == null) 
                {
                    Debug.LogError("There is no SettingsController in Scene.");
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
    /// <summary>
    /// Loads saved settings from PlayerPrefs.
    /// </summary>
    private void Awake() 
    {
        if (instance != null && instance != this) 
        {
            Destroy(this.gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        LoadSettings();
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Sets the BGM (Background Music) state.
    /// Saves the preference and applies it immediately.
    /// </summary>
    /// <param name="isOn">True to enable BGM, false to disable</param>
    public void SetBGM(bool isOn) 
    {
        IsBGMOn = isOn;
        PlayerPrefs.SetInt(BGM_KEY, isOn ? 1 : 0);
        PlayerPrefs.Save();

        ApplyAudioSettings();
        Debug.Log($"[SettingsController] BGM State Changed: {isOn}");
    }

    /// <summary>
    /// Sets the SFX (Sound Effects) state.
    /// Saves the preference and applies it immediately.
    /// </summary>
    /// <param name="isOn">True to enable SFX, false to disable</param>
    public void SetSFX(bool isOn) 
    {
        IsSFXOn = isOn;
        PlayerPrefs.SetInt(SFX_KEY, isOn ? 1 : 0);
        PlayerPrefs.Save();

        ApplyAudioSettings();
        Debug.Log($"[SettingsController] SFX State Changed: {isOn}");
    }

    /// <summary>
    /// Changes the application's language using the LocalizationManager.
    /// </summary>
    /// <param name="langCode">Language code (e.g., "vi", "en", "ja")</param>
    public void ChangeLanguage(string langCode) 
    {
        if (LocalizationManager.Instance != null) 
        {
            LocalizationManager.Instance.SwitchLanguage(langCode);
        }
    }
    #endregion

    #region Private Methods
    /// <summary>
    /// Loads audio settings from PlayerPrefs.
    /// Defaults to ON (true) if no saved preference exists.
    /// </summary>
    private void LoadSettings()
    {
        IsBGMOn = PlayerPrefs.GetInt(BGM_KEY, 1) == 1;
        IsSFXOn = PlayerPrefs.GetInt(SFX_KEY, 1) == 1;

        ApplyAudioSettings();
    }

    /// <summary>
    /// Applies the current audio settings to the audio system.
    /// TODO: Integrate with an AudioManager when implemented.
    /// </summary>
    private void ApplyAudioSettings() 
    {
        //Will add Audio Manager later
    }
    #endregion
}
