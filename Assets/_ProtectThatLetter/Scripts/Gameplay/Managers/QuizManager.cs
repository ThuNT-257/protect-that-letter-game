using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProtectThatLetter.Managers;
using ProtectThatLetter.Definitions;

/// <summary>
/// Manages quiz gameplay: displaying questions, handling answers,
/// and transitioning to either success (clear obstacles) or game over.
/// </summary>
public class QuizManager : MonoBehaviour {
    #region Instance
    // Singleton instance with public getter and private setter
    public static QuizManager Instance { get; private set; }
    #endregion

    #region Serialized Fields
    [Header("UI References")]
    [SerializeField] private QuizUI quizUI;                     // Quiz panel UI
    [SerializeField] private GameOverUI gameOverUI;             // Game over panel UI
    [SerializeField] private CanvasGroup mainHUDCanvasGroup;    // Main HUD for interaction toggling

    [Header("Clear Obstacles Settings")]
    [SerializeField] private Transform birdTransform;           // Bird position reference
    [SerializeField] private float destroyRadius = 2.0f;        // Radius to destroy obstacles
    [SerializeField] private LayerMask obstacleLayer;           // Layer of obstacles
    [SerializeField] private GameObject explosionFXPrefab;      // Explosion VFX prefab

    [Header("Localization Data Lists")]
    [SerializeField] private List<QuizQuestion> vietnameseQuestions = new List<QuizQuestion>(); // Vietnamese questions
    [SerializeField] private List<QuizQuestion> englishQuestions = new List<QuizQuestion>();    // English questions
    #endregion

    #region Private Fields
    private Coroutine quizRoutine;          // Reference to the quiz result coroutine
    private int currentQuestionIndex = -1;  // Index of the current question (-1 = none)
    private bool isQuizActive = false;      // Whether the quiz is currently active
    #endregion

    #region Properties
    // True while the countdown after a correct answer is running
    public bool IsCountingDown { get; private set; } = false;
    #endregion

    #region Lifecycle
    /// <summary>
    /// Ensures singleton integrity
    /// </summary>
    private void Awake() {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(gameObject);
        }
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
    #endregion

    #region Public Methods
    /// <summary>
    /// Starts the quiz: pauses time, picks a random question, and displays it
    /// </summary>
    public void StartQuiz() {
        List<QuizQuestion> activeQuestionList = GetActiveQuestionList();

        // Validate question list
        if (activeQuestionList == null || activeQuestionList.Count == 0) {
            string langCode = LocalizationManager.Instance != null
                ? LocalizationManager.Instance.CurrentLanguageCode
                : "unknown";
            Debug.LogWarning($"[QuizManager] No questions available for current language code: {langCode}!");
            return;
        }

        Time.timeScale = 0f; // Pause the game during the quiz
        isQuizActive = true;

        // Pick a random question if none selected
        if (currentQuestionIndex < 0 || currentQuestionIndex >= activeQuestionList.Count) {
            currentQuestionIndex = Random.Range(0, activeQuestionList.Count);
        }

        DisplayCurrentQuestion();
    }

