using System.Collections;
using UnityEngine;

namespace ProtectThatLetter.Managers {
    /// <summary>
    /// Handles fade animation using CanvasGroup alpha transitions.
    /// </summary>
    [DisallowMultipleComponent]
    public class UIFadeManager : MonoBehaviour {
        #region Instance
        // Singleton instance with public getter and private setter
        public static UIFadeManager Instance { get; private set; }
        #endregion

        #region Serialized Fields
        [Header("UI References")]
        [SerializeField] private CanvasGroup fadeCanvasGroup;      // Canvas group for fade effects
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
            if (Instance != null && Instance != this) {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist across scenes

            // Start with a fully opaque black screen
            if (fadeCanvasGroup != null) {
                SetAlphaInstant(1f);
            } else {
                Debug.LogError($"[{nameof(UIFadeManager)}] CanvasGroup is not assigned!");
            }
        }

        /// <summary>
        /// Cleans up coroutines and clears the singleton reference when destroyed
        /// </summary>
        private void OnDestroy() {
            if (currentFadeCoroutine != null) {
                StopCoroutine(currentFadeCoroutine);
                currentFadeCoroutine = null;
            }

            if (Instance == this) {
                Instance = null;
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Instantly sets the alpha value without any fade animation
        /// </summary>
        /// <param name="targetAlpha">Target alpha value (0 = transparent, 1 = opaque)</param>
        public void SetAlphaInstant(float targetAlpha) {
            if (fadeCanvasGroup == null) return;

            // Stop any running fade coroutine
            if (currentFadeCoroutine != null) {
                StopCoroutine(currentFadeCoroutine);
                currentFadeCoroutine = null;
            }

            fadeCanvasGroup.alpha = targetAlpha;
            bool isVisible = targetAlpha > 0f;

            // Block input and enable object when visible
            fadeCanvasGroup.blocksRaycasts = isVisible;
            fadeCanvasGroup.interactable = isVisible;
            fadeCanvasGroup.gameObject.SetActive(isVisible);
        }

        /// <summary>
        /// Fades the screen from current alpha to transparent (0)
        /// </summary>
        /// <param name="duration">Duration of the fade (uses default if -1)</param>
        public IEnumerator FadeInRoutine(float duration = -1f) {
            float dur = duration > 0 ? duration : defaultFadeDuration;
            float startAlpha = fadeCanvasGroup != null ? fadeCanvasGroup.alpha : 1f;
            yield return StartFadeCoroutine(startAlpha, 0f, dur); // Fade to transparent
        }

        /// <summary>
        /// Fades the screen from current alpha to opaque (1)
        /// </summary>
        /// <param name="duration">Duration of the fade (uses default if -1)</param>
        public IEnumerator FadeOutRoutine(float duration = -1f) {
            float dur = duration > 0 ? duration : defaultFadeDuration;
            float startAlpha = fadeCanvasGroup != null ? fadeCanvasGroup.alpha : 0f;
            yield return StartFadeCoroutine(startAlpha, 1f, dur); // Fade to opaque
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
            }
            currentFadeCoroutine = StartCoroutine(FadeRoutine(startAlpha, endAlpha, duration));
            return currentFadeCoroutine;
        }

        /// <summary>
        /// Core fading coroutine that interpolates alpha over time
        /// </summary>
        private IEnumerator FadeRoutine(float startAlpha, float endAlpha, float duration) {
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

            // When fully transparent: unblock input and disable the object (optimization)
            if (Mathf.Approximately(endAlpha, 0f)) {
                fadeCanvasGroup.blocksRaycasts = false;
                fadeCanvasGroup.interactable = false;
                fadeCanvasGroup.gameObject.SetActive(false);
            }

            currentFadeCoroutine = null; // Clean up reference
        }
        #endregion
    }
}