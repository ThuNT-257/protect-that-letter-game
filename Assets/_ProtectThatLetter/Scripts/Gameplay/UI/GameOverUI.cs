using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ProtectThatLetter.Managers;
using ProtectThatLetter.Definitions;

/// <summary>
/// Manages the Game Over panel: displays comfort content (text + thumbnail)
/// and handles the restart button.
/// </summary>
public class GameOverUI : MonoBehaviour {
    #region Serialized Fields
    [Header("Panels")]
    [SerializeField] private GameObject gameOverOverlay; // The main overlay panel

    [Header("Comfort Content Display")]
    [SerializeField] private TextMeshProUGUI comfortText;          // Comfort message text
    [SerializeField] private Image comfortThumbnailImage;          // Comfort thumbnail image

    [Header("Comfort Data Lists")]
    [TextArea(2, 4)]
    [SerializeField] private List<string> vietnameseComfortTextList = new List<string>(); // Vietnamese comfort texts
    [TextArea(2, 4)]
    [SerializeField] private List<string> englishComfortTextList = new List<string>();    // English comfort texts

    [SerializeField] private List<Sprite> comfortThumbnailList = new List<Sprite>();      // Thumbnail sprites

    [Header("Buttons")]
    [SerializeField] private Button restartButton; // Restart button
    #endregion

    #region Private Fields
    private int currentTextIndex = -1;    // Index of the currently displayed comfort text
    private int currentSpriteIndex = -1;  // Index of the currently displayed thumbnail
    #endregion

    #region Lifecycle
    /// <summary>
    /// Hides the panel on startup
    /// </summary>
    private void Awake() {
        HidePanel();
    }

    /// <summary>
    /// Subscribes to language change events when enabled
    /// </summary>
    private void OnEnable() {
        LocalizationManager.OnLanguageChanged += HandleLanguageChanged;
    }

    /// <summary>
    /// Unsubscribes from language change events to prevent memory leaks
    /// </summary>
    private void OnDisable() {
        LocalizationManager.OnLanguageChanged -= HandleLanguageChanged;
    }

    /// <summary>
    /// Adds the restart button listener after all Awake calls complete
    /// </summary>
    private void Start() {
        if (restartButton != null) {
            restartButton.onClick.AddListener(OnRestartButtonClicked);
        } else {
            Debug.LogWarning("[GameOverUI] Restart Button reference is Missing!");
        }
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Shows the Game Over panel with fresh random comfort content
    /// </summary>
    public void ShowPanel() {
        currentTextIndex = -1;   // Reset so a new random text is picked
        currentSpriteIndex = -1; // Reset so a new random sprite is picked

        DisplayComfortContent();

        if (gameOverOverlay != null) {
            gameOverOverlay.SetActive(true);
        } else {
            Debug.LogError("[GameOverUI] gameOverOverlay field is Missing in Inspector!");
        }
    }

    /// <summary>
    /// Hides the Game Over panel
    /// </summary>
    public void HidePanel() {
        if (gameOverOverlay != null) {
            gameOverOverlay.SetActive(false);
        }
    }
    #endregion

    #region Event Handlers & Private Methods
    /// <summary>
    /// Refreshes the comfort text when the language changes (only if panel is visible)
    /// </summary>
    private void HandleLanguageChanged(string langCode) {
        bool isOverlayActive = gameOverOverlay != null && gameOverOverlay.activeSelf;

        if (isOverlayActive) {
            RefreshComfortText();
        }
    }

    /// <summary>
    /// Picks and displays random comfort text and thumbnail
    /// </summary>
    private void DisplayComfortContent() {
        List<string> activeTextList = GetActiveComfortTextList();

        // Set comfort text
        if (comfortText == null) {
            Debug.LogError("[GameOverUI] 'comfortText' (TextMeshProUGUI) is NOT assigned in Inspector!");
        } else if (activeTextList == null || activeTextList.Count == 0) {
            Debug.LogWarning("[GameOverUI] Active comfort text list is EMPTY or NULL!");
        } else {
            currentTextIndex = Random.Range(0, activeTextList.Count);
            comfortText.text = activeTextList[currentTextIndex];
        }

        // Set comfort thumbnail
        if (comfortThumbnailImage != null && comfortThumbnailList != null && comfortThumbnailList.Count > 0) {
            currentSpriteIndex = Random.Range(0, comfortThumbnailList.Count);
            comfortThumbnailImage.sprite = comfortThumbnailList[currentSpriteIndex];
        }
    }

    /// <summary>
    /// Refreshes only the comfort text (keeps the same text index when language changes)
    /// </summary>
    private void RefreshComfortText() {
        List<string> activeTextList = GetActiveComfortTextList();

        // Validate references
        if (comfortText == null || activeTextList == null || activeTextList.Count == 0) {
            return;
        }

        // Clamp index in case the new list is shorter
        if (currentTextIndex < 0 || currentTextIndex >= activeTextList.Count) {
            currentTextIndex = 0;
        }

        comfortText.text = activeTextList[currentTextIndex];
    }

    /// <summary>
    /// Returns the correct comfort text list based on the current language
    /// </summary>
    private List<string> GetActiveComfortTextList() {
        // Fallback to Vietnamese if LocalizationManager is missing
        if (LocalizationManager.Instance == null) {
            return vietnameseComfortTextList;
        }

        string currentCode = LocalizationManager.Instance.CurrentLanguageCode;

        // Return English list if current language is English
        if (currentCode.Equals(GameDefinitions.Languages.ENGLISH, System.StringComparison.OrdinalIgnoreCase)) {
            return englishComfortTextList;
        }

        return vietnameseComfortTextList; // Default fallback
    }

    /// <summary>
    /// Handles the restart button click: hides panel, resumes time, restarts game
    /// </summary>
    private void OnRestartButtonClicked() {
        HidePanel();
        Time.timeScale = 1f; // Resume time (in case it was paused)

        if (GameManager.Instance != null) {
            GameManager.Instance.RestartGame();
        }
    }
    #endregion
}