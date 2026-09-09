using System;
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
        [SerializeField, Range(0.1f, 3f)] private float fadeInDuration = 1.0f;      
        [SerializeField, Range(0.5f, 5f)] private float minimumDisplayTime = 1.5f;  
        [SerializeField, Range(0.1f, 3f)] private float fadeOutDuration = 1.0f;     
        #endregion

        #region Private Fields
        private Coroutine initRoutine;
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
            }
        }
        #endregion

        #region Core Flow
        /// <summary>
        /// Main initialization coroutine that handles splash sequence and scene transition
        /// </summary>
        private IEnumerator InitAndSplashRoutine() {
            float startTime = Time.unscaledTime; // Track when the process started (ignores time scale)

            // Step 1: Fade in Logo / Splash screen
            if (UIFadeManager.Instance != null) {
                yield return UIFadeManager.Instance.FadeInRoutine(fadeInDuration);
            }

            // Step 2: Initialize Core Systems
            yield return InitializeCoreSystemsRoutine();

            // Step 3: Ensure minimum display time without GC Allocation
            float elapsedTime = Time.unscaledTime - startTime;
            if (elapsedTime < minimumDisplayTime) {
                float remainingTime = minimumDisplayTime - elapsedTime;
                yield return WaitForSecondsNoAlloc(remainingTime); 
            }

            // Step 4: Proceed to Next Scene
            if (SceneManager.Instance != null) {
                SceneManager.Instance.LoadNextScene(fadeOutDuration);
            } else {
                Debug.LogError("[InitManager] SceneController.Instance is null! Cannot load next scene.");
            }
        }

        /// <summary>
        /// Delegates initialization tasks to their respective Managers instead of processing them here
        /// </summary>
        private IEnumerator InitializeCoreSystemsRoutine() {
            yield return null; 
        }

        /// <summary>
        /// Custom wait coroutine that avoids GC allocation 
        /// </summary>
        /// <param name="seconds">Time to wait in seconds</param>
        private IEnumerator WaitForSecondsNoAlloc(float seconds) {
            float timer = 0f;
            while (timer < seconds) {
                timer += Time.unscaledDeltaTime; // Use unscaled time to ignore time scale
                yield return null;
            }
        }
        #endregion
    }
}