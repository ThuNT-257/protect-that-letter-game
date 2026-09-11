using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProtectThatLetter.Managers {
    /// <summary>
    /// Defines the story modes available in StoryScene.
    /// </summary>
    public enum StoryMode {
        Intro,  // Introduction story
        Outro   // Ending/Outro story
    }

    /// <summary>
    /// Manages the scene pipeline flow and global scene transition effects.
    /// </summary>
    [DisallowMultipleComponent]
    public class SceneController : MonoBehaviour {
        #region Constants
        // Scene names for easy reference and to avoid typos
        public const string INIT_SCENE = "InitScene";
        public const string LOGIN_SCENE = "LoginScene";
        public const string STORY_SCENE = "StoryScene";
        public const string PLAY_SCENE = "PlayScene";
        public const string LETTER_SCENE = "LetterScene";

        // Index of the Intro story step in the pipeline
        private const int INTRO_STORY_STEP_INDEX = 2;
        #endregion

        #region Singleton
        // Singleton instance with public getter and private setter
        public static SceneController Instance { get; private set; }
        #endregion

        #region Properties
        /// <summary>
        /// Current Story mode (Intro or Outro).
        /// </summary>
        public StoryMode CurrentStoryMode { get; private set; } = StoryMode.Intro;
        #endregion

        #region Private Fields
        private int currentStepIndex = 0;       // Current position in the pipeline
        private bool isTransitioning = false;   // Prevents overlapping transitions

        // Linear scene pipeline (forward-only transitions)
        private readonly List<string> pipeline = new List<string> {
            INIT_SCENE,   // Step 0
            LOGIN_SCENE,  // Step 1
            STORY_SCENE,  // Step 2 (Intro)
            PLAY_SCENE,   // Step 3
            STORY_SCENE,  // Step 4 (Outro)
            LETTER_SCENE  // Step 5
        };
        #endregion

        #region Lifecycle
        /// <summary>
        /// Initializes the singleton and determines the current step based on the active scene
        /// </summary>
        private void Awake() {
            // Destroy duplicate instances
            if (Instance != null && Instance != this) {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist across scenes

            InitCurrentStepIndex();
        }

        /// <summary>
        /// Clears the singleton reference when destroyed
        /// </summary>
        private void OnDestroy() {
            if (Instance == this) {
                Instance = null;
            }
        }

        /// <summary>
        /// Resets the transitioning flag when disabled
        /// </summary>
        private void OnDisable() {
            isTransitioning = false;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Gets the JSON file name for the current Story mode.
        /// </summary>
        /// <returns>File name of the story JSON</returns>
        public string GetCurrentStoryFileName() {
            return CurrentStoryMode switch {
                StoryMode.Intro => "IntroStory",
                StoryMode.Outro => "OutroStory",
                _ => "IntroStory" // Fallback to IntroStory
            };
        }

        /// <summary>
        /// Advances to the next scene in the pipeline order.
        /// </summary>
        /// <param name="fadeDuration">Fade In/Out duration (-1f uses default).</param>
        public void LoadNextScene(float fadeDuration = -1f) {
            // Prevent overlapping transitions
            if (isTransitioning) return;

            // Check if already at the final scene
            if (currentStepIndex >= pipeline.Count - 1) {
                Debug.LogWarning($"[SceneController] Already at final scene ('{pipeline[currentStepIndex]}').");
                return;
            }

            int nextStepIndex = currentStepIndex + 1;
            string targetScene = pipeline[nextStepIndex];

            // Update StoryMode based on the target index
            if (targetScene == STORY_SCENE) {
                CurrentStoryMode = (nextStepIndex == INTRO_STORY_STEP_INDEX) ? StoryMode.Intro : StoryMode.Outro;
            }

            StartCoroutine(TransitionToSceneRoutine(targetScene, nextStepIndex, fadeDuration));
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Determines the current pipeline step from the active scene name
        /// </summary>
        private void InitCurrentStepIndex() {
            string startScene = SceneManager.GetActiveScene().name;
            int idx = pipeline.IndexOf(startScene);

            // Fallback to index 0 if scene is not in the pipeline
            if (idx < 0) {
                Debug.LogWarning($"[SceneController] Scene '{startScene}' not in pipeline. Defaulting to index 0.");
                idx = 0;
            }

            currentStepIndex = idx;
        }

        /// <summary>
        /// Handles the full scene transition: fade out, async load, fade in
        /// </summary>
        private IEnumerator TransitionToSceneRoutine(string sceneName, int targetStepIndex, float fadeDuration) {
            isTransitioning = true;

            try {
                // Step 1: Fade out the screen
                if (UIFadeManager.Instance != null) {
                    yield return UIFadeManager.Instance.FadeOutRoutine(fadeDuration);
                }

                // Step 2: Load the scene asynchronously
                AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
                if (asyncLoad == null) {
                    Debug.LogError($"[SceneController] Failed to load scene: {sceneName}");
                    yield break;
                }

                asyncLoad.allowSceneActivation = false;

                // Wait until the scene is 90% loaded
                while (asyncLoad.progress < 0.9f) {
                    yield return null;
                }

                // Activate the new scene and update the step index
                currentStepIndex = targetStepIndex;
                asyncLoad.allowSceneActivation = true;
                yield return null;

                // Step 3: Fade in the screen
                if (UIFadeManager.Instance != null) {
                    yield return UIFadeManager.Instance.FadeInRoutine(fadeDuration);
                }
            } finally {
                // Ensure the flag is always unlocked even if an error occurs
                isTransitioning = false;
            }
        }
        #endregion
    }
}