using UnityEngine;
using UnityEngine.Localization;

/// <summary>
/// Manages dialogue flow, loading story data and displaying lines
/// </summary>
public class DialogueManager : MonoBehaviour {
    #region Serialized Fields
    [SerializeField] private DialogueUI dialogueUI;
    #endregion

    #region Private Fields
    private StoryData currentStory;
    private int currentLineIndex = 0;
    #endregion

    #region Lifecycle
    private void Start() {
        LoadStoryData();
        DisplayCurrentLine();
    }

    private void OnEnable() {
        DialogueUI.OnNextButtonClicked += OnNextClicked;
        LocalizationManager.OnLanguageChanged += OnLanguageChanged;
    }

    private void OnDisable() {
        DialogueUI.OnNextButtonClicked -= OnNextClicked;
        LocalizationManager.OnLanguageChanged -= OnLanguageChanged;
    }
    #endregion

    #region Private Methods
    /// <summary>
    /// Event handler when language changes during dialogue scene
    /// </summary>
    private void OnLanguageChanged(Locale newLocale) {
        LoadStoryData();
        DisplayCurrentLine();
    }

    /// <summary>
    /// Loads the story JSON file from Resources based on current story (Intro/Outro) and language
    /// </summary>
    private void LoadStoryData() {
        string storyFileName = StoryManager.GetCurrentStoryFileName();

        string lang = LocalizationManager.Instance != null
            ? LocalizationManager.Instance.CurrentLanguageCode
            : LocalizationManager.VIETNAMESE;

        if (!string.IsNullOrEmpty(lang) && lang.Contains("-")) {
            lang = lang.Split('-')[0];
        }

        string fullPath = $"StoryData/{storyFileName}_{lang}";
        Debug.Log($"[DialogueManager] Trying to load file in: Resources/{fullPath}");

        TextAsset jsonFile = Resources.Load<TextAsset>(fullPath);

        if (jsonFile == null) {
            fullPath = $"StoryData/{storyFileName}";
            Debug.LogWarning($"[DialogueManager] Not found language files, fallback to base name.");
            jsonFile = Resources.Load<TextAsset>(fullPath);
        }

        if (jsonFile == null) {
            fullPath = storyFileName;
            Debug.LogWarning($"[DialogueManager] Try root fallback: Resources/{fullPath}");
            jsonFile = Resources.Load<TextAsset>(fullPath);
        }

        if (jsonFile != null) {
            currentStory = JsonUtility.FromJson<StoryData>(jsonFile.text);
            Debug.Log($"[DialogueManager] Load Story Data successfully!");
        } else {
            Debug.LogError($"[DialogueManager] FAIL: File not found");
            EndStory();
        }
    }

    private void OnNextClicked() {
        currentLineIndex++;
        DisplayCurrentLine();
    }

    private void EndStory() {
        string nextScene = StoryManager.GetNextSceneName();
        if (SceneController.Instance != null) {
            SceneController.Instance.LoadScene(nextScene);
        } else {
            UnityEngine.SceneManagement.SceneManager.LoadScene(nextScene);
        }
    }

    private void DisplayCurrentLine() {
        if (currentStory == null) {
            Debug.LogError("[DialogueManager] currentStory is NULL!");
            return;
        }

        if (currentStory.lines == null || currentStory.lines.Count == 0) {
            Debug.LogError("[DialogueManager] Story contains no dialogue lines!");
            return;
        }

        if (currentLineIndex >= currentStory.lines.Count) {
            EndStory();
            return;
        }

        DialogueLine line = currentStory.lines[currentLineIndex];
        if (dialogueUI != null) {
            dialogueUI.DisplayLine(line);
        }
    }
    #endregion
}