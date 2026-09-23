using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ProtectThatLetter.Managers;

public class GameOverUI : MonoBehaviour {
    #region Serialized Fields
    [Header("Panels")]
    [SerializeField] private GameObject gameOverOverlay;

    [Header("Comfort Content Display")]
    [SerializeField] private TextMeshProUGUI comfortText;
    [SerializeField] private Image comfortThumbnailImage;

    [Header("Comfort Data Lists")]
    [TextArea(2, 4)]
    [SerializeField] private List<string> vietnameseComfortTextList = new List<string>();
    [TextArea(2, 4)]
    [SerializeField] private List<string> englishComfortTextList = new List<string>();

    [SerializeField] private List<Sprite> comfortThumbnailList = new List<Sprite>();

    [Header("Buttons")]
    [SerializeField] private Button restartButton;
    #endregion

    #region Private Fields
    private int currentTextIndex = -1;
    private int currentSpriteIndex = -1;
    #endregion

    #region Lifecycle
    private void Awake() {
        HidePanel();
    }

    private void OnEnable() {
        LocalizationManager.OnLanguageChanged += HandleLanguageChanged;
    }

    private void OnDisable() {
        LocalizationManager.OnLanguageChanged -= HandleLanguageChanged;
    }

    private void Start() {
        if (restartButton != null) {
            restartButton.onClick.AddListener(OnRestartButtonClicked);
        } else {
            Debug.LogWarning("[GameOverUI] Restart Button reference is Missing!");
        }
    }
    #endregion

    #region Public Methods
    public void ShowPanel() {
        currentTextIndex = -1;
        currentSpriteIndex = -1;

        DisplayComfortContent();

        if (gameOverOverlay != null) {
            gameOverOverlay.SetActive(true);
        } else {
            Debug.LogError("[GameOverUI] gameOverOverlay field is Missing in Inspector!");
        }
    }

    public void HidePanel() {
        if (gameOverOverlay != null) {
            gameOverOverlay.SetActive(false);
        }
    }
    #endregion

    #region Event Handlers & Private Methods
    private void HandleLanguageChanged(string langCode) {
        bool isOverlayActive = gameOverOverlay != null && gameOverOverlay.activeSelf;

        if (isOverlayActive) {
            RefreshComfortText();
        }
    }

    private void DisplayComfortContent() {
        List<string> activeTextList = GetActiveComfortTextList();

        if (comfortText == null) {
            Debug.LogError("[GameOverUI] 'comfortText' (TextMeshProUGUI) is NOT assigned in Inspector!");
        } else if (activeTextList == null || activeTextList.Count == 0) {
            Debug.LogWarning("[GameOverUI] Active comfort text list is EMPTY or NULL!");
        } else {
            currentTextIndex = Random.Range(0, activeTextList.Count);
            comfortText.text = activeTextList[currentTextIndex];
        }

        if (comfortThumbnailImage != null && comfortThumbnailList != null && comfortThumbnailList.Count > 0) {
            currentSpriteIndex = Random.Range(0, comfortThumbnailList.Count);
            comfortThumbnailImage.sprite = comfortThumbnailList[currentSpriteIndex];
        }
    }

    private void RefreshComfortText() {
        List<string> activeTextList = GetActiveComfortTextList();

        if (comfortText == null || activeTextList == null || activeTextList.Count == 0) {
            return;
        }

        if (currentTextIndex < 0 || currentTextIndex >= activeTextList.Count) {
            currentTextIndex = 0;
        }

        comfortText.text = activeTextList[currentTextIndex];
    }

    private List<string> GetActiveComfortTextList() {
        if (LocalizationManager.Instance == null) {
            return vietnameseComfortTextList;
        }

        string currentCode = LocalizationManager.Instance.CurrentLanguageCode;

        if (currentCode.Equals(LocalizationManager.ENGLISH, System.StringComparison.OrdinalIgnoreCase)) {
            return englishComfortTextList;
        }

        return vietnameseComfortTextList;
    }

    private void OnRestartButtonClicked() {
        HidePanel();
        Time.timeScale = 1f;

        if (GameManager.Instance != null) {
            GameManager.Instance.RestartGame();
        }
    }
    #endregion
}