using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProtectThatLetter.Managers {
    /// <summary>
    /// Story modes available for StoryScene.
    /// </summary>
    public enum StoryMode {
        Intro,  // Introduction story
        Outro   // Ending story
    }

    /// <summary>
    /// Manages strict linear scene navigation throughout the application:
    /// Init -> Login -> Story (Intro) -> Play -> Story (Outro) -> Letter.
    /// </summary>
    [DisallowMultipleComponent]
    public class SceneManager : MonoBehaviour {
        #region Constants
        // Scene names for easy reference and to avoid typos
        public const string INIT_SCENE = "InitScene";
        public const string LOGIN_SCENE = "LoginScene";
        public const string STORY_SCENE = "StoryScene";
        public const string PLAY_SCENE = "PlayScene";
        public const string LETTER_SCENE = "LetterScene";
        #endregion

        #region Singleton
        // Singleton instance with public getter and private setter
        public static SceneManager Instance { get; private set; }
        #endregion

        #region Properties
        // Current story mode (Intro or Outro) - Read-only from outside
        public StoryMode CurrentStoryMode { get; private set; } = StoryMode.Intro;
        #endregion

        #region Private Fields
        private int currentStepIndex = 0; // Current position in the pipeline

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

            // Find current step based on the active scene name
            string startScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            currentStepIndex = Mathf.Max(0, pipeline.IndexOf(startScene));
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Returns the JSON file name for the current story mode.
        /// Replaces the old StoryManager functionality.
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
        /// Advances to the next scene in the linear pipeline.
        /// </summary>
        /// <param name="fadeDuration">Duration of the fade transition (-1 uses default)</param>
        public void LoadNextScene(float fadeDuration = -1f) {
            // Check if already at the final scene
            if (currentStepIndex >= pipeline.Count - 1) {
                Debug.LogWarning($"[SceneController] Already at the final scene ('{pipeline[currentStepIndex]}').");
                return;
            }

            int nextStepIndex = currentStepIndex + 1;
            string targetScene = pipeline[nextStepIndex];

            // Update StoryMode based on the upcoming step
            if (targetScene == STORY_SCENE) {
                CurrentStoryMode = (nextStepIndex == 2) ? StoryMode.Intro : StoryMode.Outro;
            }

            currentStepIndex = nextStepIndex;

            // Load scene with fade effect if available, otherwise load directly
            if (UIFadeManager.Instance != null) {
                UIFadeManager.Instance.FadeToScene(targetScene, fadeDuration);
            } else {
                StartCoroutine(LoadSceneAsyncCoroutine(targetScene));
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Loads a scene asynchronously
        /// </summary>
        /// <param name="sceneName">Name of the scene to load</param>
        private IEnumerator LoadSceneAsyncCoroutine(string sceneName) {
            AsyncOperation asyncLoad = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName);

            while (!asyncLoad.isDone) {
                yield return null;
            }
        }
        #endregion
    }
}