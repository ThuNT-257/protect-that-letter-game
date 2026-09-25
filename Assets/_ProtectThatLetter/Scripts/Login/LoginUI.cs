using ProtectThatLetter.Definitions;
using ProtectThatLetter.Managers;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProtectThatLetter.UI {
    [DisallowMultipleComponent]
    public class LoginUI : MonoBehaviour {
        #region Serialized Fields
        [Header("Login Form")]
        [SerializeField] private TMP_InputField codeInputField;
        [SerializeField] private Button submitButton;
        [SerializeField] private TMP_Text errorText;

        [Header("Error Background (Cloud Fly Left-To-Right Animation)")]
        [SerializeField] private RectTransform errorBackground;
        [SerializeField] private Vector2 hideLeftPosition = new Vector2(-1200f, -200f);
        [SerializeField] private Vector2 targetPosition = new Vector2(0f, -200f);
        [SerializeField] private Vector2 hideRightPosition = new Vector2(1200f, -200f);
        [SerializeField] private float slideDuration = 0.25f;

        [Header("Loading Overlay")]
        [SerializeField] private GameObject loadingOverlay;
        #endregion

        #region Private Fields
        private string currentErrorKey = string.Empty;
        private Coroutine floatAnimationCoroutine;
        private Coroutine slideAnimationCoroutine;
        #endregion

        #region Data DTOs
        [Serializable]
        private class SendCodeRequestData {
            public string code;
        }

        [Serializable]
        private class CheckCodeResponseData {
            public int inviteId;
            public bool hasCompletedGame;
            public string guestName;
            public string guestNickname;
            public bool isLetterRead;
            public string letterContentVn;
            public string letterContentEn;
            public string imageUrl;
            public bool? isAttending;
            public string guestNote;
        }
        #endregion

        #region Lifecycle
        private void Awake() {
            if (submitButton != null) {
                submitButton.onClick.AddListener(OnSubmitClicked);
            }

            if (codeInputField != null) {
                codeInputField.onValueChanged.AddListener(OnInputChanged);
                codeInputField.onEndEdit.AddListener(OnInputEndEdit);
            }

            SetLoading(false);

            if (errorBackground != null) {
                errorBackground.anchoredPosition = hideLeftPosition;
                errorBackground.gameObject.SetActive(false);
            }

            ClearError();
        }

        private void OnEnable() {
            LocalizationManager.OnLanguageChanged += OnLanguageChanged;
        }

        private void Start() {
            if (AudioManager.Instance != null) {
                AudioManager.Instance.PlayBGMIndex(0);
            }
        }

        private void OnDisable() {
            LocalizationManager.OnLanguageChanged -= OnLanguageChanged;
        }

        private void OnDestroy() {
            if (submitButton != null) {
                submitButton.onClick.RemoveListener(OnSubmitClicked);
            }

            if (codeInputField != null) {
                codeInputField.onValueChanged.RemoveListener(OnInputChanged);
                codeInputField.onEndEdit.RemoveListener(OnInputEndEdit);
            }
        }
        #endregion

        #region Private Methods
        private void OnLanguageChanged(string newLanguageCode) {
            RefreshLocalizedErrorText();
        }

        private void OnSubmitClicked() {
            if (AudioManager.Instance != null) {
                AudioManager.Instance.PlaySFX("button_click");
            }

            string inputCode = codeInputField != null ? codeInputField.text.Trim() : string.Empty;

            if (string.IsNullOrEmpty(inputCode)) {
                DisplayError("ERROR_EMPTY_CODE");
                return;
            }

            if (inputCode.Length != 6) {
                DisplayError("ERROR_INVALID_CODE");
                return;
            }

            ClearError();
            SendCodeAndProceed(inputCode);
        }

        private void SendCodeAndProceed(string inputCode) {
            SetLoading(true);

            bool isSuccess = false;
            CheckCodeResponseData responseData = null;

            SetLoading(false);

            if (isSuccess && responseData != null) {
                PlayerPrefs.SetString("SavedAccessCode", inputCode);
                PlayerPrefs.Save();

                if (GuestDataManager.Instance != null) {
                    Debug.Log($"[LoginUI] Received isAttending from API: {responseData.isAttending}");

                    GuestDataManager.Instance.SaveGuestData(
                        responseData.inviteId,
                        inputCode,
                        responseData.guestName,
                        responseData.guestNickname,
                        responseData.letterContentVn,
                        responseData.letterContentEn,
                        responseData.imageUrl,
                        responseData.hasCompletedGame,
                        responseData.isLetterRead,
                        responseData.isAttending,
                        responseData.guestNote
                    );
                }

                if (SceneController.Instance != null) {
                    if (responseData.hasCompletedGame) {
                        SceneController.Instance.LoadSceneByName(SceneName.LETTER_SCENE);
                    } else {
                        SceneController.Instance.LoadNextScene();
                    }
                }
            } else {
                string errorCodeToDisplay = "FIX_LATER";
                DisplayError(errorCodeToDisplay);
            }
        }

        private void OnInputChanged(string value) {
            if (AudioManager.Instance != null) {
                AudioManager.Instance.PlaySFX("typing_one");
            }

            if (codeInputField != null) {
                string upperValue = value.ToUpper();
                if (codeInputField.text != upperValue) {
                    codeInputField.text = upperValue;
                }
            }

            if (!string.IsNullOrEmpty(currentErrorKey)) {
                ClearError();
            }
        }

        private void OnInputEndEdit(string text) {
            if (codeInputField != null && codeInputField.wasCanceled) return;

            bool isEnterPressed = false;

            if (UnityEngine.InputSystem.Keyboard.current != null) {
                var keyboard = UnityEngine.InputSystem.Keyboard.current;
                if (keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame) {
                    isEnterPressed = true;
                }
            }

            if (!isEnterPressed && TouchScreenKeyboard.isSupported && codeInputField.touchScreenKeyboard != null) {
                if (codeInputField.touchScreenKeyboard.status == TouchScreenKeyboard.Status.Done) {
                    isEnterPressed = true;
                }
            }

            if (isEnterPressed) {
                if (loadingOverlay != null && !loadingOverlay.activeSelf) {
                    OnSubmitClicked();
                }
            }
        }

        private void DisplayError(string errorKey) {
            bool isAlreadyShowingSameError = (currentErrorKey == errorKey) &&
                                            (errorBackground != null && errorBackground.gameObject.activeSelf);

            currentErrorKey = errorKey;

            if (errorText == null) return;

            FetchAndSetErrorText(errorKey);
            errorText.gameObject.SetActive(true);

            if (!isAlreadyShowingSameError) {
                AnimateCloudInFromLeft();
            }
        }

        private void ClearError() {
            currentErrorKey = string.Empty;

            if (errorText != null) {
                errorText.text = string.Empty;
                errorText.gameObject.SetActive(false);
            }

            AnimateCloudOutToRight();
        }

        #region Cloud Animation Logic
        private void AnimateCloudInFromLeft() {
            if (errorBackground == null) return;

            StopCloudCoroutines();

            errorBackground.gameObject.SetActive(true);
            errorBackground.anchoredPosition = hideLeftPosition;

            slideAnimationCoroutine = StartCoroutine(SlideCloud(hideLeftPosition, targetPosition, slideDuration, () => {
                floatAnimationCoroutine = StartCoroutine(FloatCloudLoop());
            }));
        }

        private void AnimateCloudOutToRight() {
            if (errorBackground == null || !errorBackground.gameObject.activeSelf) return;

            StopCloudCoroutines();

            Vector2 currentPos = errorBackground.anchoredPosition;

            slideAnimationCoroutine = StartCoroutine(SlideCloud(currentPos, hideRightPosition, slideDuration, () => {
                errorBackground.gameObject.SetActive(false);
                errorBackground.anchoredPosition = hideLeftPosition;
            }));
        }

        private void StopCloudCoroutines() {
            if (slideAnimationCoroutine != null) StopCoroutine(slideAnimationCoroutine);
            if (floatAnimationCoroutine != null) StopCoroutine(floatAnimationCoroutine);
        }

        private IEnumerator SlideCloud(Vector2 start, Vector2 end, float duration, Action onComplete = null) {
            float elapsed = 0f;
            while (elapsed < duration) {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
                errorBackground.anchoredPosition = Vector2.Lerp(start, end, t);
                yield return null;
            }
            errorBackground.anchoredPosition = end;
            onComplete?.Invoke();
        }

        private IEnumerator FloatCloudLoop() {
            float floatSpeed = 2.0f;
            float floatAmount = 8.0f;

            while (true) {
                float newY = targetPosition.y + (Mathf.Sin(Time.time * floatSpeed) * floatAmount);
                errorBackground.anchoredPosition = new Vector2(targetPosition.x, newY);
                yield return null;
            }
        }
        #endregion

        private void RefreshLocalizedErrorText() {
            if (!string.IsNullOrEmpty(currentErrorKey) && errorText != null && errorText.gameObject.activeSelf) {
                FetchAndSetErrorText(currentErrorKey);
            }
        }

        private void FetchAndSetErrorText(string key) {
            if (LocalizationManager.Instance != null) {
                LocalizationManager.Instance.GetLocalizedString(key, (translatedText) => {
                    if (errorText != null) {
                        errorText.text = translatedText;
                    }
                });
            } else {
                if (errorText != null) errorText.text = key;
            }
        }

        private void SetLoading(bool isLoading) {
            if (loadingOverlay != null) {
                loadingOverlay.SetActive(isLoading);
            }
        }
        #endregion
    }
}