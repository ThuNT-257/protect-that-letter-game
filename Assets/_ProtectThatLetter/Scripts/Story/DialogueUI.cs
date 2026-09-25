using ProtectThatLetter.Managers;
using ProtectThatLetter.UI;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueUI : MonoBehaviour {
    #region Serialized Fields
    [Header("Overlay Controls")]
    [SerializeField] private CanvasGroup overlayCanvasGroup;
    [SerializeField] private float fadeDuration = 0.4f;

    [Header("Background Elements")]
    [SerializeField] private Image backgroundImage;

    [Header("Left Character Elements")]
    [SerializeField] private Image leftNameFrame;
    [SerializeField] private TMP_Text leftNameText;
    [SerializeField] private Image leftAvatarImage;

    [Header("Right Character Elements")]
    [SerializeField] private Image rightNameFrame;
    [SerializeField] private TMP_Text rightNameText;
    [SerializeField] private Image rightAvatarImage;

    [Header("Dialogue Elements")]
    [SerializeField] private TMP_Text dialogueLineText;
    [SerializeField] private Button skipButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button autoButton;

    [Header("Typewriter Settings")]
    [SerializeField] private float typingSpeed = 0.03f;

    [Header("Auto Play Settings")]
    [SerializeField] private float autoDelayAfterType = 2.0f;
    [SerializeField] private TMP_Text autoButtonText; 

    [Header("Auto Button Visuals")]
    [SerializeField] private float activeAlpha = 1.0f;    
    [SerializeField] private float inactiveAlpha = 0.5f;  
    #endregion

    #region Events
    public static event Action OnNextButtonClicked;
    #endregion

    #region Private Fields
    private DialogueResponsiveLayout responsiveLayout;
    private Coroutine overlayFadeCoroutine;
    private Coroutine backgroundFadeCoroutine;
    private Coroutine typewriterCoroutine;
    private Coroutine autoNextCoroutine;

    private bool isTyping = false;
    private bool isAutoMode = false;
    private string currentFullText = "";
    private Image autoButtonImage;
    #endregion

    #region Lifecycle
    private void Awake() {
        responsiveLayout = GetComponent<DialogueResponsiveLayout>();

        if (skipButton != null) {
            skipButton.onClick.RemoveAllListeners();
            skipButton.onClick.AddListener(HandleNextClick);
        }

        if (nextButton != null) {
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(HandleNextClick);
        }

        if (autoButton != null) {
            autoButton.onClick.RemoveAllListeners();
            autoButton.onClick.AddListener(ToggleAutoMode);

            autoButtonImage = autoButton.GetComponent<Image>();
            autoButtonText = autoButton.GetComponentInChildren<TMP_Text>();

            UpdateAutoButtonVisual();

            HideDialogueOverlay(immediate: true);
        }
    }
    #endregion

    #region Public Methods
    public void DisplayLine(DialogueLine line, bool isFirstLine = false) {
        if(AudioManager.Instance != null) {
            AudioManager.Instance.PlayStorySFX("clack");
        }

        if (line == null) return;

        StopAutoNextCoroutine();

        ShowDialogueOverlay();

        if (!string.IsNullOrEmpty(line.background) && backgroundImage != null) {
            SetBackgroundWithFade(line.background);
        }

        if (!string.IsNullOrEmpty(line.sound)) PlaySFXSound(line.sound);

        if (dialogueLineText != null) {
            StartTypewriterEffect(line.content);
        }

        bool isLeft = string.IsNullOrEmpty(line.position) || line.position.ToLower() == "left";

        if (isLeft) {
            SetCharacterUI(leftNameFrame, leftNameText, leftAvatarImage, line, isFirstLine);
            SetInactiveUI(rightNameFrame, rightAvatarImage, isFirstLine);
        } else {
            SetCharacterUI(rightNameFrame, rightNameText, rightAvatarImage, line, isFirstLine);
            SetInactiveUI(leftNameFrame, leftAvatarImage, isFirstLine);
        }

        if (responsiveLayout != null) {
            responsiveLayout.SetSpeaker(isLeft);
        }
    }

    public void HideDialogueOverlay(bool immediate = false) {
        StopAutoNextCoroutine();

        if (responsiveLayout != null) responsiveLayout.SetOverlayHidden(true);

        if (overlayCanvasGroup == null) return;

        overlayCanvasGroup.interactable = false;
        overlayCanvasGroup.blocksRaycasts = false;

        if (overlayFadeCoroutine != null) StopCoroutine(overlayFadeCoroutine);

        if (immediate) {
            overlayCanvasGroup.alpha = 0f;
        } else {
            overlayFadeCoroutine = StartCoroutine(FadeCanvasGroupRoutine(overlayCanvasGroup, overlayCanvasGroup.alpha, 0f, fadeDuration, null));
        }
    }

    public void ShowDialogueOverlay(bool immediate = false) {
        if (responsiveLayout != null) responsiveLayout.SetOverlayHidden(false);

        if (overlayCanvasGroup == null) return;

        if (overlayFadeCoroutine != null) StopCoroutine(overlayFadeCoroutine);

        if (immediate) {
            overlayCanvasGroup.alpha = 1f;
            overlayCanvasGroup.interactable = true;
            overlayCanvasGroup.blocksRaycasts = true;
        } else {
            overlayFadeCoroutine = StartCoroutine(FadeCanvasGroupRoutine(overlayCanvasGroup, overlayCanvasGroup.alpha, 1f, fadeDuration, () => {
                overlayCanvasGroup.interactable = true;
                overlayCanvasGroup.blocksRaycasts = true;
            }));
        }
    }

    public void PlayIntroMedia(string backgroundName, string soundName) {
        if (!string.IsNullOrEmpty(backgroundName) && backgroundImage != null) {
            SetBackgroundWithFade(backgroundName);
        }
        if (!string.IsNullOrEmpty(soundName)) PlaySFXSound(soundName);
    }
    #endregion

    #region Auto & Click Handlers
    private void ToggleAutoMode() {
        isAutoMode = !isAutoMode;
        UpdateAutoButtonVisual();

        if (isAutoMode) {
            if (!isTyping) {
                StartAutoNextCoroutine();
            }
        } else {
            StopAutoNextCoroutine();
        }
    }

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

    private void HandleNextClick() {
        if (isTyping) {
            CompleteTypewriterImmediately();
        } else {
            StopAutoNextCoroutine();
            OnNextButtonClicked?.Invoke();
        }
    }

    private void StartTypewriterEffect(string content) {
        if (typewriterCoroutine != null) StopCoroutine(typewriterCoroutine);

        currentFullText = content ?? "";
        dialogueLineText.text = currentFullText;

        typewriterCoroutine = StartCoroutine(TypewriterRoutine());
    }

    private IEnumerator TypewriterRoutine() {
        isTyping = true;
        dialogueLineText.ForceMeshUpdate();

        int totalVisibleCharacters = dialogueLineText.textInfo.characterCount;
        dialogueLineText.maxVisibleCharacters = 0;

        for (int visibleCount = 1; visibleCount <= totalVisibleCharacters; visibleCount++) {
            dialogueLineText.maxVisibleCharacters = visibleCount;
            yield return new WaitForSeconds(typingSpeed);
        }

        OnTypewriterCompleted();
    }

    private void CompleteTypewriterImmediately() {
        if (typewriterCoroutine != null) StopCoroutine(typewriterCoroutine);

        dialogueLineText.maxVisibleCharacters = dialogueLineText.textInfo.characterCount;
        OnTypewriterCompleted();
    }

    private void OnTypewriterCompleted() {
        isTyping = false;

        if (isAutoMode) {
            StartAutoNextCoroutine();
        }
    }

    private void StartAutoNextCoroutine() {
        StopAutoNextCoroutine();
        autoNextCoroutine = StartCoroutine(AutoNextRoutine());
    }

    private void StopAutoNextCoroutine() {
        if (autoNextCoroutine != null) {
            StopCoroutine(autoNextCoroutine);
            autoNextCoroutine = null;
        }
    }

    private IEnumerator AutoNextRoutine() {
        yield return new WaitForSeconds(autoDelayAfterType);
        OnNextButtonClicked?.Invoke();
    }
    #endregion

    #region Private Methods
    private void SetBackgroundWithFade(string bgName) {
        Sprite newSprite = Resources.Load<Sprite>($"Backgrounds/{bgName}");
        if (newSprite == null) {
            Debug.LogError($"[DialogueUI] Background failed to load at path: Backgrounds/{bgName}");
            return;
        }
        if (backgroundImage == null) return;
        if (backgroundImage.sprite == newSprite && backgroundImage.color.a > 0.9f) return;

        if (backgroundFadeCoroutine != null) StopCoroutine(backgroundFadeCoroutine);
        backgroundFadeCoroutine = StartCoroutine(FadeBackgroundRoutine(newSprite));
    }

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

    private IEnumerator FadeBackgroundRoutine(Sprite newSprite) {
        float halfDuration = fadeDuration * 0.5f;

        float elapsed = 0f;
        Color color = backgroundImage.color;
        while (elapsed < halfDuration) {
            elapsed += Time.deltaTime;
            color.a = Mathf.Lerp(1f, 0f, elapsed / halfDuration);
            backgroundImage.color = color;
            yield return null;
        }

        backgroundImage.sprite = newSprite;

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

    private void PlaySFXSound(string soundName) {
        if (AudioManager.Instance == null) return;

        AudioClip clip = Resources.Load<AudioClip>($"Sounds/{soundName}");
        if (clip != null) {
            AudioManager.Instance.PlayStorySFX(clip);
        }
    }

    private void SetCharacterUI(Image nameFrame, TMP_Text nameText, Image avatarImage, DialogueLine line, bool isFirstLine) {
        if (nameFrame != null) nameFrame.gameObject.SetActive(true);
        if (nameText != null) nameText.text = line.speaker;

        bool isLandscape = responsiveLayout != null && responsiveLayout.IsLandscape;
        bool isLeft = string.IsNullOrEmpty(line.position) || line.position.ToLower() == "left";

        if (avatarImage != null) {
            if (!string.IsNullOrEmpty(line.avatar)) {
                Sprite avatarSprite = Resources.Load<Sprite>($"Avatars/{line.avatar}");
                if (avatarSprite != null) {
                    avatarImage.sprite = avatarSprite;
                }

                if (!isLandscape) {
                    avatarImage.gameObject.SetActive(true);
                }
            } else if (!isLandscape) {
                avatarImage.gameObject.SetActive(false);
            }
        }

        if (isLandscape) {
            responsiveLayout.AnimateLandscapeAvatar(isLeft, isSpeaking: true, isFirstLine: isFirstLine);
        }
    }

    private void SetInactiveUI(Image nameFrame, Image avatarImage, bool isFirstLine) {
        if (nameFrame != null) nameFrame.gameObject.SetActive(false);

        if (avatarImage != null) {
            bool isLandscape = responsiveLayout != null && responsiveLayout.IsLandscape;
            bool isLeftAvatar = (avatarImage == leftAvatarImage);

            if (!isLandscape) {
                avatarImage.gameObject.SetActive(false);
            } else {
                responsiveLayout.AnimateLandscapeAvatar(isLeftAvatar, isSpeaking: false, isFirstLine: isFirstLine);
            }
        }
    }
    #endregion
}