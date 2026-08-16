using System;
using TMPro;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.UI;

public class LoginUI : MonoBehaviour
{
    [Header("Login form")]
    [SerializeField] private TMP_InputField codeInputField;
    [SerializeField] private Button submitButton;
    [SerializeField] private TMP_Text errorText;

    private string currentErrorKey = string.Empty;

    private void Awake()
    {
        if(submitButton != null)
        {
            submitButton.onClick.AddListener(OnSubmitClicked);
        }
    }

    private void OnSubmitClicked()
    {
        string inputCode = codeInputField.text.Trim();
        
        //check empty code
        if (string.IsNullOrEmpty(inputCode))
        {
            DisplayError("ERROR_EMPTY_CODE");
            return;
        }

        //check max length
        if(inputCode.Length != 6)
        {
            DisplayError("ERROR_INVALID_CODE");
        }

        ClearError();
    }

    private void DisplayError(string errorKey)
    {
        currentErrorKey = errorKey;

        if(errorText == null)
        {
            return;
        }

        string translatedMessage = GetLocalizedString(errorKey);
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

    private string GetLocalizedString(string key)
    {
        if(LocalizationManager.Instance == null)
        {
            Debug.LogError("There is no LocalizationManager in Scene.");
            return key;
        }
        return LocalizationManager.Instance.GetText(key);
    }

}
