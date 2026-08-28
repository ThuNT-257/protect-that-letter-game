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
    [SerializeField] private Slider timebarSlider;
    [SerializeField] private ObstacleSpawner spawner;
    [SerializeField] private float transitionDelay = 5f;
    [SerializeField] private List<LevelConfig> levels = new List<LevelConfig>();
    #endregion

    #region Private Fields
    private float totalGameDuration = 0f;
    private float currentGameTime = 0f;
    private Coroutine gameLoopCoroutine;
    #endregion

    #region Properties
    public bool IsPaused { get; private set; } = false;
    #endregion

    #region Lifecycle
    private void Awake() {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    private void Start() {
        InitGame();
    }

    private void Update() { 
        if (IsPaused || totalGameDuration <= 0f) return;

        if (currentGameTime < totalGameDuration)
        {
            currentGameTime += Time.deltaTime;
            if (timebarSlider != null)
            {
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
    public void PauseGame() {
        IsPaused = true;
        Time.timeScale = 0f;
        Debug.Log("[GameManager] Game Paused");
    }

    public void ResumeGame() {
        IsPaused = false;
        Time.timeScale = 1f;
        Debug.Log("[GameManager] Game Resumed");
    }

    public void RestartGame() {
        Time.timeScale = 1f;
        IsPaused = false;

        if (gameLoopCoroutine != null) {
            StopCoroutine(gameLoopCoroutine);
            gameLoopCoroutine = null;
        }

        if (spawner != null) {
            spawner.ResetSpawner();
        }

        InitGame();
        Debug.Log("[GameManager] Game Restarted");
    }
    #endregion

    #region Private Methods
    private void InitGame() {
        Time.timeScale = 1f;
        IsPaused = false;
        currentGameTime = 0f;
        totalGameDuration = 0f;

        for (int i = 0; i < levels.Count; i++) {
            if (levels[i] != null)
            {
                totalGameDuration += levels[i].duration;
                if (i < levels.Count - 1)
                {
                    totalGameDuration += transitionDelay;
                }
            }
        }

        if (timebarSlider != null) {
            timebarSlider.minValue = 0f;
            timebarSlider.maxValue = 1f;
            timebarSlider.value = 0f;
        }

        if (gameLoopCoroutine != null) StopCoroutine(gameLoopCoroutine);
        gameLoopCoroutine = StartCoroutine(GameLoopRoutine());
    }

    private IEnumerator GameLoopRoutine() {
        if (!ValidateReferences()) {
            Debug.LogError("[GameManager] Game Loop aborted due to missing references!");
            yield break;
        }

        yield return new WaitForEndOfFrame();

        for (int i = 0; i < levels.Count; i++) {
            LevelConfig currentLevel = levels[i];
            Debug.Log($"[GameManager] - Start {currentLevel.levelName}");

            spawner.StartLevel(currentLevel);

            yield return new WaitForSeconds(currentLevel.duration);

            spawner.StopSpawning();
            Debug.Log($"[GameManager] - Finish {currentLevel.levelName}! Waiting for transition...");

            if (i < levels.Count - 1) {
                yield return new WaitForSeconds(transitionDelay);
            }
        }

        if (timebarSlider != null) timebarSlider.value = 1f;

        Debug.Log("[GameManager] - WIN!");
    }

    private bool ValidateReferences() {
        if (spawner == null) {
            spawner = FindAnyObjectByType<ObstacleSpawner>();

            if (spawner == null) {
                Debug.LogError("[GameManager] Missing ObstacleSpawner reference!");
                return false;
            }
        }

        if (levels == null || levels.Count == 0) {
            Debug.LogError("[GameManager] Level list is empty!");
            return false;
        }

        return true;
    }
    #endregion
}