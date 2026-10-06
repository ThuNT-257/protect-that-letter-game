using ProtectThatLetter.Managers;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProtectThatLetter.UI
{
    /// <summary>
    /// Orchestrator for dialogue flow: handles user input, UI overlay transitions,
    /// auto-play timers, and coordinates sub-components (Typewriter & AvatarAnimator).
    /// </summary>
    public class DialogueUI : MonoBehaviour
    {
        #region Serialized Fields
        [Header("Sub Components")]
        [SerializeField] private DialogueTypewriter typewriter;
        [SerializeField] private DialogueAvatarAnimator avatarAnimator;

        [Header("Overlay Controls")]
        [SerializeField] private CanvasGroup overlayCanvasGroup;
        [SerializeField] private float fadeDuration = 0.4f;

        [Header("Background Elements")]
        [SerializeField] private Image backgroundImage;

        [Header("Name Frames & Labels")]
        [SerializeField] private Image leftNameFrame;
        [SerializeField] private TMP_Text leftNameText;
        [SerializeField] private Image rightNameFrame;
        [SerializeField] private TMP_Text rightNameText;

        [Header("Buttons & Controls")]
        [SerializeField] private Button skipButton;
        [SerializeField] private Button nextButton;
        [SerializeField] private Button autoButton;
        [SerializeField] private TMP_Text autoButtonText;

        [Header("Auto Play Settings")]
        [SerializeField] private float autoDelayAfterType = 2.0f;
        [SerializeField] private float activeAlpha = 1.0f;
        [SerializeField] private float inactiveAlpha = 0.5f;
        #endregion

        #region Events
        public static event Action OnNextButtonClicked;
        #endregion

        #region Private Fields
        private Coroutine overlayFadeCoroutine;
        private Coroutine backgroundFadeCoroutine;
        private Coroutine autoNextCoroutine;

        private bool isAutoMode = false;
        private Image autoButtonImage;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            // Auto fallback components if not set via Inspector
            if (typewriter == null) typewriter = GetComponentInChildren<DialogueTypewriter>();
            if (avatarAnimator == null) avatarAnimator = GetComponentInChildren<DialogueAvatarAnimator>();

            if (autoButton != null)
            {
                autoButtonImage = autoButton.GetComponent<Image>();
                if (autoButtonText == null)
                    autoButtonText = autoButton.GetComponentInChildren<TMP_Text>();
            }

            UpdateAutoButtonVisual();
            HideDialogueOverlay(immediate: true);
        }

        private void OnEnable()
        {
            if (skipButton != null) skipButton.onClick.AddListener(HandleNextClick);
            if (nextButton != null) nextButton.onClick.AddListener(HandleNextClick);
            if (autoButton != null) autoButton.onClick.AddListener(ToggleAutoMode);

            if (typewriter != null)
            {
                typewriter.OnTypewriterCompleted += HandleTypewriterCompleted;
            }
        }

        private void OnDisable()
        {
            if (skipButton != null) skipButton.onClick.RemoveListener(HandleNextClick);
            if (nextButton != null) nextButton.onClick.RemoveListener(HandleNextClick);
            if (autoButton != null) autoButton.onClick.RemoveListener(ToggleAutoMode);

            if (typewriter != null)
            {
                typewriter.OnTypewriterCompleted -= HandleTypewriterCompleted;
            }

            StopAllCoroutines();

            overlayFadeCoroutine = null;
            backgroundFadeCoroutine = null;
            autoNextCoroutine = null;
        }

        private void OnDestroy()
        {
            OnNextButtonClicked = null;
        }
        #endregion

        #region Public API
        public void DisplayLine(DialogueLine line, bool isFirstLine = false)
        {
            if (line == null) return;

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayStorySFX("clack");
            }

            StopAutoNextCoroutine();
            ShowDialogueOverlay();

            if (!string.IsNullOrEmpty(line.background) && backgroundImage != null)
            {
                SetBackgroundWithFade(line.background);
            }

            if (!string.IsNullOrEmpty(line.sound))
            {
                PlaySFXSound(line.sound);
            }

            // Trigger Typewriter
            if (typewriter != null)
            {
                typewriter.StartTypewriter(line.content);
            }

            // Determine speaker side & update name tags
            bool isLeft = string.IsNullOrEmpty(line.position) || line.position.ToLower() == "left";
            if (leftNameFrame != null) leftNameFrame.gameObject.SetActive(isLeft);
            if (leftNameText != null && isLeft) leftNameText.text = line.speaker;

            if (rightNameFrame != null) rightNameFrame.gameObject.SetActive(!isLeft);
            if (rightNameText != null && !isLeft) rightNameText.text = line.speaker;

            // Trigger Avatar Animation
            if (avatarAnimator != null)
            {
                avatarAnimator.AnimateAvatars(isLeft, line, isFirstLine);
            }
        }

        public void HideDialogueOverlay(bool immediate = false)
        {
            StopAutoNextCoroutine();

            if (overlayCanvasGroup == null) return;

            overlayCanvasGroup.interactable = false;
            overlayCanvasGroup.blocksRaycasts = false;

            SafeStopCoroutine(ref overlayFadeCoroutine);

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

        public void ShowDialogueOverlay(bool immediate = false)
        {
            if (overlayCanvasGroup == null) return;

            SafeStopCoroutine(ref overlayFadeCoroutine);

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

        public void PlayIntroMedia(string backgroundName, string soundName)
        {
            if (!string.IsNullOrEmpty(backgroundName) && backgroundImage != null)
            {
                SetBackgroundWithFade(backgroundName);
            }
            if (!string.IsNullOrEmpty(soundName)) PlaySFXSound(soundName);
        }
        #endregion

        #region Private Helpers & Handlers
        private void HandleNextClick()
        {
            if (typewriter != null && typewriter.IsTyping)
            {
                typewriter.CompleteImmediately();
            }
            else
            {
                StopAutoNextCoroutine();
                OnNextButtonClicked?.Invoke();
            }
        }

        private void HandleTypewriterCompleted()
        {
            if (isAutoMode)
            {
                StartAutoNextCoroutine();
            }
        }

        private void ToggleAutoMode()
        {
            isAutoMode = !isAutoMode;
            UpdateAutoButtonVisual();

            if (isAutoMode)
            {
                if (typewriter == null || !typewriter.IsTyping)
                {
                    StartAutoNextCoroutine();
                }
            }
            else
            {
                StopAutoNextCoroutine();
            }
        }

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

        private void StartAutoNextCoroutine()
        {
            StopAutoNextCoroutine();
            autoNextCoroutine = StartCoroutine(AutoNextRoutine());
        }

        private void StopAutoNextCoroutine()
        {
            SafeStopCoroutine(ref autoNextCoroutine);
        }

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

            SafeStopCoroutine(ref backgroundFadeCoroutine);
            backgroundFadeCoroutine = StartCoroutine(FadeBackgroundRoutine(newSprite));
        }

        private void PlaySFXSound(string soundName)
        {
            if (AudioManager.Instance == null) return;

            AudioClip clip = Resources.Load<AudioClip>($"Sounds/{soundName}");
            if (clip != null)
            {
                AudioManager.Instance.PlayStorySFX(clip);
            }
        }

        private void SafeStopCoroutine(ref Coroutine coroutine)
        {
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
                coroutine = null;
            }
        }
        #endregion

        #region Coroutines
        private IEnumerator AutoNextRoutine()
        {
            yield return new WaitForSeconds(autoDelayAfterType);
            OnNextButtonClicked?.Invoke();
        }

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

        private IEnumerator FadeBackgroundRoutine(Sprite newSprite)
        {
            float halfDuration = fadeDuration * 0.5f;
            float elapsed = 0f;
            Color color = backgroundImage.color;

            while (elapsed < halfDuration)
            {
                elapsed += Time.deltaTime;
                color.a = Mathf.Lerp(1f, 0f, elapsed / halfDuration);
                backgroundImage.color = color;
                yield return null;
            }

            backgroundImage.sprite = newSprite;

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
    }
}