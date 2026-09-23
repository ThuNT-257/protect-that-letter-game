using UnityEngine;
using UnityEngine.UI;

public class CollisionUI : MonoBehaviour {
    #region Singleton
    public static CollisionUI Instance { get; private set; }

    private void Awake() {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(gameObject);
        }
    }
    #endregion

    #region Serialized Fields
    [Header("Panels")]
    [SerializeField] private GameObject collisionPanel;

    [Header("Buttons")]
    [SerializeField] private Button quizButton;
    [SerializeField] private Button restartButton;
    #endregion

    #region Lifecycle
    private void Start() {
        if (quizButton != null) {
            quizButton.onClick.AddListener(OnQuizButtonClicked);
        }

        if (restartButton != null) {
            restartButton.onClick.AddListener(OnRestartButtonClicked);
        }

        HidePanel();
    }

    private void OnDestroy() {
        if (quizButton != null) {
            quizButton.onClick.RemoveListener(OnQuizButtonClicked);
        }

        if (restartButton != null) {
            restartButton.onClick.RemoveListener(OnRestartButtonClicked);
        }
    }
    #endregion

    #region Public Methods
    public void ShowPanel() {
        if (collisionPanel != null) {
            collisionPanel.SetActive(true);
        }
    }

    public void HidePanel() {
        if (collisionPanel != null) {
            collisionPanel.SetActive(false);
        }
    }
    #endregion

    #region Event Handlers
    private void OnQuizButtonClicked() {
        HidePanel();
        Time.timeScale = 1f;

        if (QuizManager.Instance != null) {
            QuizManager.Instance.StartQuiz();
        }
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