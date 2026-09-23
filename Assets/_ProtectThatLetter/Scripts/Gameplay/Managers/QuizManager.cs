using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProtectThatLetter.Managers;

public class QuizManager : MonoBehaviour {
    #region Instance
    public static QuizManager Instance { get; private set; }
    #endregion

    #region Serialized Fields
    [Header("UI References")]
    [SerializeField] private QuizUI quizUI;
    [SerializeField] private GameOverUI gameOverUI;
    [SerializeField] private CanvasGroup mainHUDCanvasGroup;

    [Header("Clear Obstacles Settings")]
    [SerializeField] private Transform birdTransform;
    [SerializeField] private float destroyRadius = 2.0f;
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private GameObject explosionFXPrefab;

    [Header("Localization Data Lists")]
    [SerializeField] private List<QuizQuestion> vietnameseQuestions = new List<QuizQuestion>();
    [SerializeField] private List<QuizQuestion> englishQuestions = new List<QuizQuestion>();
    #endregion

    #region Private Fields
    private Coroutine quizRoutine;
    private int currentQuestionIndex = -1;
    private bool isQuizActive = false;
    #endregion

    #region Properties
    public bool IsCountingDown { get; private set; } = false;
    #endregion

    #region Lifecycle
    private void Awake() {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(gameObject);
        }
    }

    private void OnEnable() {
        LocalizationManager.OnLanguageChanged += HandleLanguageChanged;
    }

    private void OnDisable() {
        LocalizationManager.OnLanguageChanged -= HandleLanguageChanged;
    }
    #endregion

    #region Public Methods
    public void StartQuiz() {
        List<QuizQuestion> activeQuestionList = GetActiveQuestionList();

        if (activeQuestionList == null || activeQuestionList.Count == 0) {
            string langCode = LocalizationManager.Instance != null ? LocalizationManager.Instance.CurrentLanguageCode : "unknown";
            Debug.LogWarning($"[QuizManager] No questions available for current language code: {langCode}!");
            return;
        }

        Time.timeScale = 0f;
        isQuizActive = true;

        if (currentQuestionIndex < 0 || currentQuestionIndex >= activeQuestionList.Count) {
            currentQuestionIndex = Random.Range(0, activeQuestionList.Count);
        }

        DisplayCurrentQuestion();
    }

    public void ResetQuizState() {
        IsCountingDown = false;
        isQuizActive = false;
        currentQuestionIndex = -1;
        SetHUDInteraction(true);

        if (quizRoutine != null) {
            StopCoroutine(quizRoutine);
            quizRoutine = null;
        }

        if (quizUI != null) {
            quizUI.HidePanel();
        }
    }
    #endregion

    #region Private Methods
    private void HandleLanguageChanged(string langCode) {
        if (isQuizActive) {
            DisplayCurrentQuestion();
        }
    }

    private void DisplayCurrentQuestion() {
        List<QuizQuestion> activeQuestionList = GetActiveQuestionList();

        if (quizUI != null && activeQuestionList != null && currentQuestionIndex >= 0 && currentQuestionIndex < activeQuestionList.Count) {
            QuizQuestion q = activeQuestionList[currentQuestionIndex];
            quizUI.DisplayQuestion(q.question, q.options, OnAnswerSubmitted);
        }
    }

    private List<QuizQuestion> GetActiveQuestionList() {
        if (LocalizationManager.Instance == null) {
            return vietnameseQuestions;
        }

        string currentCode = LocalizationManager.Instance.CurrentLanguageCode;

        if (currentCode.Equals(LocalizationManager.ENGLISH, System.StringComparison.OrdinalIgnoreCase)) {
            return englishQuestions;
        }

        return vietnameseQuestions;
    }

    private void OnAnswerSubmitted(int selectedIndex) {
        List<QuizQuestion> activeQuestionList = GetActiveQuestionList();
        if (currentQuestionIndex < 0 || currentQuestionIndex >= activeQuestionList.Count) return;

        QuizQuestion currentQuestion = activeQuestionList[currentQuestionIndex];
        bool isCorrect = (selectedIndex == currentQuestion.correctIndex);

        if (quizUI != null) {
            quizUI.ShowAnswerResult(selectedIndex, currentQuestion.correctIndex);
        }

        if (quizRoutine != null) StopCoroutine(quizRoutine);
        quizRoutine = StartCoroutine(HandleQuizResultRoutine(isCorrect));
    }

    private IEnumerator HandleQuizResultRoutine(bool isCorrect) {
        yield return new WaitForSecondsRealtime(2.0f);

        if (isCorrect) {
            IsCountingDown = true;
            isQuizActive = false;
            SetHUDInteraction(false);

            if (GameManager.Instance != null) {
                GameManager.Instance.HasCompletedQuiz = true;
            }

            if (quizUI != null) {
                quizUI.HidePanel();
            }

            DestroyNearbyObstacles();

            if (quizUI != null) {
                yield return StartCoroutine(quizUI.StartCountdownRoutine(3f));
            }

            IsCountingDown = false;
            SetHUDInteraction(true);
            currentQuestionIndex = -1;

            Time.timeScale = 1f;
        } else {
            IsCountingDown = false;
            isQuizActive = false;
            SetHUDInteraction(true);
            currentQuestionIndex = -1;

            if (quizUI != null) {
                quizUI.HidePanel();
            }

            if (gameOverUI != null) {
                gameOverUI.ShowPanel();
            }
        }
    }

    private void SetHUDInteraction(bool interactable) {
        if (mainHUDCanvasGroup != null) {
            mainHUDCanvasGroup.blocksRaycasts = interactable;
            mainHUDCanvasGroup.interactable = interactable;
        }
    }

    private void DestroyNearbyObstacles() {
        if (birdTransform == null) return;

        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(birdTransform.position, destroyRadius, obstacleLayer);

        foreach (var hit in hitColliders) {
            if (hit.CompareTag("Obstacles")) {
                if (explosionFXPrefab != null) {
                    Instantiate(explosionFXPrefab, hit.transform.position, Quaternion.identity);
                }

                Destroy(hit.gameObject);
            }
        }
    }

    private void OnDrawGizmosSelected() {
        if (birdTransform != null) {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(birdTransform.position, destroyRadius);
        }
    }
    #endregion

    #region Nested Types / Data Types
    [System.Serializable]
    public struct QuizQuestion {
        public string question;
        public string[] options;
        public int correctIndex;
    }
    #endregion
}