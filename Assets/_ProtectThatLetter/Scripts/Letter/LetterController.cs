using ProtectThatLetter.Managers;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace ProtectThatLetter.Controllers {
    public class LetterController : MonoBehaviour {
        #region Serialized Fields
        [Header("Envelope Settings")]
        [SerializeField] private Button envelopButton;
        [SerializeField] private CanvasGroup envelopCanvasGroup;
        [SerializeField] private CanvasGroup guideTextCanvasGroup;

        [Header("Global UI Elements (Shown after opening envelope)")]
        [SerializeField] private Button settingsButton;
        [SerializeField] private CanvasGroup globalUICanvasGroup;

        [Header("Pages Setup")]
        [SerializeField] private GameObject page1;
        [SerializeField] private GameObject page2;
        [SerializeField] private CanvasGroup pagesCanvasGroup;

        [Header("Page 1 Content Elements")]
        [SerializeField] private TextMeshProUGUI page1LetterText;
        [SerializeField] private Image page1Image;

        [Header("Page 2 Content Elements")]
        [SerializeField] private TextMeshProUGUI invitedNameText;

        [Header("Page Navigation Buttons")]
        [SerializeField] private Button nextPageButton;
        [SerializeField] private Button prevPageButton;

        [Header("Countdown Settings")]
        [SerializeField] private TextMeshProUGUI countdownText;

        [Header("Save Notification UI")]
        [SerializeField] private CanvasGroup savedTextCanvasGroup;
        [SerializeField] private float textDisplayDuration = 3f;

        [Header("Animation Settings")]
        [SerializeField] private float fadeDuration = 0.3f;
        #endregion

        #region Data DTOs
        [Serializable]
        private class MarkReadRequestData {
            public string code;
        }
        #endregion

        #region Private Fields
        private bool isTransitioning = false;
        private DateTime targetTimeUtc;

        private Coroutine countdownCoroutine;
        private Coroutine imageDownloadCoroutine;
        private Coroutine savedTextFadeCoroutine;
        private Coroutine pageTransitionCoroutine;
        private Coroutine openEnvelopeCoroutine;
        #endregion

        #region Lifecycle
        private void Awake() {
            if (savedTextCanvasGroup != null) {
                SetCanvasGroupState(savedTextCanvasGroup, alpha: 0f, interactable: false, active: true);
            }
        }

        private void Start() {
            if (envelopButton != null) {
                envelopButton.onClick.AddListener(OpenEnvelope);
            }

            if (nextPageButton != null) nextPageButton.onClick.AddListener(SwitchToPage2);
            if (prevPageButton != null) prevPageButton.onClick.AddListener(SwitchToPage1);

            DateTimeOffset targetTimeOffset = new DateTimeOffset(2026, 9, 15, 18, 0, 0, TimeSpan.FromHours(7));
            targetTimeUtc = targetTimeOffset.UtcDateTime;

            StartCountdown();
            Initialize();
        }

        private void OnEnable() {
            LocalizationManager.OnLanguageChanged += HandleLanguageChanged;
        }

        private void OnDisable() {
            LocalizationManager.OnLanguageChanged -= HandleLanguageChanged;
            StopAllRunningCoroutines();
        }

        private void OnDestroy() {
            if (envelopButton != null) {
                envelopButton.onClick.RemoveListener(OpenEnvelope);
            }

            if (nextPageButton != null) nextPageButton.onClick.RemoveListener(SwitchToPage2);
            if (prevPageButton != null) prevPageButton.onClick.RemoveListener(SwitchToPage1);

            if (page1Image != null && page1Image.sprite != null) {
                Destroy(page1Image.sprite.texture);
                Destroy(page1Image.sprite);
            }
        }
        #endregion

        #region Event Handlers
        private void HandleLanguageChanged(string languageCode) {
            Debug.Log($"[LetterController] Language changed event triggered: {languageCode}");

            StartCoroutine(RefreshLocalizedContentRoutine());
        }

        private IEnumerator RefreshLocalizedContentRoutine() {
            yield return null;
            UpdateLetterText();
            UpdateInvitedNameText();
        }
        #endregion

        #region Countdown Logic
        private void StartCountdown() {
            StopCountdown();
            if (countdownText != null) {
                countdownCoroutine = StartCoroutine(UpdateCountdownRoutine());
            } else {
                Debug.LogError("[LetterController] countdownText TextMeshProUGUI field is MISSING in Inspector!");
            }
        }

        private void StopCountdown() {
            if (countdownCoroutine != null) {
                StopCoroutine(countdownCoroutine);
                countdownCoroutine = null;
            }
        }

        private IEnumerator UpdateCountdownRoutine() {
            var wait = new WaitForSecondsRealtime(1f);

            while (true) {
                TimeSpan remainingTime = targetTimeUtc - DateTime.UtcNow;

                if (remainingTime.TotalSeconds <= 0) {
                    Debug.LogWarning($"[LetterController] Target time {targetTimeUtc} UTC has passed.");
                    if (countdownText != null) {
                        countdownText.text = "00:00:00:00s";
                    }
                    yield break;
                }

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

        public void Initialize() {
            InitState();
            LoadPageData();
        }

        public void LoadPageData() {
            if (GuestDataManager.Instance == null) {
                Debug.LogError("[LetterController] GuestDataManager.Instance is NULL! Cannot load Letter Data.");
                return;
            }

            Debug.Log($"[LetterController] Loading Page Data... | Text VN length: {GuestDataManager.Instance.LetterContentVn?.Length ?? 0} | Text EN length: {GuestDataManager.Instance.LetterContentEn?.Length ?? 0}");

            UpdateLetterText();
            UpdateInvitedNameText();

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
        private void UpdateLetterText() {
            if (GuestDataManager.Instance == null || page1LetterText == null) return;

            string currentLang = LocalizationManager.Instance != null
                ? LocalizationManager.Instance.CurrentLanguageCode
                : LocalizationManager.VIETNAMESE;

            string content = currentLang.Equals(LocalizationManager.ENGLISH, StringComparison.OrdinalIgnoreCase)
                ? GuestDataManager.Instance.LetterContentEn
                : GuestDataManager.Instance.LetterContentVn;

            page1LetterText.text = content ?? string.Empty;
        }

        private void UpdateInvitedNameText() {
            if (GuestDataManager.Instance == null || invitedNameText == null) return;

            string nickname = GuestDataManager.Instance.GuestNickname;
            invitedNameText.text = !string.IsNullOrEmpty(nickname) ? nickname : string.Empty;
        }

        private IEnumerator LoadImageFromUrlRoutine(string url) {
            Debug.Log($"[LetterController] Downloading image from URL: {url}");
            using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(url)) {
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success) {
                    Texture2D texture = DownloadHandlerTexture.GetContent(request);
                    if (texture != null && page1Image != null) {
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

        private void InitState() {
            bool isLetterRead = GuestDataManager.Instance != null && GuestDataManager.Instance.IsLetterRead;

            SetCanvasGroupState(savedTextCanvasGroup, alpha: 0f, interactable: false, active: true);

            if (settingsButton != null) {
                settingsButton.gameObject.SetActive(true);
            }

            if (page1 != null) page1.SetActive(true);
            if (page2 != null) page2.SetActive(true);
            if (page1 != null) page1.transform.SetAsLastSibling();

            UpdatePageNavigationButtons(targetPage: page1);

            if (isLetterRead) {
                SetCanvasGroupState(envelopCanvasGroup, alpha: 0f, interactable: false, active: false);
                SetCanvasGroupState(guideTextCanvasGroup, alpha: 0f, interactable: false, active: false);

                SetCanvasGroupState(pagesCanvasGroup, alpha: 1f, interactable: true, active: true);
                SetCanvasGroupState(globalUICanvasGroup, alpha: 1f, interactable: true, active: true);
            } else {
                SetCanvasGroupState(envelopCanvasGroup, alpha: 1f, interactable: true, active: true);
                SetCanvasGroupState(guideTextCanvasGroup, alpha: 1f, interactable: true, active: true);

                SetCanvasGroupState(pagesCanvasGroup, alpha: 0f, interactable: false, active: true);
                SetCanvasGroupState(globalUICanvasGroup, alpha: 0f, interactable: false, active: true);
            }
        }

        private void OpenEnvelope() {
            if (isTransitioning) return;
            if (openEnvelopeCoroutine != null) StopCoroutine(openEnvelopeCoroutine);
            openEnvelopeCoroutine = StartCoroutine(OpenEnvelopeRoutine());
        }

        private void SwitchToPage1() {
            if (isTransitioning) return;
            if (pageTransitionCoroutine != null) StopCoroutine(pageTransitionCoroutine);
            pageTransitionCoroutine = StartCoroutine(SwitchPageRoutine(page1));
        }

        private void SwitchToPage2() {
            if (isTransitioning) return;
            if (pageTransitionCoroutine != null) StopCoroutine(pageTransitionCoroutine);
            pageTransitionCoroutine = StartCoroutine(SwitchPageRoutine(page2));
        }

        private IEnumerator OpenEnvelopeRoutine() {
            isTransitioning = true;

            if (GuestDataManager.Instance != null) {
                GuestDataManager.Instance.SetLetterRead(true);
            }

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

            SetCanvasGroupState(envelopCanvasGroup, alpha: 0f, interactable: false, active: false);
            SetCanvasGroupState(guideTextCanvasGroup, alpha: 0f, interactable: false, active: false);

            SetCanvasGroupState(pagesCanvasGroup, alpha: 1f, interactable: true, active: true);
            SetCanvasGroupState(globalUICanvasGroup, alpha: 1f, interactable: true, active: true);

            isTransitioning = false;
            StartCoroutine(SendMarkReadApi());
        }

        private IEnumerator SendMarkReadApi() {
            string accessCode = GuestDataManager.Instance != null ? GuestDataManager.Instance.AccessCode : string.Empty;
            if (string.IsNullOrEmpty(accessCode)) {
                accessCode = PlayerPrefs.GetString("SavedAccessCode", string.Empty);
            }

            if (!string.IsNullOrEmpty(accessCode) && NetworkManager.Instance != null) {
                var requestBody = new MarkReadRequestData { code = accessCode };

                yield return StartCoroutine(NetworkManager.Instance.PostRequest<MarkReadRequestData, bool>(
                    "/api/guest/mark-read",
                    requestBody,
                    (success, responseData, errCode) => {
                        if (!success) {
                            Debug.LogWarning($"[LetterController] Failed to mark letter as read. Error: {errCode}");
                        }
                    }
                ));
            }
        }

        private IEnumerator SwitchPageRoutine(GameObject targetPage) {
            isTransitioning = true;
            float timer = 0f;

            while (timer < fadeDuration) {
                timer += Time.unscaledDeltaTime;
                if (pagesCanvasGroup != null) {
                    pagesCanvasGroup.alpha = 1f - (timer / fadeDuration);
                }
                yield return null;
            }

            if (pagesCanvasGroup != null) pagesCanvasGroup.alpha = 0f;

            if (targetPage != null) {
                targetPage.transform.SetAsLastSibling();
                UpdatePageNavigationButtons(targetPage);
            }

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

        private IEnumerator ShowAndHideSavedTextRoutine() {
            if (savedTextCanvasGroup == null) yield break;

            SetCanvasGroupState(savedTextCanvasGroup, alpha: 0f, interactable: false, active: true);

            float timer = 0f;
            while (timer < fadeDuration) {
                timer += Time.unscaledDeltaTime;
                savedTextCanvasGroup.alpha = Mathf.Clamp01(timer / fadeDuration);
                yield return null;
            }
            savedTextCanvasGroup.alpha = 1f;

            yield return new WaitForSecondsRealtime(textDisplayDuration);

            timer = 0f;
            while (timer < fadeDuration) {
                timer += Time.unscaledDeltaTime;
                savedTextCanvasGroup.alpha = Mathf.Clamp01(1f - (timer / fadeDuration));
                yield return null;
            }

            SetCanvasGroupState(savedTextCanvasGroup, alpha: 0f, interactable: false, active: true);
            savedTextFadeCoroutine = null;
        }

        private void UpdatePageNavigationButtons(GameObject targetPage) {
            bool isPage1 = (targetPage == page1);

            if (nextPageButton != null) nextPageButton.gameObject.SetActive(isPage1);
            if (prevPageButton != null) prevPageButton.gameObject.SetActive(!isPage1);
        }

        private void SetCanvasGroupState(CanvasGroup cg, float alpha, bool interactable, bool active) {
            if (cg == null) return;
            cg.alpha = alpha;
            cg.blocksRaycasts = interactable;
            cg.interactable = interactable;
            cg.gameObject.SetActive(active);
        }

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