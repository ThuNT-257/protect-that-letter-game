using ProtectThatLetter.Controllers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static ObstacleSpawner;

public class GameManager : MonoBehaviour {
    #region Instance
    private static GameManager instance;
    public static GameManager Instance {
        get {
            if (instance == null) {
                instance = FindAnyObjectByType<GameManager>();
            }
            return instance;
        }
    }
    #endregion

    #region Serialized Fields
    [Header("UI References")]
    [SerializeField] private Slider timebarSlider;
    [SerializeField] private GameWinUI gameWinUI;
    [SerializeField] private GuideTextController guideTextController;

    [Header("Game Settings")]
    [SerializeField] private ObstacleSpawner obstacleSpawner;
    [SerializeField] private CloudSpawner cloudSpawner;
    [SerializeField] private LevelBackgroundManager backgroundManager;
    [SerializeField] private ShieldController shieldController;
    [SerializeField] private BirdController birdController;
    [SerializeField] private float transitionDelay = 2f;
    [SerializeField] private List<LevelConfig> levels = new List<LevelConfig>();
    [SerializeField] private bool autoStartOnLoad = false;
    #endregion

    #region Private Fields
    private float totalGameDuration = 0f;
    private float currentGameTime = 0f;
    private Coroutine gameLoopCoroutine;
    #endregion

    #region Properties
    public bool IsPaused { get; private set; } = false;
    public bool HasGameStarted { get; private set; } = false;
    public bool HasCompletedQuiz { get; set; } = false; 
    #endregion

    #region Lifecycle
    private void Awake() {
        if (instance != null && instance != this) {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    private void Start() {
        InitGame();

        if (autoStartOnLoad) {
            StartGame();
        }
    }

    private void Update() {
        if (!HasGameStarted || IsPaused || totalGameDuration <= 0f) return;

        if (currentGameTime < totalGameDuration) {
            currentGameTime += Time.deltaTime;
            if (timebarSlider != null) {
                timebarSlider.value = currentGameTime / totalGameDuration;
            }
        }
    }

    private void OnDestroy() {
        if (instance == this) {
            instance = null;
        }
    }
    #endregion

    #region Public Methods
    public void StartGame() {
        if (HasGameStarted) return;

        HasGameStarted = true;

        if (AudioManager.Instance != null) {
            AudioManager.Instance.PlayLoopSFX("wind_sfx");
        }

        if (cloudSpawner != null) {
            cloudSpawner.StartSpawning();
        }

        if (guideTextController != null) {
            guideTextController.HideGuideWithFade();
        }

        if (gameLoopCoroutine != null) StopCoroutine(gameLoopCoroutine);
        gameLoopCoroutine = StartCoroutine(GameLoopRoutine());
    }

    public void PauseGame() {
        if (QuizManager.Instance != null && QuizManager.Instance.IsCountingDown) return;

        IsPaused = true;
        Time.timeScale = 0f;
    }

    public void ResumeGame() {
        IsPaused = false;
        Time.timeScale = 1f;
    }

    public void RestartGame() {
        Time.timeScale = 1f;
        IsPaused = false;

        if (AudioManager.Instance != null) {
            AudioManager.Instance.StopSFX();
        }

        if (gameLoopCoroutine != null) {
            StopCoroutine(gameLoopCoroutine);
            gameLoopCoroutine = null;
        }

        if (obstacleSpawner != null) {
            obstacleSpawner.ResetSpawner();
        }

        if (cloudSpawner != null) {
            cloudSpawner.StopAndResetSpawner();
        }

        InitGame();
    }

    public void OnAllObstaclesCleared() {
        if (gameWinUI != null) {
            gameWinUI.ShowWinAndTransition();
        }
    }
    #endregion

    #region Private Methods
    private void InitGame() {
        Time.timeScale = 1f;
        IsPaused = false;
        HasGameStarted = false;
        HasCompletedQuiz = false; 
        currentGameTime = 0f;
        totalGameDuration = 0f;

        if (guideTextController != null) {
            guideTextController.ShowGuide();
        }

        if (backgroundManager == null) {
            backgroundManager = FindAnyObjectByType<LevelBackgroundManager>();
        }

        if (backgroundManager != null) {
            backgroundManager.ChangeLevel(0);
        }

        if (shieldController != null) {
            shieldController.ResetShield();
        } else {
            shieldController = FindAnyObjectByType<ShieldController>();
            if (shieldController != null) {
                shieldController.ResetShield();
            }
        }

        if (birdController != null) {
            birdController.ResetCollisionCount();
        } else {
            birdController = FindAnyObjectByType<BirdController>();
            if (birdController != null) {
                birdController.ResetCollisionCount();
            }
        }

        if (QuizManager.Instance != null) {
            QuizManager.Instance.ResetQuizState();
        }

        for (int i = 0; i < levels.Count; i++) {
            if (levels[i] != null) {
                totalGameDuration += levels[i].duration;
                if (i < levels.Count - 1) {
                    totalGameDuration += transitionDelay;
                }
            }
        }

        if (timebarSlider != null) {
            timebarSlider.minValue = 0f;
            timebarSlider.maxValue = 1f;
            timebarSlider.value = 0f;
        }
    }

    private IEnumerator GameLoopRoutine() {
        if (!ValidateReferences()) yield break;

        yield return new WaitForEndOfFrame();

        for (int i = 0; i < levels.Count; i++) {
            LevelConfig currentLevel = levels[i];

            if (backgroundManager != null) {
                backgroundManager.ChangeLevel(i);
            }

            obstacleSpawner.StartLevel(currentLevel);

            yield return new WaitForSeconds(currentLevel.duration);

            obstacleSpawner.StopSpawning();

            if (i < levels.Count - 1) {
                yield return new WaitForSeconds(transitionDelay);
            }
        }

        if (timebarSlider != null) timebarSlider.value = 1f;

        yield return StartCoroutine(WaitAndTriggerWinRoutine());
    }

    private IEnumerator WaitAndTriggerWinRoutine() {
        yield return new WaitUntil(() => GameObject.FindGameObjectsWithTag("Obstacles").Length == 0);

        if (gameWinUI != null) {
            gameWinUI.ShowWinAndTransition();
        }
    }

    private bool ValidateReferences() {
        if (obstacleSpawner == null) {
            obstacleSpawner = FindAnyObjectByType<ObstacleSpawner>();
            if (obstacleSpawner == null) return false;
        }

        if (levels == null || levels.Count == 0) return false;

        return true;
    }
    #endregion
}