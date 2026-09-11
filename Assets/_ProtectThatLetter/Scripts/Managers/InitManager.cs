using System.Collections;
using UnityEngine;

namespace ProtectThatLetter.Managers {
    /// <summary>
    /// Controls the Bootstrap/Splash screen execution flow and handles smooth transition to the Login scene.
    /// </summary>
    [DisallowMultipleComponent]
    public class InitManager : MonoBehaviour {
        #region Serialized Fields
        [Header("Splash Timing Settings")]
        [SerializeField, Range(0.1f, 3f)] private float fadeInDuration = 1.0f;     // Fade in Splash duration
        [SerializeField, Range(0.5f, 5f)] private float minimumDisplayTime = 1.5f;   // Time splash is fully visible
        [SerializeField, Range(0.1f, 3f)] private float fadeOutDuration = 1.0f;     // Fade out to next scene duration
        #endregion

        #region Private Fields
        private Coroutine initRoutine; // Reference to the initialization coroutine
        #endregion

        #region Lifecycle
        /// <summary>
        /// Starts the initialization routine when the scene loads
        /// </summary>
        private void Start() {
            initRoutine = StartCoroutine(InitAndSplashRoutine());
        }

        /// <summary>
        /// Stops the initialization coroutine if the object is disabled
        /// </summary>
        private void OnDisable() {
            if (initRoutine != null) {
                StopCoroutine(initRoutine);
                initRoutine = null; // Clean reference to avoid dangling coroutine
            }
        }
        #endregion

        #region Core Flow
        /// <summary>
        /// Main initialization coroutine that handles splash sequence and scene transition
        /// </summary>
        private IEnumerator InitAndSplashRoutine() {
            // Step 1: Fade in the splash screen
            if (UIFadeManager.Instance != null) {
                yield return UIFadeManager.Instance.FadeInRoutine(fadeInDuration);
            } else {
                Debug.LogWarning("[InitManager] UIFadeManager.Instance is null! Skipping FadeIn sequence.");
            }

            // Step 2: Wait for the minimum display time (unscaled, ignores Time.timeScale)
            yield return new WaitForSecondsRealtime(minimumDisplayTime);

            // Step 3: Transition to the next scene
            if (SceneController.Instance != null) {
                SceneController.Instance.LoadNextScene(fadeOutDuration);
            } else {
                Debug.LogError("[InitManager] SceneManager.Instance is null! Cannot load next scene.");
            }

            initRoutine = null; // Clean up after completion
        }
        #endregion
    }
}