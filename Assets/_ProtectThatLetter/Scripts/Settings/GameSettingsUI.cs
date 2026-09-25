using ProtectThatLetter.Managers;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages in-game settings UI: pause/resume/restart, BGM/SFX toggles.
/// Delegates settings logic to SettingsManager.
/// </summary>
public class GameSettingsUI : MonoBehaviour {
    #region Serialized Fields
    [Header("Controller References")]
    [SerializeField] private SettingsManager controller; // Reference to SettingsManager

    [Header("UI Panels")]
    [SerializeField] private GameObject overlay;         // Dark overlay behind popup
    [SerializeField] private GameObject settingsPopup;   // Main settings popup

    [Header("Action Buttons")]
    [SerializeField] private Button pauseButton;         // Pause button
    [SerializeField] private Button resumeButton;        // Resume button
    [SerializeField] private Button restartButton;       // Restart button

    [Header("Sound On/Off Buttons")]
    [SerializeField] private Button bgmButton;           // BGM toggle button
    [SerializeField] private Sprite bgmOnSprite;         // BGM ON sprite
    [SerializeField] private Sprite bgmOffSprite;        // BGM OFF sprite

    [SerializeField] private Button sfxButton;           // SFX toggle button
    [SerializeField] private Sprite sfxOnSprite;         // SFX ON sprite
    [SerializeField] private Sprite sfxOffSprite;        // SFX OFF sprite
    #endregion

    #region Private Fields
    // Cached audio states (mirrors SettingsManager)
    private bool isBGMOn = true;
    private bool isSFXOn = true;
    #endregion

    #region Lifecycle
    /// <summary>
    /// Sets up button listeners and initial UI state
    /// </summary>
    private void Start() {
        // Auto-find SettingsManager if not assigned
        if (controller == null) {
            controller = SettingsManager.Instance != null
                ? SettingsManager.Instance
                : GetComponent<SettingsManager>();
        }

        SetOverlayActive(false);

        // Register action button listeners
        if (pauseButton != null) pauseButton.onClick.AddListener(OnClickPause);
        if (resumeButton != null) resumeButton.onClick.AddListener(OnClickResume);
        if (restartButton != null) restartButton.onClick.AddListener(OnClickRestart);

        // Register sound toggle listeners
        if (bgmButton != null) bgmButton.onClick.AddListener(OnClickBGM);
        if (sfxButton != null) sfxButton.onClick.AddListener(OnClickSFX);

        SyncSoundUI();
    }

    /// <summary>
    /// Re-syncs sound UI whenever the object becomes active
    /// </summary>
    private void OnEnable() {
        SyncSoundUI();
    }

    /// <summary>
    /// Removes all button listeners to prevent memory leaks
    /// </summary>
    private void OnDestroy() {
        if (pauseButton != null) pauseButton.onClick.RemoveListener(OnClickPause);
        if (resumeButton != null) resumeButton.onClick.RemoveListener(OnClickResume);
        if (restartButton != null) restartButton.onClick.RemoveListener(OnClickRestart);

        if (bgmButton != null) bgmButton.onClick.RemoveListener(OnClickBGM);
        if (sfxButton != null) sfxButton.onClick.RemoveListener(OnClickSFX);
    }
    #endregion

    #region Private Methods
    /// <summary>
    /// Shows or hides the overlay and settings popup
    /// </summary>
    private void SetOverlayActive(bool isActive) {
        if (overlay != null) {
            overlay.SetActive(isActive);
        }

        if (settingsPopup != null) {
            settingsPopup.SetActive(isActive);
        }
    }

    /// <summary>
    /// Pauses the game and shows the settings popup
    /// </summary>
    private void OnClickPause() {
        if (AudioManager.Instance != null) {
            AudioManager.Instance.PlaySFX("settings_button_click");
        }

        if (GameManager.Instance != null) {
            GameManager.Instance.PauseGame();
        }

        SyncSoundUI();
        SetOverlayActive(true);

        // Hide the pause button while popup is open
        if (pauseButton != null) pauseButton.gameObject.SetActive(false);
    }

    /// <summary>
    /// Resumes the game and hides the settings popup
    /// </summary>
    private void OnClickResume() {
        if (AudioManager.Instance != null) {
            AudioManager.Instance.PlaySFX("button_click");
        }

        if (GameManager.Instance != null) {
            GameManager.Instance.ResumeGame();
        }

        SetOverlayActive(false);

        // Show the pause button again
        if (pauseButton != null) pauseButton.gameObject.SetActive(true);
    }

    /// <summary>
    /// Restarts the game and hides the settings popup
    /// </summary>
    private void OnClickRestart() {
        if (AudioManager.Instance != null) {
            AudioManager.Instance.PlaySFX("button_click");
        }

        Time.timeScale = 1f; // Ensure time is running
        SetOverlayActive(false);

        if (pauseButton != null) {
            pauseButton.gameObject.SetActive(true);
        }

        if (GameManager.Instance != null) {
            GameManager.Instance.ResumeGame();
            GameManager.Instance.RestartGame();
        }
    }

    /// <summary>
    /// Toggles BGM and updates visuals
    /// </summary>
    private void OnClickBGM() {
        if (AudioManager.Instance != null) {
            AudioManager.Instance.PlaySFX("button_click");
        }

        isBGMOn = !isBGMOn;
        UpdateBGMVisual();

        // Send change to SettingsManager (which broadcasts event)
        if (controller != null) {
            controller.SetBGM(isBGMOn);
        } else if (SettingsManager.Instance != null) {
            SettingsManager.Instance.SetBGM(isBGMOn);
        }
    }

    /// <summary>
    /// Toggles SFX and updates visuals
    /// </summary>
    private void OnClickSFX() {
        if (AudioManager.Instance != null) {
            AudioManager.Instance.PlaySFX("button_click");
        }

        isSFXOn = !isSFXOn;
        UpdateSFXVisual();

        // Send change to SettingsManager (which broadcasts event)
        if (controller != null) {
            controller.SetSFX(isSFXOn);
        } else if (SettingsManager.Instance != null) {
            SettingsManager.Instance.SetSFX(isSFXOn);
        }
    }

    /// <summary>
    /// Syncs cached sound state with SettingsManager
    /// </summary>
    private void SyncSoundUI() {
        if (controller != null) {
            isBGMOn = controller.IsBGMOn;
            isSFXOn = controller.IsSFXOn;
        } else if (SettingsManager.Instance != null) {
            isBGMOn = SettingsManager.Instance.IsBGMOn;
            isSFXOn = SettingsManager.Instance.IsSFXOn;
        }

        UpdateBGMVisual();
        UpdateSFXVisual();
    }

    /// <summary>
    /// Updates the BGM button sprite based on cached state
    /// </summary>
    private void UpdateBGMVisual() {
        if (bgmButton != null && bgmButton.image != null) {
            bgmButton.image.sprite = isBGMOn ? bgmOnSprite : bgmOffSprite;
        }
    }

    /// <summary>
    /// Updates the SFX button sprite based on cached state
    /// </summary>
    private void UpdateSFXVisual() {
        if (sfxButton != null && sfxButton.image != null) {
            sfxButton.image.sprite = isSFXOn ? sfxOnSprite : sfxOffSprite;
        }
    }
    #endregion
}