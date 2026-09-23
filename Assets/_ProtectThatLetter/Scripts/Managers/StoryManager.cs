using ProtectThatLetter.Definitions;
using UnityEngine;

namespace ProtectThatLetter.Managers {

    /// <summary>
    /// Handles story-specific concerns such as story mode (Intro/Outro)
    /// and the corresponding JSON file name.
    /// Subscribes to SceneController events to stay in sync.
    /// </summary>
    public class StoryManager : MonoBehaviour {
        #region Constants
        // Index of the Intro story step in the SceneController pipeline
        private const int INTRO_STORY_STEP_INDEX = 2;
        #endregion

        #region Instance
        // Singleton instance with public getter and private setter
        public static StoryManager Instance { get; private set; }
        #endregion

        #region Public Properties
        // Current story mode (Intro or Outro) - read-only externally
        public StoryMode CurrentStoryMode { get; private set; } = StoryMode.Intro;
        #endregion

        #region Lifecycle
        /// <summary>
        /// Ensures singleton integrity and makes the object persistent across scenes
        /// </summary>
        private void Awake() {
            // Destroy duplicate instances
            if (Instance != null && Instance != this) {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist across scenes
        }

        /// <summary>
        /// Registers event listeners after all Awake calls have completed
        /// </summary>
        private void Start() {
            RegisterEvents();
        }

        /// <summary>
        /// Unregisters events and clears the singleton reference when destroyed
        /// </summary>
        private void OnDestroy() {
            UnregisterEvents();
            if (Instance == this) {
                Instance = null;
            }
        }
        #endregion

        #region Event Handlers
        /// <summary>
        /// Subscribes to SceneController scene change events
        /// </summary>
        private void RegisterEvents() {
            if (SceneController.Instance != null) {
                // Unsubscribe first to prevent duplicate subscriptions
                SceneController.Instance.OnSceneChanged -= HandleSceneChanged;
                SceneController.Instance.OnSceneChanged += HandleSceneChanged;
            } else {
                Debug.LogWarning($"[{nameof(StoryManager)}] {nameof(SceneController)}.Instance is null. Failed to subscribe to scene change events.");
            }
        }

        /// <summary>
        /// Unsubscribes from SceneController scene change events
        /// </summary>
        private void UnregisterEvents() {
            if (SceneController.Instance != null) {
                SceneController.Instance.OnSceneChanged -= HandleSceneChanged;
            }
        }

        /// <summary>
        /// Updates the current story mode when the scene changes to StoryScene
        /// </summary>
        /// <param name="sceneName">Name of the newly loaded scene</param>
        /// <param name="stepIndex">Index of the step in the pipeline</param>
        private void HandleSceneChanged(string sceneName, int stepIndex) {
            if (sceneName == SceneName.STORY_SCENE) {
                // Intro if index matches INTRO_STORY_STEP_INDEX, otherwise Outro
                CurrentStoryMode = (stepIndex == INTRO_STORY_STEP_INDEX) ? StoryMode.Intro : StoryMode.Outro;
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Gets the JSON file name for the current story mode
        /// </summary>
        /// <returns>File name of the story JSON</returns>
        public string GetCurrentStoryFileName() {
            return CurrentStoryMode switch {
                StoryMode.Intro => "IntroStory",
                StoryMode.Outro => "OutroStory",
                _ => "IntroStory" // Fallback
            };
        }
        #endregion
    }
}