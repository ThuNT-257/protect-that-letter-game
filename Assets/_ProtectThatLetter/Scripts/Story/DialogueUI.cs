using ProtectThatLetter.Managers;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProtectThatLetter.UI
{
    /// <summary>
    /// Handles dialogue presentation: typewriter effect, character display,
    /// avatar animations, background transitions, auto-play, and user input events.
    /// </summary>
    public class DialogueUI : MonoBehaviour
    {
        #region Serialized Fields
        [Header("Overlay Controls")]
        [SerializeField] private CanvasGroup overlayCanvasGroup; // Main dialogue overlay
        [SerializeField] private float fadeDuration = 0.4f;       // Fade animation duration

        [Header("Background Elements")]
        [SerializeField] private Image backgroundImage;           // Scene background

        [Header("Left Character Elements")]
        [SerializeField] private Image leftNameFrame;             // Left name frame
        [SerializeField] private TMP_Text leftNameText;           // Left name text
        [SerializeField] private RectTransform leftAvatarRect;    // Left avatar RectTransform
        [SerializeField] private Image leftAvatarImage;           // Left avatar Image

        [Header("Right Character Elements")]
        [SerializeField] private Image rightNameFrame;            // Right name frame
        [SerializeField] private TMP_Text rightNameText;          // Right name text
        [SerializeField] private RectTransform rightAvatarRect;   // Right avatar RectTransform
        [SerializeField] private Image rightAvatarImage;          // Right avatar Image

        [Header("Dialogue Elements")]
        [SerializeField] private TMP_Text dialogueLineText;       // Dialogue text
        [SerializeField] private Button skipButton;               // Skip button
        [SerializeField] private Button nextButton;               // Next button
        [SerializeField] private Button autoButton;               // Auto-play toggle

        [Header("Typewriter Settings")]
        [SerializeField] private float typingSpeed = 0.03f;       // Delay per character

        [Header("Auto Play Settings")]
        [SerializeField] private float autoDelayAfterType = 2.0f; // Delay before auto-next
        [SerializeField] private TMP_Text autoButtonText;         // Auto button label

        [Header("Auto Button Visuals")]
        [SerializeField] private float activeAlpha = 1.0f;        // Alpha when auto is ON
        [SerializeField] private float inactiveAlpha = 0.5f;      // Alpha when auto is OFF

        [Header("Avatar Focus Visuals & Animation")]
        [SerializeField] private float avatarAnimDuration = 0.3f;  // Animation duration
        [SerializeField] private float inactiveScale = 0.75f;     // Scale for non-speaking avatar
        [SerializeField] private float inactiveShiftX = 50f;      // Horizontal offset for non-speaking avatar
        [SerializeField] private Color activeAvatarColor = Color.white;                                  // Active speaker tint
        [SerializeField] private Color inactiveAvatarColor = new Color(0.55f, 0.55f, 0.55f, 1f); // Inactive dim
        #endregion

        #region Events
        /// <summary>
        /// Raised when the player clicks Next/Skip, or when auto-play advances.
        /// </summary>
        public static event Action OnNextButtonClicked;
        #endregion

        #region Private Fields
        private Coroutine overlayFadeCoroutine;      // Overlay fade coroutine
        private Coroutine backgroundFadeCoroutine;   // Background fade coroutine
        private Coroutine typewriterCoroutine;       // Typewriter coroutine
        private Coroutine autoNextCoroutine;         // Auto-next coroutine
        private Coroutine leftAvatarCoroutine;       // Left avatar animation coroutine
        private Coroutine rightAvatarCoroutine;      // Right avatar animation coroutine

        private Vector2 leftAvatarDefaultPos;        // Original anchored position for left avatar
        private Vector2 rightAvatarDefaultPos;       // Original anchored position for right avatar

        private bool isTyping = false;               // True while typewriter is running
        private bool isAutoMode = false;             // True when auto-play is enabled
        private Image autoButtonImage;               // Cached auto button image
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            // Cache original avatar anchored positions
            if (leftAvatarRect != null) leftAvatarDefaultPos = leftAvatarRect.anchoredPosition;
            if (rightAvatarRect != null) rightAvatarDefaultPos = rightAvatarRect.anchoredPosition;

            // Fallback RectTransform refs if not set manually in Inspector
            if (leftAvatarRect == null && leftAvatarImage != null)
                leftAvatarRect = leftAvatarImage.rectTransform;
            if (rightAvatarRect == null && rightAvatarImage != null)
                rightAvatarRect = rightAvatarImage.rectTransform;

            // Cache auto button references
            if (autoButton != null)
            {
                autoButtonImage = autoButton.GetComponent<Image>();
                if (autoButtonText == null)
                {
                    autoButtonText = autoButton.GetComponentInChildren<TMP_Text>();
                }
            }

            UpdateAutoButtonVisual();
            HideDialogueOverlay(immediate: true);
        }

        /// <summary>
        /// Subscribes to button click events when enabled
        /// </summary>
        private void OnEnable()
        {
            if (skipButton != null) skipButton.onClick.AddListener(HandleNextClick);
            if (nextButton != null) nextButton.onClick.AddListener(HandleNextClick);
            if (autoButton != null) autoButton.onClick.AddListener(ToggleAutoMode);
        }

        /// <summary>
        /// Unsubscribes from button click events and stops coroutines when disabled to prevent memory leaks
        /// </summary>
        private void OnDisable()
        {
            if (skipButton != null) skipButton.onClick.RemoveListener(HandleNextClick);
            if (nextButton != null) nextButton.onClick.RemoveListener(HandleNextClick);
            if (autoButton != null) autoButton.onClick.RemoveListener(ToggleAutoMode);

            StopAllCoroutines();
            isTyping = false;
        }

        private void OnDestroy()
        {
            OnNextButtonClicked = null;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Displays a dialogue line with typewriter effect, animated avatars, background, and SFX
        /// </summary>
        public void DisplayLine(DialogueLine line, bool isFirstLine = false)
        {
            if (line == null) return;

            // Play "clack" SFX on each new line
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayStorySFX("clack");
            }

            StopAutoNextCoroutine();
            ShowDialogueOverlay();

            // Set background with crossfade if specified
            if (!string.IsNullOrEmpty(line.background) && backgroundImage != null)
            {
                SetBackgroundWithFade(line.background);
            }

            // Play sound effect if specified
            if (!string.IsNullOrEmpty(line.sound))
            {
                PlaySFXSound(line.sound);
            }

            // Start typewriter effect
            if (dialogueLineText != null)
            {
                StartTypewriterEffect(line.content);
            }

            // Determine active speaker position
            bool isLeft = string.IsNullOrEmpty(line.position) || line.position.ToLower() == "left";

            // Update Name Frames & Texts
            if (leftNameFrame != null) leftNameFrame.gameObject.SetActive(isLeft);
            if (leftNameText != null && isLeft) leftNameText.text = line.speaker;

            if (rightNameFrame != null) rightNameFrame.gameObject.SetActive(!isLeft);
            if (rightNameText != null && !isLeft) rightNameText.text = line.speaker;

            // Animate Avatars
            AnimateAvatar(isLeft: true, isSpeaking: isLeft, line: line, isFirstLine: isFirstLine);
            AnimateAvatar(isLeft: false, isSpeaking: !isLeft, line: line, isFirstLine: isFirstLine);
        }

        /// <summary>
        /// Hides the dialogue overlay
        /// </summary>
        public void HideDialogueOverlay(bool immediate = false)
        {
            StopAutoNextCoroutine();

            if (overlayCanvasGroup == null) return;

            overlayCanvasGroup.interactable = false;
            overlayCanvasGroup.blocksRaycasts = false;

            if (overlayFadeCoroutine != null) StopCoroutine(overlayFadeCoroutine);

            if (immediate)
            {
                overlayCanvasGroup.alpha = 0f;
            }
            else
            {
                overlayFadeCoroutine = StartCoroutine(FadeCanvasGroupRoutine(
                    overlayCanvasGroup, overlayCanvasGroup.alpha, 0f, fadeDuration, null));
            }
        }

        /// <summary>
        /// Shows the dialogue overlay
        /// </summary>
        public void ShowDialogueOverlay(bool immediate = false)
        {
            if (overlayCanvasGroup == null) return;

            if (overlayFadeCoroutine != null) StopCoroutine(overlayFadeCoroutine);

            if (immediate)
            {
                overlayCanvasGroup.alpha = 1f;
                overlayCanvasGroup.interactable = true;
                overlayCanvasGroup.blocksRaycasts = true;
            }
            else
            {
                overlayFadeCoroutine = StartCoroutine(FadeCanvasGroupRoutine(
                    overlayCanvasGroup, overlayCanvasGroup.alpha, 1f, fadeDuration, () =>
                    {
                        overlayCanvasGroup.interactable = true;
                        overlayCanvasGroup.blocksRaycasts = true;
                    }));
            }
        }

        /// <summary>
        /// Plays intro media (background + sound) before the first line
        /// </summary>
        public void PlayIntroMedia(string backgroundName, string soundName)
        {
            if (!string.IsNullOrEmpty(backgroundName) && backgroundImage != null)
            {
                SetBackgroundWithFade(backgroundName);
            }
            if (!string.IsNullOrEmpty(soundName)) PlaySFXSound(soundName);
        }
        #endregion

        #region Private Methods & Avatar Animation
        /// <summary>
        /// Handles avatar image loading and triggers smooth scaling, positioning, and color tint transitions.
        /// </summary>
        private void AnimateAvatar(bool isLeft, bool isSpeaking, DialogueLine line, bool isFirstLine)
        {
            RectTransform avatarRect = isLeft ? leftAvatarRect : rightAvatarRect;
            Image avatarImg = isLeft ? leftAvatarImage : rightAvatarImage;
            Vector2 defaultPos = isLeft ? leftAvatarDefaultPos : rightAvatarDefaultPos;

            if (avatarRect == null || avatarImg == null) return;

            // If it's the first line and this avatar isn't speaking, hide it
            if (isFirstLine && !isSpeaking && string.IsNullOrEmpty(line?.avatar))
            {
                avatarRect.gameObject.SetActive(false);
                return;
            }

            // Load sprite if speaking or provided
            if (isSpeaking && line != null && !string.IsNullOrEmpty(line.avatar))
            {
                Sprite avatarSprite = Resources.Load<Sprite>($"Avatars/{line.avatar}");
                if (avatarSprite != null)
                {
                    avatarImg.sprite = avatarSprite;
                }
            }

            // Make sure active avatar is visible if sprite exists
            if (avatarImg.sprite == null)
            {
                avatarRect.gameObject.SetActive(false);
                return;
            }

            avatarRect.gameObject.SetActive(true);

            // Compute target transform and visual values
            Vector3 targetScale = isSpeaking ? Vector3.one : new Vector3(inactiveScale, inactiveScale, 1f);
            Vector2 targetPos = defaultPos;
            Color targetColor = isSpeaking ? activeAvatarColor : inactiveAvatarColor;

            if (!isSpeaking)
            {
                // Shift non-speaking avatar slightly outward and downwards to match pivot scaling
                float shiftX = isLeft ? -inactiveShiftX : inactiveShiftX;
                targetPos.x += shiftX;

                float height = avatarRect.rect.height;
                float scaleDifference = 1f - inactiveScale;
                float bottomOffset = height * scaleDifference * avatarRect.pivot.y;
                targetPos.y -= bottomOffset;
            }

            // Trigger animation coroutine for corresponding side
            if (isLeft)
            {
                if (leftAvatarCoroutine != null) StopCoroutine(leftAvatarCoroutine);
                leftAvatarCoroutine = StartCoroutine(AnimateAvatarRoutine(avatarRect, avatarImg, targetScale, targetPos, targetColor, isSpeaking));
            }
            else
            {
                if (rightAvatarCoroutine != null) StopCoroutine(rightAvatarCoroutine);
                rightAvatarCoroutine = StartCoroutine(AnimateAvatarRoutine(avatarRect, avatarImg, targetScale, targetPos, targetColor, isSpeaking));
            }
        }

        /// <summary>
        /// Sets the background with a crossfade effect
        /// </summary>
        private void SetBackgroundWithFade(string bgName)
        {
            Sprite newSprite = Resources.Load<Sprite>($"Backgrounds/{bgName}");
            if (newSprite == null)
            {
                Debug.LogError($"[DialogueUI] Background failed to load at path: Backgrounds/{bgName}");
                return;
            }
            if (backgroundImage == null) return;

            if (backgroundImage.sprite == newSprite && backgroundImage.color.a > 0.9f) return;

            if (backgroundFadeCoroutine != null) StopCoroutine(backgroundFadeCoroutine);
            backgroundFadeCoroutine = StartCoroutine(FadeBackgroundRoutine(newSprite));
        }

        /// <summary>
        /// Plays an SFX sound loaded from Resources
        /// </summary>
        private void PlaySFXSound(string soundName)
        {
            if (AudioManager.Instance == null) return;

            AudioClip clip = Resources.Load<AudioClip>($"Sounds/{soundName}");
            if (clip != null)
            {
                AudioManager.Instance.PlayStorySFX(clip);
            }
        }

        /// <summary>
        /// Updates auto button alpha based on current auto state
        /// </summary>
        private void UpdateAutoButtonVisual()
        {
            float targetAlpha = isAutoMode ? activeAlpha : inactiveAlpha;

            if (autoButtonImage != null)
            {
                Color imgColor = autoButtonImage.color;
                imgColor.a = targetAlpha;
                autoButtonImage.color = imgColor;
            }

            if (autoButtonText != null)
            {
                Color textColor = autoButtonText.color;
                textColor.a = targetAlpha;
                autoButtonText.color = textColor;
            }
        }

        /// <summary>
        /// Starts the typewriter effect for the given content
        /// </summary>
        private void StartTypewriterEffect(string content)
        {
            if (typewriterCoroutine != null) StopCoroutine(typewriterCoroutine);

            dialogueLineText.text = content ?? "";
            typewriterCoroutine = StartCoroutine(TypewriterRoutine());
        }

        /// <summary>
        /// Immediately completes the typewriter effect
        /// </summary>
        private void CompleteTypewriterImmediately()
        {
            if (typewriterCoroutine != null) StopCoroutine(typewriterCoroutine);

            dialogueLineText.maxVisibleCharacters = dialogueLineText.textInfo.characterCount;
            OnTypewriterCompleted();
        }

        /// <summary>
        /// Called when the typewriter finishes; starts auto-next if enabled
        /// </summary>
        private void OnTypewriterCompleted()
        {
            isTyping = false;

            if (isAutoMode)
            {
                StartAutoNextCoroutine();
            }
        }

        /// <summary>
        /// Starts the auto-next countdown
        /// </summary>
        private void StartAutoNextCoroutine()
        {
            StopAutoNextCoroutine();
            autoNextCoroutine = StartCoroutine(AutoNextRoutine());
        }

        /// <summary>
        /// Stops the auto-next countdown
        /// </summary>
        private void StopAutoNextCoroutine()
        {
            if (autoNextCoroutine != null)
            {
                StopCoroutine(autoNextCoroutine);
                autoNextCoroutine = null;
            }
        }
        #endregion

        #region Coroutines
        /// <summary>
        /// Smoothly animates avatar scale, position, and color tint using Easing curves
        /// </summary>
        private IEnumerator AnimateAvatarRoutine(RectTransform avatarRect, Image img, Vector3 targetScale, Vector2 targetPos, Color targetColor, bool isSpeaking)
        {
            float elapsed = 0f;
            Vector3 startScale = avatarRect.localScale;
            Vector2 startPos = avatarRect.anchoredPosition;
            Color startColor = img != null ? img.color : Color.white;

            while (elapsed < avatarAnimDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / avatarAnimDuration;

                float scaleT = isSpeaking ? EaseOutBack(t) : EaseOutCubic(t);

                avatarRect.localScale = Vector3.LerpUnclamped(startScale, targetScale, scaleT);
                avatarRect.anchoredPosition = Vector2.Lerp(startPos, targetPos, EaseOutCubic(t));

                if (img != null)
                {
                    img.color = Color.Lerp(startColor, targetColor, t);
                }

                yield return null;
            }

            avatarRect.localScale = targetScale;
            avatarRect.anchoredPosition = targetPos;
            if (img != null) img.color = targetColor;
        }

        private float EaseOutBack(float x)
        {
            float c1 = 1.4f;
            float c3 = c1 + 1f;
            return 1f + c3 * Mathf.Pow(x - 1f, 3f) + c1 * Mathf.Pow(x - 1f, 2f);
        }

        private float EaseOutCubic(float x)
        {
            return 1f - Mathf.Pow(1f - x, 3f);
        }

        /// <summary>
        /// Typewriter coroutine that reveals characters one by one
        /// </summary>
        private IEnumerator TypewriterRoutine()
        {
            isTyping = true;
            dialogueLineText.ForceMeshUpdate();

            int totalVisibleCharacters = dialogueLineText.textInfo.characterCount;
            dialogueLineText.maxVisibleCharacters = 0;

            for (int visibleCount = 1; visibleCount <= totalVisibleCharacters; visibleCount++)
            {
                dialogueLineText.maxVisibleCharacters = visibleCount;
                yield return new WaitForSeconds(typingSpeed);
            }

            OnTypewriterCompleted();
        }

        /// <summary>
        /// Waits, then fires the next-line event
        /// </summary>
        private IEnumerator AutoNextRoutine()
        {
            yield return new WaitForSeconds(autoDelayAfterType);
            OnNextButtonClicked?.Invoke();
        }

        /// <summary>
        /// Fades a CanvasGroup from startAlpha to targetAlpha
        /// </summary>
        private IEnumerator FadeCanvasGroupRoutine(CanvasGroup group, float startAlpha, float targetAlpha, float duration, Action onComplete)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                group.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);
                yield return null;
            }
            group.alpha = targetAlpha;
            onComplete?.Invoke();
        }

        /// <summary>
        /// Crossfades the background sprite
        /// </summary>
        private IEnumerator FadeBackgroundRoutine(Sprite newSprite)
        {
            float halfDuration = fadeDuration * 0.5f;
            float elapsed = 0f;
            Color color = backgroundImage.color;

            // Fade out
            while (elapsed < halfDuration)
            {
                elapsed += Time.deltaTime;
                color.a = Mathf.Lerp(1f, 0f, elapsed / halfDuration);
                backgroundImage.color = color;
                yield return null;
            }

            backgroundImage.sprite = newSprite;

            // Fade in
            elapsed = 0f;
            while (elapsed < halfDuration)
            {
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
        /// Toggles auto-play mode and starts/stops auto-next accordingly
        /// </summary>
        private void ToggleAutoMode()
        {
            isAutoMode = !isAutoMode;
            UpdateAutoButtonVisual();

            if (isAutoMode)
            {
                if (!isTyping)
                {
                    StartAutoNextCoroutine();
                }
            }
            else
            {
                StopAutoNextCoroutine();
            }
        }

        /// <summary>
        /// Handles Next/Skip: completes typewriter or advances to next line
        /// </summary>
        private void HandleNextClick()
        {
            if (isTyping)
            {
                CompleteTypewriterImmediately();
            }
            else
            {
                StopAutoNextCoroutine();
                OnNextButtonClicked?.Invoke();
            }
        }
        #endregion
    }
}