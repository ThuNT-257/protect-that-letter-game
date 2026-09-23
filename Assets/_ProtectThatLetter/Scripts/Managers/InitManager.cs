using System.Collections;
using UnityEngine;

namespace ProtectThatLetter.Managers {
    /// <summary>
    /// Controls the Bootstrap execution flow and delegates scene transitions to SceneController.
    /// </summary>
    [DisallowMultipleComponent]
    public class InitManager : MonoBehaviour {
        #region Serialized Fields
        [Header("Splash Timing Settings")]
        [SerializeField, Range(0.5f, 5f)] private float splashHoldDuration = 1.5f;  // Duration to hold splash before transitioning
        [SerializeField, Range(0.1f, 3f)] private float transitionDuration = 1.0f;   // Fade duration for the scene transition
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
        /// Holds the splash screen for a fixed duration, then delegates the scene transition
        /// </summary>
        private IEnumerator InitAndSplashRoutine() {
            // Wait for the splash hold duration (unscaled time, ignores Time.timeScale)
            yield return new WaitForSecondsRealtime(splashHoldDuration);

            // Delegate scene transition to SceneController
            if (SceneController.Instance != null) {
                SceneController.Instance.LoadNextScene(transitionDuration);
            } else {
                Debug.LogError($"[{nameof(InitManager)}] SceneController.Instance is NULL! Cannot transition.");
            }

            initRoutine = null; // Clean up after completion
        }
        #endregion
    }
}