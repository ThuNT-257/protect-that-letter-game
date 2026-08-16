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
    // Serialized Fields
    //------------------
    [Header("Login Form")]
    [SerializeField] private TMP_InputField codeInputField;
    [SerializeField] private Button submitButton;
    [SerializeField] private TMP_Text errorText;

    [Header("Loading Overlay")]
    [SerializeField] private GameObject loadingOverlay;

    // Private Fields
    //---------------
    private string currentErrorKey = string.Empty;

    // Unity Lifecycle
    //----------------
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

    // Public Methods
    //---------------
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

        StartCoroutine(SubmitCode(inputCode));
    }

    private IEnumerator SubmitCode(string inputCode)
    {
        SetLoading(true);
        yield return new WaitForSeconds(2.0f);
        SetLoading(false);
        OnServerResponseMessage("MEOBEO");
    }

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

        if (!string.IsNullOrEmpty(currentErrorKey))
        {
            ClearError();
        }
    }

    private void OnInputEndEdit(string text)
    {
        if (codeInputField.wasCanceled) return;

        if (TouchScreenKeyboard.isSupported)
        {
            OnSubmitClicked();
        }
        else if (UnityEngine.InputSystem.Keyboard.current != null &&
            (UnityEngine.InputSystem.Keyboard.current.enterKey.wasPressedThisFrame ||
             UnityEngine.InputSystem.Keyboard.current.numpadEnterKey.wasPressedThisFrame))
        {
            if (loadingOverlay != null && !loadingOverlay.activeSelf)
            {
                OnSubmitClicked();
            }
        }
    }

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

    private void ClearError()
    {
        currentErrorKey = string.Empty;

        if(errorText != null)
        {
            errorText.text = string.Empty;
            errorText.gameObject.SetActive(false);
        }
    }

    private void RefreshLocalizedErrorText()
    {
        if(!string.IsNullOrEmpty(currentErrorKey) && errorText != null && errorText.gameObject.activeSelf)
        {
            errorText.text = GetLocalizedString(currentErrorKey);
        }
    }

    private string GetLocalizedString(string key)
    {
        if(LocalizationManager.Instance == null)
        {
            Debug.LogError("There is no LocalizationManager in Scene.");
            return key;
        }
        return LocalizationManager.Instance.GetText(key);
    }

    private void SetLoading(bool isLoading)
    {
        if (loadingOverlay != null)
        {
            loadingOverlay.SetActive(isLoading);
        }
    }

    public void OnServerResponseMessage(string errorCode)
    {
        SetLoading(false);
        if(errorCode == "MEOBEO")
        {
            SceneController.Instance.LoadScene("StoryScene");
        }
        DisplayError(errorCode);
    }


}
