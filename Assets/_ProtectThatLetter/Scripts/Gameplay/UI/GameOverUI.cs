using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour {
    #region Serialized Fields
    [Header("Panels & Titles")]
    [SerializeField] private GameObject gameOverOverlay;
    [SerializeField] private TextMeshProUGUI gameOverTitleText;

    [Header("Comfort Content Display")]
    [SerializeField] private TextMeshProUGUI comfortText;
    [SerializeField] private Image comfortThumbnailImage;

    [Header("Comfort Data Lists")]
    [TextArea(2, 4)]
    [SerializeField] private List<string> comfortTextList = new List<string>();
    [SerializeField] private List<Sprite> comfortThumbnailList = new List<Sprite>();

    [Header("Buttons")]
    [SerializeField] private Button restartButton;
    #endregion

    #region Lifecycle
    private void Awake() {
        HidePanel();
    }

    private void Start() {
        if (restartButton != null) {
            restartButton.onClick.AddListener(OnRestartButtonClicked);
        }
    }
    #endregion

    #region Public Methods
    public void ShowPanel() {
        DisplayRandomComfortContent();

        if (gameOverOverlay != null) {
            gameOverOverlay.SetActive(true);
        }
    }

    public void HidePanel() {
        if (gameOverOverlay != null) {
            gameOverOverlay.SetActive(false);
        }
    }
    #endregion

    #region Private Methods
    private void DisplayRandomComfortContent() {
        if (comfortText != null && comfortTextList != null && comfortTextList.Count > 0) {
            int randomTextIndex = Random.Range(0, comfortTextList.Count);
            comfortText.text = comfortTextList[randomTextIndex];
        }

        if (comfortThumbnailImage != null && comfortThumbnailList != null && comfortThumbnailList.Count > 0) {
            int randomSpriteIndex = Random.Range(0, comfortThumbnailList.Count);
            comfortThumbnailImage.sprite = comfortThumbnailList[randomSpriteIndex];
        }
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