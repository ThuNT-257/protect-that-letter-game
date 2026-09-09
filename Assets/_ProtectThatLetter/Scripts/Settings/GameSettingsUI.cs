using UnityEngine;
using UnityEngine.UI;

public class GameSettingsUI : MonoBehaviour {
    #region Serialized Fields
    [Header("Controller References")]
    [SerializeField] private SettingsManager controller;

    [Header("UI Panels")]
    [SerializeField] private GameObject overlay;
    [SerializeField] private GameObject settingsPopup;

    [Header("Action Buttons")]
    [SerializeField] private Button pauseButton;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button restartButton;

    [Header("Sound On/Off Buttons")]
    [SerializeField] private Button bgmButton;
    [SerializeField] private Sprite bgmOnSprite;
    [SerializeField] private Sprite bgmOffSprite;

    [SerializeField] private Button sfxButton;
    [SerializeField] private Sprite sfxOnSprite;
    [SerializeField] private Sprite sfxOffSprite;
    #endregion

    #region Private Fields
    private bool isBGMOn = true;
    private bool isSFXOn = true;
    #endregion

    #region Lifecycle
    private void Start() {
        if (controller == null) {
            controller = GetComponent<SettingsManager>();
        }

        SetOverlayActive(false);

        // Action Buttons
        if (pauseButton != null) pauseButton.onClick.AddListener(OnClickPause);
        if (resumeButton != null) resumeButton.onClick.AddListener(OnClickResume);
        if (restartButton != null) restartButton.onClick.AddListener(OnClickRestart);

        // Sound Buttons
        if (bgmButton != null) bgmButton.onClick.AddListener(OnClickBGM);
        if (sfxButton != null) sfxButton.onClick.AddListener(OnClickSFX);
    }

    private void OnEnable() {
        SyncSoundUI();
    }

    private void OnDestroy() {
        if (pauseButton != null) pauseButton.onClick.RemoveListener(OnClickPause);
        if (resumeButton != null) resumeButton.onClick.RemoveListener(OnClickResume);
        if (restartButton != null) restartButton.onClick.RemoveListener(OnClickRestart);

        if (bgmButton != null) bgmButton.onClick.RemoveListener(OnClickBGM);
        if (sfxButton != null) sfxButton.onClick.RemoveListener(OnClickSFX);
    }
    #endregion

    #region Private Methods
    private void SetOverlayActive(bool isActive) {
        if (overlay != null) {
            overlay.SetActive(isActive);
        }

        if (settingsPopup != null) {
            settingsPopup.SetActive(isActive);
        }
    }

    private void OnClickPause() {
        if (GameManager.Instance != null) {
            GameManager.Instance.PauseGame();
        }

        SyncSoundUI();
        SetOverlayActive(true);

        if (pauseButton != null) pauseButton.gameObject.SetActive(false);
    }

    private void OnClickResume() {
        if (GameManager.Instance != null) {
            GameManager.Instance.ResumeGame();
        }

        SetOverlayActive(false);

        if (pauseButton != null) pauseButton.gameObject.SetActive(true);
    }

    private void OnClickRestart() {
        Time.timeScale = 1f;
        SetOverlayActive(false);

        if (pauseButton != null) {
            pauseButton.gameObject.SetActive(true);
        }

        if (GameManager.Instance != null) {
            GameManager.Instance.ResumeGame();
            GameManager.Instance.RestartGame();
        }
    }

    private void OnClickBGM() {
        isBGMOn = !isBGMOn;
        UpdateBGMVisual();

        if (controller != null) {
            controller.SetBGM(isBGMOn);
        } else if (SettingsManager.Instance != null) {
            SettingsManager.Instance.SetBGM(isBGMOn);
        }
    }

    private void OnClickSFX() {
        isSFXOn = !isSFXOn;
        UpdateSFXVisual();

        if (controller != null) {
            controller.SetSFX(isSFXOn);
        } else if (SettingsManager.Instance != null) {
            SettingsManager.Instance.SetSFX(isSFXOn);
        }
    }

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
    #endregion
}