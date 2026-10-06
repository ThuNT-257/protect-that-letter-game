using ProtectThatLetter.Definitions;
using ProtectThatLetter.Managers;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProtectThatLetter.UI {
    /// <summary>
    /// Orchestrator for dialogue flow: handles user input, UI overlay transitions,
    /// auto-play timers, and coordinates sub-components (Typewriter & AvatarAnimator).
    /// </summary>
    public class DialogueUI : MonoBehaviour {
        #region Serialized Fields
        [Header("Sub Components")]
        [SerializeField] private DialogueTypewriter typewriter;             // Typewriter effect
        [SerializeField] private DialogueAvatarAnimator avatarAnimator;     // Avatar animations

        [Header("Overlay Controls")]
        [SerializeField] private CanvasGroup overlayCanvasGroup;           // Main dialogue overlay
        [SerializeField] private float fadeDuration = 0.4f;                 // Fade duration for overlay/background

        [Header("Background Elements")]
        [SerializeField] private Image backgroundImage;                     // Scene background

        [Header("Name Frames & Labels")]
        [SerializeField] private Image leftNameFrame;                       // Left name frame
        [SerializeField] private TMP_Text leftNameText;                     // Left name text
        [SerializeField] private Image rightNameFrame;                      // Right name frame
        [SerializeField] private TMP_Text rightNameText;                    // Right name text

        [Header("Buttons & Controls")]
        [SerializeField] private Button skipButton;                         // Skip button
        [SerializeField] private Button nextButton;                         // Next button
        [SerializeField] private Button autoButton;                         // Auto-play toggle
        [SerializeField] private TMP_Text autoButtonText;                   // Auto button label

        [Header("Auto Play Settings")]
        [SerializeField] private float autoDelayAfterType = 2.0f;           // Delay before auto-next
        [SerializeField] private float activeAlpha = 1.0f;                  // Alpha when auto is ON
        [SerializeField] private float inactiveAlpha = 0.5f;                // Alpha when auto is OFF
        #endregion

        #region Events
        /// <summary>
        /// Raised when the player advances to the next line (manual or auto).
        /// </summary>
        public static event Action OnNextButtonClicked;
        #endregion

        #region Private Fields
        private Coroutine overlayFadeCoroutine;      // Overlay fade coroutine
        private Coroutine backgroundFadeCoroutine;   // Background fade coroutine
        private Coroutine autoNextCoroutine;         // Auto-next countdown coroutine

        private bool isAutoMode = false;             // True when auto-play is enabled
        private Image autoButtonImage;               // Cached auto button image
        #endregion

        #region Unity Lifecycle
        /// <summary>
        /// Caches sub-components and initializes UI state
        /// </summary>
        private void Awake() {
            // Auto-fallback components if not assigned in Inspector
            if (typewriter == null) typewriter = GetComponentInChildren<DialogueTypewriter>();
            if (avatarAnimator == null) avatarAnimator = GetComponentInChildren<DialogueAvatarAnimator>();

            // Cache auto button image and label
            if (autoButton != null) {
                autoButtonImage = autoButton.GetComponent<Image>();
                if (autoButtonText == null) {
                    autoButtonText = autoButton.GetComponentInChildren<TMP_Text>();
                }
            }

            UpdateAutoButtonVisual();
            HideDialogueOverlay(immediate: true);
        }

        /// <summary>
        /// Subscribes to button and typewriter events when enabled
        /// </summary>
        private void OnEnable() {
            // Register button listeners
            if (skipButton != null) skipButton.onClick.AddListener(HandleNextClick);
            if (nextButton != null) nextButton.onClick.AddListener(HandleNextClick);
            if (autoButton != null) autoButton.onClick.AddListener(ToggleAutoMode);

            // Subscribe to typewriter completion
            if (typewriter != null) {
                typewriter.OnTypewriterCompleted += HandleTypewriterCompleted;
            }
        }

        /// <summary>
        /// Unsubscribes from events and stops all coroutines when disabled
        /// </summary>
        private void OnDisable() {
            // Unregister button listeners
            if (skipButton != null) skipButton.onClick.RemoveListener(HandleNextClick);
            if (nextButton != null) nextButton.onClick.RemoveListener(HandleNextClick);
            if (autoButton != null) autoButton.onClick.RemoveListener(ToggleAutoMode);

            // Unsubscribe from typewriter completion
            if (typewriter != null) {
                typewriter.OnTypewriterCompleted -= HandleTypewriterCompleted;
            }

            // Stop all coroutines and clear references
            StopAllCoroutines();
            overlayFadeCoroutine = null;
            backgroundFadeCoroutine = null;
            autoNextCoroutine = null;
        }

        /// <summary>
        /// Clears the static event when destroyed to prevent memory leaks
        /// </summary>
        private void OnDestroy() {
            OnNextButtonClicked = null;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Displays a dialogue line: typewriter, background, character, and SFX
        /// </summary>
        public void DisplayLine(DialogueLine line, bool isFirstLine = false) {
            if (line == null) return;

            // Play new-line SFX
            if (AudioManager.Instance != null) {
                AudioManager.Instance.PlayStorySFX(GameDefinitions.SfxNames.SFX_DIALOGUE_NEW_LINE);
            }

            StopAutoNextCoroutine();
            ShowDialogueOverlay();

            // Set background with crossfade if specified
            if (!string.IsNullOrEmpty(line.background) && backgroundImage != null) {
                SetBackgroundWithFade(line.background);
            }

            // Play sound effect if specified
            if (!string.IsNullOrEmpty(line.sound)) {
                PlaySFXSound(line.sound);
            }

            // Start typewriter effect
            if (typewriter != null) {
                typewriter.StartTypewriter(line.content);
            }

            // Determine speaker side & update name tags
            bool isLeft = string.IsNullOrEmpty(line.position) || line.position.ToLower() == "left";
            if (leftNameFrame != null) leftNameFrame.gameObject.SetActive(isLeft);
            if (leftNameText != null && isLeft) leftNameText.text = line.speaker;

            if (rightNameFrame != null) rightNameFrame.gameObject.SetActive(!isLeft);
            if (rightNameText != null && !isLeft) rightNameText.text = line.speaker;

            // Trigger avatar animation
            if (avatarAnimator != null) {
                avatarAnimator.AnimateAvatars(isLeft, line, isFirstLine);
            }
        }

        /// <summary>
        /// Hides the dialogue overlay (fade or immediate)
        /// </summary>
        public void HideDialogueOverlay(bool immediate = false) {
            StopAutoNextCoroutine();

            if (overlayCanvasGroup == null) return;

            overlayCanvasGroup.interactable = false;
            overlayCanvasGroup.blocksRaycasts = false;

            SafeStopCoroutine(ref overlayFadeCoroutine);

            if (immediate) {
                overlayCanvasGroup.alpha = 0f;
            } else {
                overlayFadeCoroutine = StartCoroutine(FadeCanvasGroupRoutine(
                    overlayCanvasGroup, overlayCanvasGroup.alpha, 0f, fadeDuration, null));
            }
        }

        /// <summary>
        /// Shows the dialogue overlay (fade or immediate)
        /// </summary>
        public void ShowDialogueOverlay(bool immediate = false) {
            if (overlayCanvasGroup == null) return;

            SafeStopCoroutine(ref overlayFadeCoroutine);

            if (immediate) {
                overlayCanvasGroup.alpha = 1f;
                overlayCanvasGroup.interactable = true;
                overlayCanvasGroup.blocksRaycasts = true;
            } else {
                overlayFadeCoroutine = StartCoroutine(FadeCanvasGroupRoutine(
                    overlayCanvasGroup, overlayCanvasGroup.alpha, 1f, fadeDuration, () => {
                        overlayCanvasGroup.interactable = true;
                        overlayCanvasGroup.blocksRaycasts = true;
                    }));
            }
        }

        /// <summary>
        /// Plays intro media (background + sound) before the first line
        /// </summary>
        public void PlayIntroMedia(string backgroundName, string soundName) {
            if (!string.IsNullOrEmpty(backgroundName) && backgroundImage != null) {
                SetBackgroundWithFade(backgroundName);
            }
            if (!string.IsNullOrEmpty(soundName)) PlaySFXSound(soundName);
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Updates auto button alpha based on current auto state
        /// </summary>
        private void UpdateAutoButtonVisual() {
            float targetAlpha = isAutoMode ? activeAlpha : inactiveAlpha;

            if (autoButtonImage != null) {
                Color imgColor = autoButtonImage.color;
                imgColor.a = targetAlpha;
                autoButtonImage.color = imgColor;
            }

            if (autoButtonText != null) {
                Color textColor = autoButtonText.color;
                textColor.a = targetAlpha;
                autoButtonText.color = textColor;
            }
        }

        /// <summary>
        /// Starts the auto-next countdown coroutine
        /// </summary>
        private void StartAutoNextCoroutine() {
            StopAutoNextCoroutine();
            autoNextCoroutine = StartCoroutine(AutoNextRoutine());
        }

        /// <summary>
        /// Stops the auto-next countdown coroutine
        /// </summary>
        private void StopAutoNextCoroutine() {
            SafeStopCoroutine(ref autoNextCoroutine);
        }

        /// <summary>
        /// Sets the background with a crossfade effect
        /// </summary>
        private void SetBackgroundWithFade(string bgName) {
            Sprite newSprite = Resources.Load<Sprite>($"Backgrounds/{bgName}");
            if (newSprite == null) {
                Debug.LogError($"[DialogueUI] Background failed to load at path: Backgrounds/{bgName}");
                return;
            }
            if (backgroundImage == null) return;

            // Skip if the same background is already shown
            if (backgroundImage.sprite == newSprite && backgroundImage.color.a > 0.9f) return;

            SafeStopCoroutine(ref backgroundFadeCoroutine);
            backgroundFadeCoroutine = StartCoroutine(FadeBackgroundRoutine(newSprite));
        }

        /// <summary>
        /// Plays an SFX sound loaded from Resources
        /// </summary>
        private void PlaySFXSound(string soundName) {
            if (AudioManager.Instance == null) return;

            AudioClip clip = Resources.Load<AudioClip>($"Sounds/{soundName}");
            if (clip != null) {
                AudioManager.Instance.PlayStorySFX(clip);
            }
        }

        /// <summary>
        /// Safely stops a coroutine and clears the reference
        /// </summary>
        private void SafeStopCoroutine(ref Coroutine coroutine) {
            if (coroutine != null) {
                StopCoroutine(coroutine);
                coroutine = null;
            }
        }
        #endregion

        #region Coroutines
        /// <summary>
        /// Waits, then fires the next-line event (used for auto-play)
        /// </summary>
        private IEnumerator AutoNextRoutine() {
            yield return new WaitForSeconds(autoDelayAfterType);
            OnNextButtonClicked?.Invoke();
        }

        /// <summary>
        /// Fades a CanvasGroup from startAlpha to targetAlpha
        /// </summary>
        private IEnumerator FadeCanvasGroupRoutine(CanvasGroup group, float startAlpha, float targetAlpha, float duration, Action onComplete) {
            float elapsed = 0f;
            while (elapsed < duration) {
                elapsed += Time.deltaTime;
                group.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);
                yield return null;
            }
            group.alpha = targetAlpha;
            onComplete?.Invoke();
        }

        /// <summary>
        /// Crossfades the background sprite (fade out → swap → fade in)
        /// </summary>
        private IEnumerator FadeBackgroundRoutine(Sprite newSprite) {
            float halfDuration = fadeDuration * 0.5f;
            float elapsed = 0f;
            Color color = backgroundImage.color;

            // Fade out
            while (elapsed < halfDuration) {
                elapsed += Time.deltaTime;
                color.a = Mathf.Lerp(1f, 0f, elapsed / halfDuration);
                backgroundImage.color = color;
                yield return null;
            }

            backgroundImage.sprite = newSprite;

            // Fade in
            elapsed = 0f;
            while (elapsed < halfDuration) {
                elapsed += Time.deltaTime;
                color.a = Mathf.Lerp(0f, 1f, elapsed / halfDuration);
                backgroundImage.color = color;
                yield return null;
            }
            color.a = 1f;
            backgroundImage.color = color;
        }
        #endregion

        #region Event Handlers
        /// <summary>
        /// Handles Next/Skip: completes typewriter or advances to next line
        /// </summary>
        private void HandleNextClick() {
            if (typewriter != null && typewriter.IsTyping) {
                typewriter.CompleteImmediately(); // Skip typing
            } else {
                StopAutoNextCoroutine();
                OnNextButtonClicked?.Invoke();    // Advance to next line
            }
        }

        /// <summary>
        /// Called when the typewriter finishes; starts auto-next if enabled
        /// </summary>
        private void HandleTypewriterCompleted() {
            if (isAutoMode) {
                StartAutoNextCoroutine();
            }
        }

        /// <summary>
        /// Toggles auto-play mode and starts/stops auto-next accordingly
        /// </summary>
        private void ToggleAutoMode() {
            isAutoMode = !isAutoMode;
            UpdateAutoButtonVisual();

            if (isAutoMode) {
                // Start auto-next only if typewriter has finished
                if (typewriter == null || !typewriter.IsTyping) {
                    StartAutoNextCoroutine();
                }
            } else {
                StopAutoNextCoroutine();
            }
        }
        #endregion
    }
}