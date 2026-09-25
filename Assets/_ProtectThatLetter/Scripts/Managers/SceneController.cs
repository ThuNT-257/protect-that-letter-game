using ProtectThatLetter.Definitions;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProtectThatLetter.Managers
{

    /// <summary>
    /// Manages the scene pipeline flow and global scene transition effects.
    /// Broadcasts scene change events for other managers to react to.
    /// </summary>
    [DisallowMultipleComponent]
    public class SceneController : MonoBehaviour
    {
        #region Constants
        private const float SCENE_LOAD_TIMEOUT = 15f; // Max time to wait for scene load/activation
        #endregion

        #region Instance
        // Singleton instance with public getter and private setter
        public static SceneController Instance { get; private set; }
        #endregion

        #region Private Fields
        private int currentStepIndex = 0;       // Current position in the pipeline
        private bool isTransitioning = false;   // Prevents overlapping transitions

        // Linear scene pipeline (forward-only transitions)
        private readonly List<string> pipeline = new List<string> 
        {
            SceneName.INIT_SCENE,    // 0
            SceneName.LOGIN_SCENE,   // 1
            SceneName.STORY_SCENE,   // 2 (Intro)
            SceneName.PLAY_SCENE,    // 3
            SceneName.STORY_SCENE,   // 4 (Outro)
            SceneName.LETTER_SCENE   // 5
        };
        #endregion

        #region Events
        /// <summary>
        /// Broadcasts after a successful scene transition.
        /// Parameters: sceneName, stepIndex
        /// </summary>
        public event Action<string, int> OnSceneChanged;
        #endregion

        #region Unity Lifecycle
        /// <summary>
        /// Initializes the singleton and determines the current step from the active scene
        /// </summary>
        private void Awake()
        {
            // Destroy duplicate instances
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist across scenes

            InitCurrentStepIndex();
        }

        /// <summary>
        /// Fades in the screen on startup to reveal the initial scene
        /// </summary>
        private void Start()
        {
            if (UIFadeManager.Instance != null)
            {
                StartCoroutine(UIFadeManager.Instance.FadeInRoutine());
            }
        }

        /// <summary>
        /// Resets the transitioning flag when disabled
        /// </summary>
        private void OnDisable()
        {
            isTransitioning = false;
        }

        /// <summary>
        /// Clears the singleton reference when destroyed
        /// </summary>
        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Advances to the next scene in the pipeline order
        /// </summary>
        /// <param name="fadeDuration">Fade duration (-1 uses default)</param>
        public void LoadNextScene(float fadeDuration = -1f)
        {
            // Prevent overlapping transitions
            if (isTransitioning) return;

            // Check if already at the final scene
            if (currentStepIndex >= pipeline.Count - 1)
            {
                Debug.LogWarning($"[SceneController] Already at final scene ('{pipeline[currentStepIndex]}').");
                return;
            }

            int nextStepIndex = currentStepIndex + 1;
            string targetScene = pipeline[nextStepIndex];

            StartCoroutine(TransitionToSceneRoutine(targetScene, nextStepIndex, fadeDuration));
        }

        /// <summary>
        /// Loads a specific scene by name (must be in the pipeline)
        /// </summary>
        /// <param name="sceneName">Name of the scene to load</param>
        /// <param name="fadeDuration">Fade duration (-1 uses default)</param>
        public void LoadSceneByName(string sceneName, float fadeDuration = -1f)
        {
            if (isTransitioning) return;

            int targetIndex = pipeline.IndexOf(sceneName);
            if (targetIndex < 0)
            {
                Debug.LogWarning($"[SceneController] Target scene '{sceneName}' is not in the defined pipeline.");
                targetIndex = currentStepIndex; // Keep current index if not found
            }

            StartCoroutine(TransitionToSceneRoutine(sceneName, targetIndex, fadeDuration));
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Determines the current pipeline step from the active scene name
        /// </summary>
        private void InitCurrentStepIndex()
        {
            string startScene = SceneManager.GetActiveScene().name;
            int idx = pipeline.IndexOf(startScene);

            // Fallback to index 0 if scene not in pipeline
            if (idx < 0)
            {
                Debug.LogWarning($"[SceneController] Scene '{startScene}' not in pipeline. Defaulting to index 0.");
                idx = 0;
            }

            currentStepIndex = idx;
        }
        #endregion

        #region Coroutines
        /// <summary>
        /// Handles the full scene transition with fade, async load, timeout, and rollback
        /// </summary>
        private IEnumerator TransitionToSceneRoutine(string sceneName, int targetStepIndex, float fadeDuration)
        {
            isTransitioning = true;
            bool isSceneReady = false;
            UnityEngine.Events.UnityAction<Scene, LoadSceneMode> onSceneLoaded = null;

            try
            {
                // Step 1: Fade out the screen
                if (UIFadeManager.Instance != null)
                {
                    yield return UIFadeManager.Instance.FadeOutRoutine(fadeDuration);
                }

                // Step 2: Register scene loaded callback
                onSceneLoaded = (loadedScene, mode) => {
                    if (loadedScene.name == sceneName)
                    {
                        isSceneReady = true;
                    }
                };
                SceneManager.sceneLoaded += onSceneLoaded;

                // Step 3: Start async scene load
                AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
                if (asyncLoad == null)
                {
                    Debug.LogError($"[SceneController] Failed to initiate async load for scene: {sceneName}");
                    yield return RollbackFadeRoutine(fadeDuration);
                    yield break;
                }
                asyncLoad.allowSceneActivation = false;

                // Step 4: Wait for progress with timeout
                float startTime = Time.realtimeSinceStartup;
                while (asyncLoad.progress < 0.9f)
                {
                    if (Time.realtimeSinceStartup - startTime >= SCENE_LOAD_TIMEOUT)
                    {
                        Debug.LogError($"[SceneController] Loading scene '{sceneName}' timed out. Aborting transition.");
                        yield return RollbackFadeRoutine(fadeDuration);
                        yield break;
                    }
                    yield return null;
                }

                // Step 5: Activate the scene
                asyncLoad.allowSceneActivation = true;

                // Step 6: Wait for scene activation with timeout
                startTime = Time.realtimeSinceStartup;
                while (!isSceneReady)
                {
                    if (Time.realtimeSinceStartup - startTime >= SCENE_LOAD_TIMEOUT)
                    {
                        Debug.LogError($"[SceneController] Scene activation for '{sceneName}' timed out. Aborting transition.");
                        yield return RollbackFadeRoutine(fadeDuration);
                        yield break;
                    }
                    yield return null;
                }

                // Step 7: Update index & broadcast scene change event
                currentStepIndex = targetStepIndex;
                OnSceneChanged?.Invoke(sceneName, targetStepIndex);

                yield return new WaitForEndOfFrame();

                // Step 8: Fade in the screen
                if (UIFadeManager.Instance != null)
                {
                    yield return UIFadeManager.Instance.FadeInRoutine(fadeDuration);
                }
            }
            finally
            {
                // Guaranteed cleanup: unsubscribe from scene loaded event
                if (onSceneLoaded != null)
                {
                    SceneManager.sceneLoaded -= onSceneLoaded;
                }
                isTransitioning = false;
            }
        }

        /// <summary>
        /// Fades the screen back in when a transition fails
        /// </summary>
        private IEnumerator RollbackFadeRoutine(float fadeDuration)
        {
            if (UIFadeManager.Instance != null)
            {
                yield return UIFadeManager.Instance.FadeInRoutine(fadeDuration);
            }
        }
        #endregion
    }
}