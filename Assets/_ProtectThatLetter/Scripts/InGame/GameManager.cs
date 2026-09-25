using ProtectThatLetter.Controllers;
using ProtectThatLetter.Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static ObstacleSpawner;

public class GameManager : MonoBehaviour {
    #region State Enum
    public enum GameState {
        Init,
        Playing,
        Paused,
        GameOver,
        GameWin
    }
    #endregion

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
    private Coroutine winWaitCoroutine;
    private WaitForEndOfFrame waitForEndOfFrame = new WaitForEndOfFrame();
    private bool areAllLevelsFinished = false;
    #endregion

    #region Properties
    public GameState CurrentState { get; private set; } = GameState.Init;
    public bool IsPaused => CurrentState == GameState.Paused;
    public bool HasGameStarted => CurrentState == GameState.Playing || CurrentState == GameState.Paused;
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
        if (CurrentState != GameState.Playing || totalGameDuration <= 0f) return;

        if (timebarSlider != null && !areAllLevelsFinished) {
            float targetValue = Mathf.Clamp01(currentGameTime / totalGameDuration);
            timebarSlider.value = targetValue;
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
        CurrentState = GameState.Playing;
        areAllLevelsFinished = false;

        if (AudioManager.Instance != null) {
            AudioManager.Instance.PlayLoopSFX("wind_sfx");
        }

        if (cloudSpawner != null) {
            cloudSpawner.StartSpawning();
        }

        if (guideTextController != null) {
            guideTextController.HideGuideWithFade();
        }

        StopAllGameCoroutines();
        gameLoopCoroutine = StartCoroutine(GameLoopRoutine());
    }

    public void PauseGame() {
        if (QuizManager.Instance != null && QuizManager.Instance.IsCountingDown) return;
        if (CurrentState != GameState.Playing) return;

        CurrentState = GameState.Paused;
        Time.timeScale = 0f;
    }
    public void OnLevelComplete() {
        obstacleSpawner.StopSpawning();

        obstacleSpawner.ClearAllActiveObstacles();

        birdController.PlayWinFlyAnimation();
    }

    public void ResumeGame() {
        if (CurrentState != GameState.Paused) return;

        CurrentState = GameState.Playing;
        Time.timeScale = 1f;
    }

    public void RestartGame() {
        Time.timeScale = 1f;

        StopAllGameCoroutines();

        if (AudioManager.Instance != null) {
            AudioManager.Instance.StopSFX();
        }

        if (obstacleSpawner != null) {
            obstacleSpawner.ResetSpawner();
        }

        if (cloudSpawner != null) {
            cloudSpawner.StopAndResetSpawner();
        }

        InitGame();
    }

    // HÀM XỬ LÝ KHI THUA
    public void GameOver() {
        CurrentState = GameState.GameOver;

        // KIỂM SOÁT TRIỆT ĐỂ: Dừng sạch mọi Coroutine đang chạy
        StopAllGameCoroutines();

        if (obstacleSpawner != null) {
            obstacleSpawner.StopSpawning();
        }

        if (gameWinUI != null) {
            gameWinUI.ResetWinUI();
        }
    }

    public void OnAllObstaclesCleared() {
        if (!areAllLevelsFinished || CurrentState != GameState.Playing) return;

        TriggerWin();
    }
    #endregion

    #region Private Methods
    private void InitGame() {
        Time.timeScale = 1f;
        CurrentState = GameState.Init;
        HasCompletedQuiz = false;
        areAllLevelsFinished = false;
        currentGameTime = 0f;
        totalGameDuration = 0f;

        StopAllGameCoroutines();

        if (gameWinUI != null) {
            gameWinUI.ResetWinUI();
        }

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

        yield return waitForEndOfFrame;

        for (int i = 0; i < levels.Count; i++) {
            LevelConfig currentLevel = levels[i];

            if (backgroundManager != null) {
                backgroundManager.ChangeLevel(i);
            }

            obstacleSpawner.StartLevel(currentLevel);

            float timer = 0f;
            while (timer < currentLevel.duration) {
                if (CurrentState == GameState.Playing) {
                    float dt = Time.deltaTime;
                    timer += dt;
                    currentGameTime += dt;
                } else if (CurrentState == GameState.GameOver || CurrentState == GameState.GameWin) {
                    yield break;
                }
                yield return null;
            }

            obstacleSpawner.StopSpawning();

            if (i < levels.Count - 1) {
                float delayTimer = 0f;
                while (delayTimer < transitionDelay) {
                    if (CurrentState == GameState.Playing) {
                        float dt = Time.deltaTime;
                        delayTimer += dt;
                        currentGameTime += dt;
                    } else if (CurrentState == GameState.GameOver || CurrentState == GameState.GameWin) {
                        yield break;
                    }
                    yield return null;
                }
            }
        }

        areAllLevelsFinished = true;
        currentGameTime = totalGameDuration;
        if (timebarSlider != null) timebarSlider.value = 1f;

        winWaitCoroutine = StartCoroutine(WaitUntilAllObstaclesCleared());
    }

    private IEnumerator WaitUntilAllObstaclesCleared() {
        if (obstacleSpawner != null) {
            obstacleSpawner.StopSpawning();
        }

        float safetyTimeout = 10.0f;
        float elapsed = 0f;

        while (elapsed < safetyTimeout) {
            if (CurrentState != GameState.Playing) yield break;

            elapsed += Time.deltaTime;

            if (IsScreenClear()) {
                break;
            }

            yield return null;
        }

        TriggerWin();
    }

    private void TriggerWin() {
        if (CurrentState != GameState.Playing) return;

        CurrentState = GameState.GameWin;

        if (obstacleSpawner != null) {
            obstacleSpawner.StopSpawning();
            obstacleSpawner.ClearAllActiveObstacles();
        }

        if (birdController != null) {
            birdController.PlayWinFlyAnimation();
        }

        if (gameWinUI != null) {
            gameWinUI.ShowWinAndTransition();
        }
    }

    private bool IsScreenClear() {
        if (obstacleSpawner == null) return true;
        if (obstacleSpawner.ActiveObstacleCount == 0) return true;

        var activeObstacles = obstacleSpawner.GetActiveObstacles();
        if (activeObstacles == null || activeObstacles.Count == 0) return true;

        Camera mainCam = Camera.main;
        if (mainCam == null) return false;

        foreach (GameObject obstacle in activeObstacles) {
            if (obstacle != null && obstacle.activeInHierarchy) {
                Vector3 viewportPos = mainCam.WorldToViewportPoint(obstacle.transform.position);
                if (viewportPos.y > -0.2f && viewportPos.y < 1.2f) {
                    return false; 
                }
            }
        }

        return true;
    }

    private void StopAllGameCoroutines() {
        if (gameLoopCoroutine != null) {
            StopCoroutine(gameLoopCoroutine);
            gameLoopCoroutine = null;
        }
        if (winWaitCoroutine != null) {
            StopCoroutine(winWaitCoroutine);
            winWaitCoroutine = null;
        }
        StopAllCoroutines();
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