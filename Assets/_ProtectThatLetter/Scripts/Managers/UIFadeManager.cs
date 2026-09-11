using System.Collections;
using UnityEngine;

namespace ProtectThatLetter.Managers {
    /// <summary>
    /// Handles pure CanvasGroup alpha transitions for visual fade effects.
    /// </summary>
    [DisallowMultipleComponent]
    public class UIFadeManager : MonoBehaviour {
        #region Instance
        // Singleton instance
        private static UIFadeManager instance;
        public static UIFadeManager Instance {
            get {
                if (instance == null) {
                    instance = FindFirstObjectByType<UIFadeManager>();
                }
                return instance;
            }
        }
        #endregion

        #region Serialized Fields
        [Header("UI References")]
        [SerializeField] private CanvasGroup fadeCanvasGroup;    // Canvas group for fade effects
        [SerializeField] private float defaultFadeDuration = 1.0f; // Default fade duration
        #endregion

        #region Private Fields
        private Coroutine currentFadeCoroutine; // Reference to the currently running fade coroutine
        #endregion

        #region Lifecycle
        /// <summary>
        /// Ensures singleton integrity and initializes the fade panel to fully opaque
        /// </summary>
        private void Awake() {
            // Destroy duplicate instances
            if (instance != null && instance != this) {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject); // Persist across scenes

            // Start with a fully opaque black screen blocking raycasts
            if (fadeCanvasGroup != null) {
                fadeCanvasGroup.alpha = 1f;
                fadeCanvasGroup.blocksRaycasts = true;
                fadeCanvasGroup.interactable = true;
                fadeCanvasGroup.gameObject.SetActive(true);
            } else {
                Debug.LogError("[UIFadeManager] CanvasGroup is not assigned!");
            }
        }

        /// <summary>
        /// Cleans up coroutines and instance reference when destroyed
        /// </summary>
        private void OnDestroy() {
            // Stop any running fade coroutine
            if (currentFadeCoroutine != null) {
                StopCoroutine(currentFadeCoroutine);
                currentFadeCoroutine = null;
            }

            // Clear singleton reference
            if (instance == this) {
                instance = null;
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Fades screen from Black (opaque) to Clear (transparent)
        /// </summary>
        /// <param name="duration">Duration of the fade (uses default if -1)</param>
        public IEnumerator FadeInRoutine(float duration = -1f) {
            float dur = duration > 0 ? duration : defaultFadeDuration;
            yield return StartFadeCoroutine(1f, 0f, dur); // Fade from 1 to 0
        }

        /// <summary>
        /// Fades screen from Clear (transparent) to Black (opaque)
        /// </summary>
        /// <param name="duration">Duration of the fade (uses default if -1)</param>
        public IEnumerator FadeOutRoutine(float duration = -1f) {
            float dur = duration > 0 ? duration : defaultFadeDuration;
            yield return StartFadeCoroutine(0f, 1f, dur); // Fade from 0 to 1
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Starts a fade coroutine, stopping any currently running one
        /// </summary>
        private Coroutine StartFadeCoroutine(float startAlpha, float endAlpha, float duration) {
            // Stop any existing fade coroutine
            if (currentFadeCoroutine != null) {
                StopCoroutine(currentFadeCoroutine);
                currentFadeCoroutine = null;
            }
            currentFadeCoroutine = StartCoroutine(FadeRoutine(startAlpha, endAlpha, duration));
            return currentFadeCoroutine;
        }

        /// <summary>
        /// Core fading coroutine that interpolates alpha over time
        /// </summary>
        private IEnumerator FadeRoutine(float startAlpha, float endAlpha, float duration) {
            // Validate canvas group reference
            if (fadeCanvasGroup == null) {
                currentFadeCoroutine = null;
                yield break;
            }

            // Ensure the fade panel is active and blocking input during fade
            fadeCanvasGroup.gameObject.SetActive(true);
            fadeCanvasGroup.blocksRaycasts = true;
            fadeCanvasGroup.interactable = true;

            float timer = 0f;
            while (timer < duration) {
                timer += Time.unscaledDeltaTime; // Use unscaled time to ignore time scale
                fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, timer / duration);
                yield return null;
            }

            // Ensure final alpha is exact
            fadeCanvasGroup.alpha = endAlpha;

            // Unblock input when fully transparent
            if (Mathf.Approximately(endAlpha, 0f)) {
                fadeCanvasGroup.blocksRaycasts = false;
                fadeCanvasGroup.interactable = false;
            }

            currentFadeCoroutine = null; // Clean up reference
        }
        #endregion
    }
}