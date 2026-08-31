using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class CollisionUI : MonoBehaviour
{
    #region Serialized Fields
    [Header("Panels")]
    [SerializeField] private GameObject collisionPanel;

    [Header("Buttons")]
    [SerializeField] private Button quizButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button homeButton;
    #endregion

    #region Lifecycle
    private void Start()
    {
        if(quizButton != null)
        {
            quizButton.onClick.AddListener(OnQuizButtonClicked);
        }

        if (restartButton != null)
        {
            restartButton.onClick.AddListener(OnRestartButtonClicked);
        }

        if (homeButton != null)
        {
            homeButton.onClick.AddListener(OnHomeButtonClicked);
        }

        HidePanel();
    }
    #endregion

    #region Public Methods
    public void ShowPanel()
    {
        if (collisionPanel != null)
        {
            collisionPanel.SetActive(true);
        }
    }

    public void HidePanel()
    {
        if (collisionPanel != null)
        {
            collisionPanel.SetActive(false);
        }
    }
    #endregion

    #region Event Handlers
    private void OnQuizButtonClicked()
    {
        HidePanel();
        QuizManager.Instance.StartQuiz();
    }

    private void OnRestartButtonClicked()
    {
        HidePanel();
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RestartGame();
        }
    }

    private void OnHomeButtonClicked()
    {
        Time.timeScale = 1f;
        if (SceneController.Instance != null)
        {
            SceneController.Instance.LoadScene(SceneController.LOGIN_SCENE);
        }
    }
    #endregion
}
