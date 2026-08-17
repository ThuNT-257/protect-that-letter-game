using TMPro;
using UnityEngine;

/// <summary>
/// Automatically updates a TextMeshPro text component with localized text.
/// Listens to language change events and refreshes its content accordingly.
/// </summary>
[RequireComponent(typeof(TMP_Text))]
public class LocalizedText : MonoBehaviour
{
    #region Serialized Fields
    // The localization key to look up
    [SerializeField] private string key;
    #endregion

    #region Private Fields
    private TMP_Text textComponent;
    #endregion

    #region Properties
    public void SetKey(string newKey)
    {
        key = newKey;
        UpdateText();
    }
    #endregion

    #region Lifecycle
    /// <summary>
    /// Caches the TMP_Text component for performance.
    /// </summary>
    private void Awake()
    {
        textComponent = GetComponent<TMP_Text>();
    }

    /// <summary>
    /// Subscribes to language change events when the object becomes active.
    /// Also updates the text immediately to ensure correct display.
    /// </summary>
    private void OnEnable()
    {
        LocalizationManager.OnLanguageChanged += UpdateText;
        UpdateText();
    }

    /// <summary>
    /// Unsubscribes from language change events.
    /// </summary>
    private void OnDisable()
    {
        LocalizationManager.OnLanguageChanged -= UpdateText;
    }
    #endregion

    #region Public Methods
    //---------------
    /// <summary>
    /// Updates the TMP_Text component with the localized text for the current key.
    /// Only executes if the LocalizationManager exists and the key is valid.
    /// </summary>
    public void UpdateText()
    {
        if(LocalizationManager.Instance != null && !string.IsNullOrEmpty(key))
        {
            textComponent.text = LocalizationManager.Instance.GetText(key);
        }
    }
    #endregion
}
