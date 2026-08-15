using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class LocalizedText : MonoBehaviour
{
    [SerializeField] private string key;

    private TMP_Text textComponent;

    public void SetKey(string newKey)
    {
        key = newKey;
        UpdateText();
    }

    private void Awake()
    {
        textComponent = GetComponent<TMP_Text>();
    }

    private void OnEnable()
    {
        LocalizationManager.OnLanguageChanged += UpdateText;
        UpdateText();
    }

    private void OnDisable()
    {
        LocalizationManager.OnLanguageChanged -= UpdateText;
    }

    public void UpdateText()
    {
        if(LocalizationManager.Instance != null && !string.IsNullOrEmpty(key))
        {
            textComponent.text = LocalizationManager.Instance.GetText(key);
        }
    }
}
