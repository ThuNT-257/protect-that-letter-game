using UnityEngine;

public class SettingsController : MonoBehaviour
{
    private const string BGM_KEY = "Settings_BGM";
    private const string SFX_KEY = "Settings_SFX";

    public bool IsBGMOn { get; private set; } = true;
    public bool IsSFXOn { get; private set; } = true;

    private void Awake()
    {
        LoadSettings();
    }

    private void LoadSettings()
    {
        IsBGMOn = PlayerPrefs.GetInt(BGM_KEY, 1) == 1;
        IsSFXOn = PlayerPrefs.GetInt(SFX_KEY, 1) == 1;

        ApplyAudioSettings();
    }

    public void SetBGM(bool isOn)
    {
        IsBGMOn = isOn;
        PlayerPrefs.SetInt(BGM_KEY, isOn ? 1 : 0);
        PlayerPrefs.Save();

        ApplyAudioSettings();
        Debug.Log($"[SettingsController] BGM State Changed: {isOn}");
    }

    public void SetSFX(bool isOn)
    {
        IsSFXOn = isOn;
        PlayerPrefs.SetInt(SFX_KEY, isOn ? 1 : 0);
        PlayerPrefs.Save();

        ApplyAudioSettings();
        Debug.Log($"[SettingsController] SFX State Changed: {isOn}");
    }

    private void ApplyAudioSettings()
    {
        //Add Audio Manager later
    }

    public void ChangeLanguage(string langCode)
    {
        if (LocalizationManager.Instance != null)
        {
            LocalizationManager.Instance.SwitchLanguage(langCode);
        }
    }
}
