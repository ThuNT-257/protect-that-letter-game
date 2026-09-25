using ProtectThatLetter.Definitions;
using ProtectThatLetter.Managers;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace ProtectThatLetter.Controllers {
    /// <summary>
    /// Manages the Letter scene: envelope opening, page navigation,
    /// countdown timer, image download, and localized content.
    /// </summary>
    public class LetterController : MonoBehaviour {
        #region Serialized Fields
        [Header("Envelope Settings")]
        [SerializeField] private Button envelopButton;                 // Button to open the envelope
        [SerializeField] private CanvasGroup envelopCanvasGroup;       // Envelope visual
        [SerializeField] private CanvasGroup guideTextCanvasGroup;     // Guide text ("Tap to open")

        [Header("Global UI Elements (Shown after opening envelope)")]
        [SerializeField] private Button settingsButton;                // Settings button (revealed after opening)
        [SerializeField] private CanvasGroup globalUICanvasGroup;      // Global UI container

        [Header("Pages Setup")]
        [SerializeField] private GameObject page1;                     // First letter page
        [SerializeField] private GameObject page2;                     // Second letter page
        [SerializeField] private CanvasGroup pagesCanvasGroup;         // Container for both pages

        [Header("Page 1 Content Elements")]
        [SerializeField] private TextMeshProUGUI page1LetterText;      // Letter body text
        [SerializeField] private Image page1Image;                     // Letter image (from URL)

        [Header("Page 2 Content Elements")]
        [SerializeField] private TextMeshProUGUI invitedNameText;      // Invited guest's name

        [Header("Page Navigation Buttons")]
        [SerializeField] private Button nextPageButton;                // Next page button
        [SerializeField] private Button prevPageButton;                // Previous page button

        [Header("Countdown Settings")]
        [SerializeField] private TextMeshProUGUI countdownText;        // Countdown timer display

        [Header("Save Notification UI")]
        [SerializeField] private CanvasGroup savedTextCanvasGroup;     // "Saved!" notification
        [SerializeField] private float textDisplayDuration = 3f;       // How long the notification stays

        [Header("Animation Settings")]
        [SerializeField] private float fadeDuration = 0.3f;            // Fade duration for transitions
        #endregion

        #region Data DTOs
        /// <summary>
        /// Request body for the mark-read API.
        /// </summary>
        [Serializable]
        private class MarkReadRequestData {
            public string code;
        }
        #endregion

        #region Private Fields
        private bool isTransitioning = false;         // Prevents overlapping transitions
        private DateTime targetTimeUtc;                // Target time for countdown (UTC)

        private Coroutine countdownCoroutine;          // Countdown timer coroutine
        private Coroutine imageDownloadCoroutine;      // Image download coroutine
        private Coroutine savedTextFadeCoroutine;      // "Saved!" notification coroutine
        private Coroutine pageTransitionCoroutine;     // Page transition coroutine
        private Coroutine openEnvelopeCoroutine;       // Envelope opening coroutine
        #endregion

        #region Lifecycle
        /// <summary>
        /// Initializes the saved notification as invisible
        /// </summary>
        private void Awake() {
            if (savedTextCanvasGroup != null) {
                SetCanvasGroupState(savedTextCanvasGroup, alpha: 0f, interactable: false, active: true);
            }
        }

        /// <summary>
        /// Sets up button listeners, countdown target, and initial state
        /// </summary>
        private void Start() {
            // Register button listeners
            if (envelopButton != null) envelopButton.onClick.AddListener(OpenEnvelope);
            if (nextPageButton != null) nextPageButton.onClick.AddListener(SwitchToPage2);
            if (prevPageButton != null) prevPageButton.onClick.AddListener(SwitchToPage1);

            // Set target time (Sept 15, 2026 at 18:00 UTC+7)
            DateTimeOffset targetTimeOffset = new DateTimeOffset(2026, 9, 15, 18, 0, 0, TimeSpan.FromHours(7));
            targetTimeUtc = targetTimeOffset.UtcDateTime;

            StartCountdown();
            Initialize();
        }

        /// <summary>
        /// Subscribes to language change events when enabled
        /// </summary>
        private void OnEnable() {
            LocalizationManager.OnLanguageChanged += HandleLanguageChanged;
        }

        /// <summary>
        /// Unsubscribes from events and stops coroutines when disabled
        /// </summary>
        private void OnDisable() {
            LocalizationManager.OnLanguageChanged -= HandleLanguageChanged;
            StopAllRunningCoroutines();
        }

        /// <summary>
        /// Cleans up listeners and destroys runtime-created textures
        /// </summary>
        private void OnDestroy() {
            if (envelopButton != null) envelopButton.onClick.RemoveListener(OpenEnvelope);
            if (nextPageButton != null) nextPageButton.onClick.RemoveListener(SwitchToPage2);
            if (prevPageButton != null) prevPageButton.onClick.RemoveListener(SwitchToPage1);

            // Clean up runtime-created sprite/texture to prevent memory leaks
            if (page1Image != null && page1Image.sprite != null) {
                Destroy(page1Image.sprite.texture);
                Destroy(page1Image.sprite);
            }
        }
        #endregion

        #region Event Handlers
        /// <summary>
        /// Refreshes localized content when the language changes
        /// </summary>
        private void HandleLanguageChanged(string languageCode) {
            Debug.Log($"[LetterController] Language changed event triggered: {languageCode}");
            StartCoroutine(RefreshLocalizedContentRoutine());
        }

        /// <summary>
        /// Waits one frame, then refreshes localized text content
        /// </summary>
        private IEnumerator RefreshLocalizedContentRoutine() {
            yield return null;
            UpdateLetterText();
            UpdateInvitedNameText();
        }
        #endregion

        #region Countdown Logic
        /// <summary>
        /// Starts the countdown coroutine
        /// </summary>
        private void StartCountdown() {
            StopCountdown();
            if (countdownText != null) {
                countdownCoroutine = StartCoroutine(UpdateCountdownRoutine());
            } else {
                Debug.LogError("[LetterController] countdownText TextMeshProUGUI field is MISSING in Inspector!");
            }
        }

        /// <summary>
        /// Stops the countdown coroutine
        /// </summary>
        private void StopCountdown() {
            if (countdownCoroutine != null) {
                StopCoroutine(countdownCoroutine);
                countdownCoroutine = null;
            }
        }

        /// <summary>
        /// Updates the countdown every second (unscaled time)
        /// </summary>
        private IEnumerator UpdateCountdownRoutine() {
            var wait = new WaitForSecondsRealtime(1f);

            while (true) {
                TimeSpan remainingTime = targetTimeUtc - DateTime.UtcNow;

                // Countdown finished
                if (remainingTime.TotalSeconds <= 0) {
                    Debug.LogWarning($"[LetterController] Target time {targetTimeUtc} UTC has passed.");
                    if (countdownText != null) {
                        countdownText.text = "00:00:00:00s";
                    }
                    yield break;
                }

                // Format as DD:HH:MM:SS
                if (countdownText != null) {
                    countdownText.text = string.Format("{0:D2}:{1:D2}:{2:D2}:{3:D2}s",
                        remainingTime.Days,
                        remainingTime.Hours,
                        remainingTime.Minutes,
                        remainingTime.Seconds);
                }

                yield return wait;
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Shows the "Saved!" notification briefly
        /// </summary>
        public void ShowSavedNotification() {
            if (savedTextCanvasGroup == null) {
                Debug.LogWarning("[LetterController] savedTextCanvasGroup Reference is MISSING in Inspector!");
                return;
            }

            if (savedTextFadeCoroutine != null) {
                StopCoroutine(savedTextFadeCoroutine);
            }

            savedTextFadeCoroutine = StartCoroutine(ShowAndHideSavedTextRoutine());
        }

        /// <summary>
        /// Initializes the letter state and loads page data
        /// </summary>
        public void Initialize() {
            InitState();
            LoadPageData();
        }

        /// <summary>
        /// Loads letter content and image from GuestDataManager
        /// </summary>
        public void LoadPageData() {
            if (GuestDataManager.Instance == null) {
                Debug.LogError("[LetterController] GuestDataManager.Instance is NULL! Cannot load Letter Data.");
                return;
            }

            Debug.Log($"[LetterController] Loading Page Data... | Text VN length: {GuestDataManager.Instance.LetterContentVn?.Length ?? 0} | Text EN length: {GuestDataManager.Instance.LetterContentEn?.Length ?? 0}");

            UpdateLetterText();
            UpdateInvitedNameText();

            // Download image if URL is available
            string imageUrl = GuestDataManager.Instance.ImageUrl;
            if (!string.IsNullOrEmpty(imageUrl)) {
                if (page1Image == null) {
                    Debug.LogError("[LetterController] page1Image Reference is MISSING in Inspector!");
                } else {
                    if (imageDownloadCoroutine != null) {
                        StopCoroutine(imageDownloadCoroutine);
                    }
                    imageDownloadCoroutine = StartCoroutine(LoadImageFromUrlRoutine(imageUrl));
                }
            } else {
                Debug.LogWarning("[LetterController] GuestDataManager ImageUrl is NULL or EMPTY.");
            }
        }
        #endregion

        #region Private Methods - UI Updates
        /// <summary>
        /// Updates the letter body text based on current language
        /// </summary>
        private void UpdateLetterText() {
            if (GuestDataManager.Instance == null || page1LetterText == null) return;

            string currentLang = LocalizationManager.Instance != null
                ? LocalizationManager.Instance.CurrentLanguageCode
                : GameDefinitions.Languages.VIETNAMESE;

            string content = currentLang.Equals(GameDefinitions.Languages.ENGLISH, StringComparison.OrdinalIgnoreCase)
                ? GuestDataManager.Instance.LetterContentEn
                : GuestDataManager.Instance.LetterContentVn;

            page1LetterText.text = content ?? string.Empty;
        }

        /// <summary>
        /// Updates the invited guest's name
        /// </summary>
        private void UpdateInvitedNameText() {
            if (GuestDataManager.Instance == null || invitedNameText == null) return;

            string nickname = GuestDataManager.Instance.GuestNickname;
            invitedNameText.text = !string.IsNullOrEmpty(nickname) ? nickname : string.Empty;
        }

        /// <summary>
        /// Downloads an image from a URL and applies it to page1Image
        /// </summary>
        private IEnumerator LoadImageFromUrlRoutine(string url) {
            Debug.Log($"[LetterController] Downloading image from URL: {url}");
            using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(url)) {
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success) {
                    Texture2D texture = DownloadHandlerTexture.GetContent(request);
                    if (texture != null && page1Image != null) {
                        // Clean up previous sprite/texture to prevent memory leaks
                        if (page1Image.sprite != null) {
                            Destroy(page1Image.sprite.texture);
                            Destroy(page1Image.sprite);
                        }

                        Sprite newSprite = Sprite.Create(
                            texture,
                            new Rect(0, 0, texture.width, texture.height),
                            new Vector2(0.5f, 0.5f)
                        );
                        page1Image.sprite = newSprite;
                    }
                } else {
                    Debug.LogError($"[LetterController] Failed to load image from URL: {url} | Error: {request.error}");
                }
            }
        }

        /// <summary>
        /// Initializes the letter state based on whether it's been read before
        /// </summary>
        private void InitState() {
            bool isLetterRead = GuestDataManager.Instance != null && GuestDataManager.Instance.IsLetterRead;

            SetCanvasGroupState(savedTextCanvasGroup, alpha: 0f, interactable: false, active: true);

            if (settingsButton != null) {
                settingsButton.gameObject.SetActive(true);
            }

            // Ensure both pages are active but one is on top
            if (page1 != null) page1.SetActive(true);
            if (page2 != null) page2.SetActive(true);
            if (page1 != null) page1.transform.SetAsLastSibling();

            UpdatePageNavigationButtons(targetPage: page1);

            if (isLetterRead) {
                // Letter was read: skip envelope, show pages directly
                SetCanvasGroupState(envelopCanvasGroup, alpha: 0f, interactable: false, active: false);
                SetCanvasGroupState(guideTextCanvasGroup, alpha: 0f, interactable: false, active: false);

                SetCanvasGroupState(pagesCanvasGroup, alpha: 1f, interactable: true, active: true);
                SetCanvasGroupState(globalUICanvasGroup, alpha: 1f, interactable: true, active: true);
            } else {
                // First time: show envelope
                SetCanvasGroupState(envelopCanvasGroup, alpha: 1f, interactable: true, active: true);
                SetCanvasGroupState(guideTextCanvasGroup, alpha: 1f, interactable: true, active: true);

                SetCanvasGroupState(pagesCanvasGroup, alpha: 0f, interactable: false, active: true);
                SetCanvasGroupState(globalUICanvasGroup, alpha: 0f, interactable: false, active: true);
            }
        }

        /// <summary>
        /// Handles envelope button click: opens the envelope
        /// </summary>
        private void OpenEnvelope() {
            if (isTransitioning) return;
            if (openEnvelopeCoroutine != null) StopCoroutine(openEnvelopeCoroutine);
            openEnvelopeCoroutine = StartCoroutine(OpenEnvelopeRoutine());
        }

        /// <summary>
        /// Switches to page 1 with a fade transition
        /// </summary>
        private void SwitchToPage1() {
            if (isTransitioning) return;
            if (pageTransitionCoroutine != null) StopCoroutine(pageTransitionCoroutine);
            pageTransitionCoroutine = StartCoroutine(SwitchPageRoutine(page1));
        }

        /// <summary>
        /// Switches to page 2 with a fade transition
        /// </summary>
        private void SwitchToPage2() {
            if (isTransitioning) return;
            if (pageTransitionCoroutine != null) StopCoroutine(pageTransitionCoroutine);
            pageTransitionCoroutine = StartCoroutine(SwitchPageRoutine(page2));
        }

        /// <summary>
        /// Coroutine that opens the envelope and reveals the letter pages
        /// </summary>
        private IEnumerator OpenEnvelopeRoutine() {
            isTransitioning = true;

            // Mark letter as read
            if (GuestDataManager.Instance != null) {
                GuestDataManager.Instance.SetLetterRead(true);
            }

            // Fade out envelope, fade in pages
            float timer = 0f;
            while (timer < fadeDuration) {
                timer += Time.unscaledDeltaTime;
                float progress = Mathf.Clamp01(timer / fadeDuration);

                if (envelopCanvasGroup != null) envelopCanvasGroup.alpha = 1f - progress;
                if (guideTextCanvasGroup != null) guideTextCanvasGroup.alpha = 1f - progress;

                if (pagesCanvasGroup != null) pagesCanvasGroup.alpha = progress;
                if (globalUICanvasGroup != null) globalUICanvasGroup.alpha = progress;

                yield return null;
            }

            // Ensure final state
            SetCanvasGroupState(envelopCanvasGroup, alpha: 0f, interactable: false, active: false);
            SetCanvasGroupState(guideTextCanvasGroup, alpha: 0f, interactable: false, active: false);

            SetCanvasGroupState(pagesCanvasGroup, alpha: 1f, interactable: true, active: true);
            SetCanvasGroupState(globalUICanvasGroup, alpha: 1f, interactable: true, active: true);

            isTransitioning = false;
            SendMarkReadApi();
        }

        /// <summary>
        /// Sends a "mark as read" API request to the server
        /// </summary>
        private void SendMarkReadApi() {
            string accessCode = GuestDataManager.Instance != null
                ? GuestDataManager.Instance.AccessCode
                : string.Empty;

            if (string.IsNullOrEmpty(accessCode)) {
                accessCode = PlayerPrefs.GetString("SavedAccessCode", string.Empty);
            }
        }

        /// <summary>
        /// Coroutine that fades out, swaps page, then fades in
        /// </summary>
        private IEnumerator SwitchPageRoutine(GameObject targetPage) {
            isTransitioning = true;
            float timer = 0f;

            // Fade out
            while (timer < fadeDuration) {
                timer += Time.unscaledDeltaTime;
                if (pagesCanvasGroup != null) {
                    pagesCanvasGroup.alpha = 1f - (timer / fadeDuration);
                }
                yield return null;
            }

            if (pagesCanvasGroup != null) pagesCanvasGroup.alpha = 0f;

            // Swap page
            if (targetPage != null) {
                targetPage.transform.SetAsLastSibling();
                UpdatePageNavigationButtons(targetPage);
            }

            // Fade in
            timer = 0f;
            while (timer < fadeDuration) {
                timer += Time.unscaledDeltaTime;
                if (pagesCanvasGroup != null) {
                    pagesCanvasGroup.alpha = timer / fadeDuration;
                }
                yield return null;
            }

            SetCanvasGroupState(pagesCanvasGroup, alpha: 1f, interactable: true, active: true);
            isTransitioning = false;
        }

        /// <summary>
        /// Coroutine that shows then hides the "Saved!" notification
        /// </summary>
        private IEnumerator ShowAndHideSavedTextRoutine() {
            if (savedTextCanvasGroup == null) yield break;

            SetCanvasGroupState(savedTextCanvasGroup, alpha: 0f, interactable: false, active: true);

            // Fade in
            float timer = 0f;
            while (timer < fadeDuration) {
                timer += Time.unscaledDeltaTime;
                savedTextCanvasGroup.alpha = Mathf.Clamp01(timer / fadeDuration);
                yield return null;
            }
            savedTextCanvasGroup.alpha = 1f;

            // Hold
            yield return new WaitForSecondsRealtime(textDisplayDuration);

            // Fade out
            timer = 0f;
            while (timer < fadeDuration) {
                timer += Time.unscaledDeltaTime;
                savedTextCanvasGroup.alpha = Mathf.Clamp01(1f - (timer / fadeDuration));
                yield return null;
            }

            SetCanvasGroupState(savedTextCanvasGroup, alpha: 0f, interactable: false, active: true);
            savedTextFadeCoroutine = null;
        }

        /// <summary>
        /// Shows/hides navigation buttons based on the current page
        /// </summary>
        private void UpdatePageNavigationButtons(GameObject targetPage) {
            bool isPage1 = (targetPage == page1);

            if (nextPageButton != null) nextPageButton.gameObject.SetActive(isPage1);
            if (prevPageButton != null) prevPageButton.gameObject.SetActive(!isPage1);
        }

        /// <summary>
        /// Helper to set all CanvasGroup state in one call
        /// </summary>
        private void SetCanvasGroupState(CanvasGroup cg, float alpha, bool interactable, bool active) {
            if (cg == null) return;
            cg.alpha = alpha;
            cg.blocksRaycasts = interactable;
            cg.interactable = interactable;
            cg.gameObject.SetActive(active);
        }

        /// <summary>
        /// Stops all running coroutines to prevent conflicts
        /// </summary>
        private void StopAllRunningCoroutines() {
            StopCountdown();
            if (imageDownloadCoroutine != null) StopCoroutine(imageDownloadCoroutine);
            if (savedTextFadeCoroutine != null) StopCoroutine(savedTextFadeCoroutine);
            if (pageTransitionCoroutine != null) StopCoroutine(pageTransitionCoroutine);
            if (openEnvelopeCoroutine != null) StopCoroutine(openEnvelopeCoroutine);
        }
        #endregion
    }
}