using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

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
        LocalizationManager.OnLanguageChanged += OnLanguageChanged;
        Debug.Log("[GameOverUI] Subscribed to OnLanguageChanged event.");
    }

    private void OnDisable() {
        LocalizationManager.OnLanguageChanged -= OnLanguageChanged;
        Debug.Log("[GameOverUI] Unsubscribed from OnLanguageChanged event.");
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
        Debug.Log("[GameOverUI] ShowPanel() called.");
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

    #region Private Methods
    private void OnLanguageChanged(Locale newLocale) {
        string localeCode = newLocale != null ? newLocale.Identifier.Code : "null";
        Debug.Log($"[GameOverUI] OnLanguageChanged triggered. New Locale: {localeCode}");

        bool isOverlayActive = gameOverOverlay != null && gameOverOverlay.activeSelf;
        Debug.Log($"[GameOverUI] Overlay Active State: {isOverlayActive}, GameObject Active State: {gameObject.activeInHierarchy}");

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
            Debug.Log($"[GameOverUI] Applied initial text index {currentTextIndex}: \"{comfortText.text}\"");
        }

        if (comfortThumbnailImage != null && comfortThumbnailList != null && comfortThumbnailList.Count > 0) {
            currentSpriteIndex = Random.Range(0, comfortThumbnailList.Count);
            comfortThumbnailImage.sprite = comfortThumbnailList[currentSpriteIndex];
        }
    }

    private void RefreshComfortText() {
        List<string> activeTextList = GetActiveComfortTextList();

        if (comfortText == null) {
            Debug.LogError("[GameOverUI] 'comfortText' is NOT assigned when refreshing!");
            return;
        }

        if (activeTextList == null || activeTextList.Count == 0) {
            Debug.LogWarning("[GameOverUI] Target text list is empty during refresh!");
            return;
        }

        if (currentTextIndex < 0 || currentTextIndex >= activeTextList.Count) {
            currentTextIndex = 0;
        }

        string oldText = comfortText.text;
        comfortText.text = activeTextList[currentTextIndex];
        Debug.Log($"[GameOverUI] Refreshed text at index {currentTextIndex}. Old: \"{oldText}\" -> New: \"{comfortText.text}\"");
    }

    private List<string> GetActiveComfortTextList() {
        if (LocalizationManager.Instance == null) {
            Debug.LogWarning("[GameOverUI] LocalizationManager.Instance is NULL! Defaulting to Vietnamese list.");
            return vietnameseComfortTextList;
        }

        string currentCode = LocalizationManager.Instance.CurrentLanguageCode;
        Debug.Log($"[GameOverUI] Fetching list for language code: '{currentCode}'");

        if (currentCode.Equals(LocalizationManager.ENGLISH, System.StringComparison.OrdinalIgnoreCase)) {
            Debug.Log($"[GameOverUI] Returning English List (Count: {englishComfortTextList.Count})");
            return englishComfortTextList;
        }

        Debug.Log($"[GameOverUI] Returning Vietnamese List (Count: {vietnameseComfortTextList.Count})");
        return vietnameseComfortTextList;
    }
    #endregion

    #region Event Handlers
    private void OnRestartButtonClicked() {
        HidePanel();
        Time.timeScale = 1f;

        if (GameManager.Instance != null) {
            GameManager.Instance.RestartGame();
        }
    }
    #endregion
}