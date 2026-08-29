using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuizManager : MonoBehaviour
{
    #region Instance
    public static QuizManager Instance { get; private set; }
    #endregion

    #region Serialized Fields
    [Header("UI References")]
    [SerializeField] private QuizUI quizUI;
    [SerializeField] private GameOverUI gameOverUI;

    [Header("Clear Obstacles Settings")]
    [SerializeField] private Transform birdTransform;
    [SerializeField] private float destroyRadius = 5.0f;
    [SerializeField] private LayerMask obstacleLayer;
    #endregion

    #region Lifecycle
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    #endregion

    #region Public Methods
    public void StartQuiz()
    {
        if (questionList == null || questionList.Count == 0)
        {
            Debug.LogWarning("[QuizManager] No questions available!");
            return;
        }

        Time.timeScale = 0f;

        int randomIndex = Random.Range(0, questionList.Count);
        currentQuestion = questionList[randomIndex];

        if (quizUI != null)
        {
            quizUI.DisplayQuestion(currentQuestion.question, currentQuestion.options, OnAnswerSubmitted);
        }
    }
    #endregion

    #region Private Methods
    private void OnAnswerSubmitted(int selectedIndex)
    {
        bool isCorrect = (selectedIndex == currentQuestion.correctIndex);

        if (quizUI != null)
        {
            quizUI.ShowAnswerResult(selectedIndex, currentQuestion.correctIndex);
        }

        StartCoroutine(HandleQuizResultRoutine(isCorrect));
    }

    private IEnumerator HandleQuizResultRoutine(bool isCorrect)
    {
        yield return new WaitForSecondsRealtime(2.0f);

        if (isCorrect)
        {
            if (quizUI != null)
            {
                quizUI.HidePanel();
            }

            DestroyNearbyObstacles();

            if (quizUI != null)
            {
                yield return StartCoroutine(quizUI.StartCountdownRoutine(3f));
            }

            Time.timeScale = 1f;
        }
        else
        {
            if (quizUI != null)
            {
                quizUI.HidePanel();
            }

            if (gameOverUI != null)
            {
                gameOverUI.ShowPanel();
            }
        }
    }

    private void DestroyNearbyObstacles()
    {
        if (birdTransform == null) return;

        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(birdTransform.position, destroyRadius, obstacleLayer);

        foreach (var hit in hitColliders)
        {
            if (hit.CompareTag("Obstacles"))
            {
                Destroy(hit.gameObject);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (birdTransform != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(birdTransform.position, destroyRadius);
        }
    }
    #endregion

    #region Nested Types / Data Types
    [System.Serializable]
    public struct QuizQuestion
    {
        public string question;
        public string[] options;
        public int correctIndex;
    }

    [Header("Sample Data")]
    [SerializeField] private List<QuizQuestion> questionList = new List<QuizQuestion>();
    private QuizQuestion currentQuestion;
    #endregion
}
