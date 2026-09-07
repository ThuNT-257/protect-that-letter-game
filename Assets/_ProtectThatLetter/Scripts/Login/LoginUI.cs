using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

/// <summary>
/// Manages the login UI functionality including input validation, 
/// localization, and guest send-code API verification before scene transition.
/// </summary>
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
        public bool hasCompletedGame;
        public string guestName;
        public string letterContentVn;
        public string letterContentEn;
        public string imageUrl;
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
    #endregion

    #region Private Methods
    private void OnLanguageChanged(Locale newLocale) {
        RefreshLocalizedErrorText();
    }

    private void OnSubmitClicked() {
        if (AudioManager.Instance != null) {
            AudioManager.Instance.PlaySFX("button_click");
        }

        string inputCode = codeInputField != null ? codeInputField.text.Trim() : string.Empty;

        Debug.Log($"[LoginUI] === Submit Button Clicked ===");
        Debug.Log($"[LoginUI] Raw Input Code: '{codeInputField?.text}', Trimmed Input Code: '{inputCode}' (Length: {inputCode.Length})");

        // 1. Check if code is empty
        if (string.IsNullOrEmpty(inputCode)) {
            Debug.LogWarning("[LoginUI] Validation Failed: Code is empty. Displaying 'ERROR_EMPTY_CODE'.");
            DisplayError("ERROR_EMPTY_CODE");
            return;
        }

        // 2. Check if code has exactly 6 characters
        if (inputCode.Length != 6) {
            Debug.LogWarning($"[LoginUI] Validation Failed: Code length is {inputCode.Length} (Expected 6). Displaying 'ERROR_INVALID_CODE'.");
            DisplayError("ERROR_INVALID_CODE");
            return;
        }

        Debug.Log("[LoginUI] Input validation passed. Clearing previous errors and sending Code...");
        ClearError();
        StartCoroutine(SendCodeAndProceed(inputCode));
    }

    /// <summary>
    /// Calls Send Code API via NetworkManager before changing scene.
    /// </summary>
    private IEnumerator SendCodeAndProceed(string inputCode) {
        Debug.Log("[LoginUI] SendCodeAndProceed started. Displaying loading overlay...");
        SetLoading(true);

        bool isRequestFinished = false;
        bool isSuccess = false;
        string returnedErrorCode = null;
        CheckCodeResponseData responseData = null;

        if (NetworkManager.Instance != null) {
            var requestBody = new SendCodeRequestData { code = inputCode };
            string jsonPayload = JsonUtility.ToJson(requestBody);

            Debug.Log($"[LoginUI] Sending POST request to '/api/guest/send-code' with payload: {jsonPayload}");
            yield return StartCoroutine(NetworkManager.Instance.PostRequest<CheckCodeResponseData>(
                "/api/guest/send-code",
                jsonPayload,
                (success, data, errCode) => {
                    isSuccess = success;
                    responseData = data;
                    returnedErrorCode = errCode;
                    isRequestFinished = true;
                    Debug.Log($"[LoginUI] NetworkManager Callback received -> Success: {isSuccess}, ErrorCode: '{returnedErrorCode}'");
                }
            ));
        } else {
            Debug.LogError("[LoginUI] NetworkManager Instance is null! Cannot proceed with send-code.");
            returnedErrorCode = ErrorCodes.ERROR_INTERNAL_SERVER;
            isRequestFinished = true;
        }

        yield return new WaitUntil(() => isRequestFinished);
        Debug.Log("[LoginUI] Request finished. Hiding loading overlay...");
        SetLoading(false);

        if (isSuccess) {
            Debug.Log("[LoginUI] Send Code SUCCESSFUL! Preparing to transition scene...");

            if (responseData != null && !responseData.hasCompletedGame) {
                PlayerPrefs.SetString("SavedAccessCode", inputCode);
                PlayerPrefs.Save();
                Debug.Log($"[LoginUI] Code '{inputCode}' successfully saved to PlayerPrefs.");
            }

            if (SceneController.Instance != null) {
                SceneController.Instance.CurrentStoryMode = StoryMode.Intro;
                SceneController.Instance.LoadNextScene();
            } else {
                Debug.LogError("[LoginUI] SceneController Instance is null! Cannot proceed.");
            }
        } else {
            string errorCodeToDisplay = !string.IsNullOrEmpty(returnedErrorCode)
                ? returnedErrorCode
                : ErrorCodes.ERROR_DATABASE_OFFLINE;

            Debug.LogError($"[LoginUI] Send Code FAILED. Displaying Error Code: '{errorCodeToDisplay}'");
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
        if (codeInputField.wasCanceled) return;

        if (TouchScreenKeyboard.isSupported) {
            Debug.Log("[LoginUI] Input EndEdit triggered via TouchScreenKeyboard.");
            OnSubmitClicked();
        } else if (UnityEngine.InputSystem.Keyboard.current != null &&
              (UnityEngine.InputSystem.Keyboard.current.enterKey.wasPressedThisFrame ||
               UnityEngine.InputSystem.Keyboard.current.numpadEnterKey.wasPressedThisFrame)) {
            if (loadingOverlay != null && !loadingOverlay.activeSelf) {
                Debug.Log("[LoginUI] Input EndEdit triggered via Enter key.");
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

    #region Cloud Animation Logic (In From Left -> Out To Right)
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
            Debug.LogError("[LoginUI] There is no LocalizationManager in Scene.");
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