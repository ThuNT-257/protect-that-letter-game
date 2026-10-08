using ProtectThatLetter.Managers;
using ProtectThatLetter.UI;
using UnityEngine;

namespace ProtectThatLetter.Controllers
{
    /// <summary>
    /// Listens to DialogueManager events and handles scene transitions
    /// for success (story completed) and failure (load failed).
    /// </summary>
    public class StorySceneController : MonoBehaviour
    {
        #region Unity Lifecycle
        /// <summary>
        /// Subscribes to DialogueManager events when enabled
        /// </summary>
        private void OnEnable()
        {
            DialogueManager.OnStoryCompleted += HandleStoryCompleted;
            DialogueManager.OnStoryLoadFailed += HandleStoryFailed;
        }

        /// <summary>
        /// Unsubscribes from DialogueManager events to prevent memory leaks
        /// </summary>
        private void OnDisable()
        {
            DialogueManager.OnStoryCompleted -= HandleStoryCompleted;
            DialogueManager.OnStoryLoadFailed -= HandleStoryFailed;
        }
        #endregion

        #region Event Handlers
        /// <summary>
        /// Handles successful story completion: advances the scene pipeline
        /// </summary>
        private void HandleStoryCompleted()
        {
            if (SceneController.Instance != null)
            {
                SceneController.Instance.LoadNextScene();
            }
            else
            {
                Debug.LogWarning($"[{nameof(StorySceneController)}] SceneController.Instance is NULL. Cannot load next scene.");
            }
        }

        /// <summary>
        /// Handles story load failure: falls back to the Login scene
        /// </summary>
        private void HandleStoryFailed(string fileName, string errorMsg)
        {
            Debug.LogError($"[{nameof(StorySceneController)}] Cannot proceed. Story '{fileName}' failed: {errorMsg}");

            if (SceneController.Instance != null)
            {
                SceneController.Instance.LoadSceneByName(Definitions.SceneName.LOGIN_SCENE);
            }
            else
            {
                Debug.LogWarning($"[{nameof(StorySceneController)}] SceneController.Instance is NULL. Cannot fall back to Login.");
            }
        }
        #endregion
    }
}