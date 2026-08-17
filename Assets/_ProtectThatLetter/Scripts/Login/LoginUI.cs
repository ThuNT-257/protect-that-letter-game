using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages the login UI functionality including input validation, 
/// localization, and server communication simulation.
/// </summary>
public class LoginUI : MonoBehaviour
{
    #region Serialized Fields
    [Header("Login Form")]
    [SerializeField] private TMP_InputField codeInputField;
    [SerializeField] private Button submitButton;
    [SerializeField] private TMP_Text errorText;

    [Header("Loading Overlay")]
    [SerializeField] private GameObject loadingOverlay;
    #endregion

    #region Private Fields
    private string currentErrorKey = string.Empty;
    #endregion

    #region Lifecycle
    /// <summary>
    /// Sets up event listeners and initial UI state.
    /// </summary>
    private void Awake()
    {
        if(submitButton != null)
        {
            submitButton.onClick.AddListener(OnSubmitClicked);
        }

        if(codeInputField != null)
        {
            codeInputField.onValueChanged.AddListener(OnInputChanged);
            codeInputField.onEndEdit.AddListener(OnInputEndEdit);
        }

        SetLoading(false);
        ClearError();
    }

    /// <summary>
    /// Subscribes to language change events when the object becomes active.
    /// </summary>
    private void OnEnable()
    {
        LocalizationManager.OnLanguageChanged += RefreshLocalizedErrorText;
    }

    /// <summary>
    /// Unsubscribes from language change events.
    /// </summary>
    private void OnDisable()
    {
        LocalizationManager.OnLanguageChanged -= RefreshLocalizedErrorText;
    }
    #endregion

    #region Private Methods
    /// <summary>
    /// Handles the submit button click event.
    /// Validates the input code before sending to server.
    /// </summary>
    private void OnSubmitClicked()
    {
        string inputCode = codeInputField.text.Trim();

        // Validation 1: Check if code is empty
        if (string.IsNullOrEmpty(inputCode))
        {
            DisplayError("ERROR_EMPTY_CODE");
            return;
        }

        // Validation 2: Check if code has exactly 6 characters
        if (inputCode.Length != 6)
        {
            DisplayError("ERROR_INVALID_CODE");
            return;
        }

        ClearError();

        //Submit code to server (will deploy later)
        StartCoroutine(SubmitCode(inputCode));
    }

    /// <summary>
    /// Simulates server communication with a 2-second delay.
    /// </summary>
    /// <param name="inputCode">The code submitted by the user</param>
    private IEnumerator SubmitCode(string inputCode)
    {
        SetLoading(true);
        yield return new WaitForSeconds(2.0f);
        SetLoading(false);

        //Will replace by real api one later
        OnServerResponseMessage("MEOBEO");
    }

    /// <summary>
    /// Called when the code input field changes.
    /// Converts input to uppercase and clears error text when user types.
    /// </summary>
    /// <param name="value">The current input value</param>
    private void OnInputChanged(string value)
    {
        if (codeInputField != null)
        {
            string upperValue = value.ToUpper();
            if (codeInputField.text != upperValue)
            {
                codeInputField.text = upperValue;
            }
        }

        // Clear error
        if (!string.IsNullOrEmpty(currentErrorKey))
        {
            ClearError();
        }
    }

    /// <summary>
    /// Called when the user finishes editing the input field (presses Enter or tabs out).
    /// Handles keyboard and mouse/controller submissions.
    /// </summary>
    /// <param name="text">The final input text</param>
    private void OnInputEndEdit(string text)
    {
        //Not submit if user cancelled
        if (codeInputField.wasCanceled) return;

        // Handle touchscreen keyboard submission
        if (TouchScreenKeyboard.isSupported)
        {
            OnSubmitClicked();
        }
        // Handle physical keyboard Enter key
        else if (UnityEngine.InputSystem.Keyboard.current != null &&
            (UnityEngine.InputSystem.Keyboard.current.enterKey.wasPressedThisFrame ||
             UnityEngine.InputSystem.Keyboard.current.numpadEnterKey.wasPressedThisFrame))
        {
            //Only submit when loading panel is not active
            if (loadingOverlay != null && !loadingOverlay.activeSelf)
            {
                OnSubmitClicked();
            }
        }
    }

    /// <summary>
    /// Displays an error message using a localization key.
    /// </summary>
    /// <param name="errorKey">The localization key for the error message</param>
    private void DisplayError(string errorKey)
    {
        currentErrorKey = errorKey;

        if(errorText == null)
        {
            return;
        }

        string translatedMessage = GetLocalizedString(errorKey);
        errorText.text = translatedMessage;
        errorText.gameObject.SetActive(true);
    }

    /// <summary>
    /// Clears the currently displayed error message.
    /// </summary>
    private void ClearError()
    {
        currentErrorKey = string.Empty;

        if(errorText != null)
        {
            errorText.text = string.Empty;
            errorText.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Refreshes the error text when language changes
    /// </summary>
    private void RefreshLocalizedErrorText()
    {
        // Only update if there is an active error message
        if (!string.IsNullOrEmpty(currentErrorKey) && errorText != null && errorText.gameObject.activeSelf)
        {
            errorText.text = GetLocalizedString(currentErrorKey);
        }
    }

    /// <summary>
    /// Gets a localized string from the LocalizationManager.
    /// </summary>
    /// <param name="key">The localization key</param>
    /// <returns>The localized string, or the key itself in case manager is missing</returns>
    private string GetLocalizedString(string key)
    {
        if(LocalizationManager.Instance == null)
        {
            Debug.LogError("There is no LocalizationManager in Scene.");
            return key;
        }
        return LocalizationManager.Instance.GetText(key);
    }

    /// <summary>
    /// Shows or hides the loading overlay.
    /// </summary>
    /// <param name="isLoading">True to show loading, false to hide</param>
    private void SetLoading(bool isLoading)
    {
        if (loadingOverlay != null)
        {
            loadingOverlay.SetActive(isLoading);
        }
    }

    /// <summary>
    /// Handles the server response message.
    /// </summary>
    /// <param name="errorCode">The response code from the server</param>
    private void OnServerResponseMessage(string errorCode)
    {
        SetLoading(false);
        //Fake success response -> will handle later (update data for each code, load to story scene)
        if(errorCode == "MEOBEO")
        {
            SceneController.Instance.LoadScene(SceneController.STORY_SCENE);
        }
        DisplayError(errorCode);
    }
    #endregion
}
