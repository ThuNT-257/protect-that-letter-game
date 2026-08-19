using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages dialogue flow, loading story data and displaying lines
/// </summary>
public class DialogueManager : MonoBehaviour
{
    #region Serialized Fields
    [SerializeField] private DialogueUI dialogueUI;
    #endregion

    #region Private Fields
    private StoryData currentStory;
    private int currentLineIndex = 0;
    #endregion

    #region Lifecycle
    /// <summary>
    /// Loads story data and displays the first line when the script starts
    /// </summary>
    private void Start() {
        LoadStoryData();
        DisplayCurrentLine();
    }

    /// <summary>
    /// Subscribes to the next button click event when enabled
    /// </summary>
    private void OnEnable() {
        DialogueUI.OnNextButtonClicked += OnNextClicked;
    }

    /// <summary>
    /// Unsubscribes from events to prevent memory leaks
    /// </summary>
    private void OnDisable() {
        DialogueUI.OnNextButtonClicked -= OnNextClicked;
    }
    #endregion

    #region Private Methods
    /// <summary>
    /// Loads the story JSON file from Resources based on current story (Intro/Outro) and language
    /// </summary>
    private void LoadStoryData() {
        string storyFileName = StoryManager.GetCurrentStoryFileName();

        string lang = LocalizationManager.Instance != null ? LocalizationManager.Instance.CurrentLanguage : LocalizationManager.VIETNAMESE;

        string fullPath = $"StoryData/{storyFileName}_{lang}";
        TextAsset jsonFile = Resources.Load<TextAsset>(fullPath);

        if(jsonFile != null) {
            currentStory = JsonUtility.FromJson<StoryData>(jsonFile.text);
        } else {
            Debug.LogError($"[DialogueManager] Not found file JSON at: Resources/{fullPath}");
            EndStory();
        }
    }

    /// <summary>
    /// Handles next button click by advancing to the next line
    /// </summary>
    private void OnNextClicked() {
        currentLineIndex++;
        DisplayCurrentLine();
    }

    /// <summary>
    /// Ends the current story and loads the next scene
    /// </summary>
    private void EndStory() {
        string nextScene = StoryManager.GetNextSceneName();
        if (SceneController.Instance != null) {
            SceneController.Instance.LoadScene(nextScene);
        } else {
            UnityEngine.SceneManagement.SceneManager.LoadScene(nextScene);
        }
    }

    /// <summary>
    /// Displays the current dialogue line or ends the story if complete
    /// </summary>
    private void DisplayCurrentLine() {
        if (currentStory == null || currentStory.lines == null || currentLineIndex >= currentStory.lines.Count) {
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