    /// <summary>
    /// Resets the quiz state (used when restarting or exiting the quiz)
    /// </summary>
    public void ResetQuizState() {
        IsCountingDown = false;
        isQuizActive = false;
        currentQuestionIndex = -1;
        SetHUDInteraction(true);

        // Stop any running quiz coroutine
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
    /// <summary>
    /// Refreshes the current question when the language changes
    /// </summary>
    private void HandleLanguageChanged(string langCode) {
        if (isQuizActive) {
            DisplayCurrentQuestion();
        }
    }

    /// <summary>
    /// Displays the current question on the quiz UI
    /// </summary>
    private void DisplayCurrentQuestion() {
        List<QuizQuestion> activeQuestionList = GetActiveQuestionList();

        if (quizUI != null && activeQuestionList != null
            && currentQuestionIndex >= 0
            && currentQuestionIndex < activeQuestionList.Count) {
            QuizQuestion q = activeQuestionList[currentQuestionIndex];
            quizUI.DisplayQuestion(q.question, q.options, OnAnswerSubmitted);
        }
    }

    /// <summary>
    /// Returns the correct question list based on the current language
    /// </summary>
    private List<QuizQuestion> GetActiveQuestionList() {
        // Fallback to Vietnamese if LocalizationManager is missing
        if (LocalizationManager.Instance == null) {
            return vietnameseQuestions;
        }

        string currentCode = LocalizationManager.Instance.CurrentLanguageCode;

        // Return English questions if current language is English
        if (currentCode.Equals(GameDefinitions.Languages.ENGLISH, System.StringComparison.OrdinalIgnoreCase)) {
            return englishQuestions;
        }

        return vietnameseQuestions; // Default fallback
    }

    /// <summary>
    /// Handles the answer submission from the quiz UI
    /// </summary>
    /// <param name="selectedIndex">Index of the selected answer</param>
    private void OnAnswerSubmitted(int selectedIndex) {
        List<QuizQuestion> activeQuestionList = GetActiveQuestionList();
        if (currentQuestionIndex < 0 || currentQuestionIndex >= activeQuestionList.Count) return;

        QuizQuestion currentQuestion = activeQuestionList[currentQuestionIndex];
        bool isCorrect = (selectedIndex == currentQuestion.correctIndex);

        // Show visual feedback on the UI
        if (quizUI != null) {
            quizUI.ShowAnswerResult(selectedIndex, currentQuestion.correctIndex);
        }

        // Stop any existing coroutine and start a new one
        if (quizRoutine != null) StopCoroutine(quizRoutine);
        quizRoutine = StartCoroutine(HandleQuizResultRoutine(isCorrect));
    }

    /// <summary>
    /// Handles the result after a 2-second delay: either success or failure
    /// </summary>
    private IEnumerator HandleQuizResultRoutine(bool isCorrect) {
        // Wait for feedback to play (unscaled time since Time.timeScale = 0)
        yield return new WaitForSecondsRealtime(2.0f);

        if (isCorrect) {
            // Success path
            IsCountingDown = true;
            isQuizActive = false;
            SetHUDInteraction(false);

            if (GameManager.Instance != null) {
                GameManager.Instance.HasCompletedQuiz = true;
            }

            if (quizUI != null) {
                quizUI.HidePanel();
            }

            DestroyNearbyObstacles(); // Clear obstacles around the bird

            // Play countdown before resuming
            if (quizUI != null) {
                yield return StartCoroutine(quizUI.StartCountdownRoutine(3f));
            }

            IsCountingDown = false;
            SetHUDInteraction(true);
            currentQuestionIndex = -1;

            Time.timeScale = 1f; // Resume the game
        } else {
            // Failure path
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

    /// <summary>
    /// Enables or disables interaction with the main HUD
    /// </summary>
    private void SetHUDInteraction(bool interactable) {
        if (mainHUDCanvasGroup != null) {
            mainHUDCanvasGroup.blocksRaycasts = interactable;
            mainHUDCanvasGroup.interactable = interactable;
        }
    }

    /// <summary>
    /// Destroys obstacles within the destroy radius around the bird
    /// </summary>
    private void DestroyNearbyObstacles() {
        if (birdTransform == null) return;

        // Find all obstacles within the destroy radius
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(
            birdTransform.position, destroyRadius, obstacleLayer);

        foreach (var hit in hitColliders) {
            if (hit.CompareTag("Obstacles")) {
                // Spawn explosion VFX if available
                if (explosionFXPrefab != null) {
                    Instantiate(explosionFXPrefab, hit.transform.position, Quaternion.identity);
                }

                Destroy(hit.gameObject);
            }
        }
    }

    /// <summary>
    /// Draws the destroy radius in the Scene view when selected
    /// </summary>
    private void OnDrawGizmosSelected() {
        if (birdTransform != null) {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(birdTransform.position, destroyRadius);
        }
    }
    #endregion

    #region Nested Types / Data Types
    /// <summary>
    /// Represents a single quiz question with options and correct answer index
    /// </summary>
    [System.Serializable]
    public struct QuizQuestion {
        public string question;      // The question text
        public string[] options;     // Multiple-choice options
        public int correctIndex;     // Index of the correct answer
    }
    #endregion
}