using ProtectThatLetter.Definitions;
using ProtectThatLetter.Managers;
using System.Collections;
using UnityEngine;

namespace ProtectThatLetter.UI {
    /// <summary>
    /// Manages dialogue flow: loads story data, displays lines,
    /// and handles next/skip interactions.
    /// </summary>
    [DisallowMultipleComponent]
    public class DialogueManager : MonoBehaviour {
        #region Serialized Fields
        [Header("UI References")]
        [SerializeField] private DialogueUI dialogueUI; // Reference to the dialogue UI

        [Header("Intro Settings")]
        [SerializeField] private float introDelayDuration = 2.5f; // Delay before first line (intro media)
        #endregion

        #region Private Fields
        private StoryData currentStory;              // Currently loaded story data
        private int currentLineIndex = 0;            // Current dialogue line index
        private Coroutine startStoryCoroutine;       // Reference to the startup coroutine
        #endregion

        #region Lifecycle
        /// <summary>
        /// Loads story data and starts the intro sequence
        /// </summary>
        private void Start() {
            LoadStoryData();
            if (startStoryCoroutine != null) StopCoroutine(startStoryCoroutine);
            startStoryCoroutine = StartCoroutine(StartStoryRoutine());
        }

        /// <summary>
        /// Subscribes to events when enabled
        /// </summary>
        private void OnEnable() {
            DialogueUI.OnNextButtonClicked += OnNextClicked;
            LocalizationManager.OnLanguageChanged += OnLanguageChanged;
        }

        /// <summary>
        /// Unsubscribes from events to prevent memory leaks
        /// </summary>
        private void OnDisable() {
            DialogueUI.OnNextButtonClicked -= OnNextClicked;
            LocalizationManager.OnLanguageChanged -= OnLanguageChanged;
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Plays intro media, waits, then displays the first line
        /// </summary>
        private IEnumerator StartStoryRoutine() {
            // Validate story data
            if (currentStory == null || currentStory.lines == null || currentStory.lines.Count == 0) {
                Debug.LogError("[DialogueManager] Invalid story data!");
                yield break;
            }

            if (introDelayDuration > 0f) {
                // Hide dialogue overlay and play intro media (background + sound)
                dialogueUI.HideDialogueOverlay();

                DialogueLine firstLine = currentStory.lines[0];
                dialogueUI.PlayIntroMedia(firstLine.background, firstLine.sound);

                // Wait for intro media to finish
                yield return new WaitForSeconds(introDelayDuration);
            }

            DisplayCurrentLine();
        }

        /// <summary>
        /// Reloads story data and refreshes the current line when language changes
        /// </summary>
        private void OnLanguageChanged(string newLanguageCode) {
            LoadStoryData();
            DisplayCurrentLine();
        }

        /// <summary>
        /// Loads the story JSON from Resources with multiple fallback paths
        /// </summary>
        private void LoadStoryData() {
            // Get the current story filename from StoryManager
            string storyFileName = SceneController.Instance != null
                ? StoryManager.Instance.GetCurrentStoryFileName()
                : "IntroStory";

            // Get the current language (fallback to Vietnamese)
            string lang = LocalizationManager.Instance != null
                ? LocalizationManager.Instance.CurrentLanguageCode
                : GameDefinitions.Languages.VIETNAMESE;

            // Normalize locale codes like "en-US" → "en"
            if (!string.IsNullOrEmpty(lang) && lang.Contains("-")) {
                lang = lang.Split('-')[0];
            }

            // Try path 1: localized file (e.g., StoryData/IntroStory_en)
            string fullPath = $"StoryData/{storyFileName}_{lang}";
            TextAsset jsonFile = Resources.Load<TextAsset>(fullPath);

            // Try path 2: non-localized file (e.g., StoryData/IntroStory)
            if (jsonFile == null) {
                fullPath = $"StoryData/{storyFileName}";
                jsonFile = Resources.Load<TextAsset>(fullPath);
            }

            // Try path 3: root-level file (e.g., IntroStory)
            if (jsonFile == null) {
                fullPath = storyFileName;
                jsonFile = Resources.Load<TextAsset>(fullPath);
            }

            // Parse the JSON if found
            if (jsonFile != null) {
                currentStory = JsonUtility.FromJson<StoryData>(jsonFile.text);
            } else {
                Debug.LogError($"[DialogueManager] FAIL: File not found");
                EndStory();
            }
        }

        /// <summary>
        /// Advances to the next dialogue line
        /// </summary>
        private void OnNextClicked() {
            currentLineIndex++;
            DisplayCurrentLine();
        }

        /// <summary>
        /// Ends the story and loads the next scene
        /// </summary>
        private void EndStory() {
            if (SceneController.Instance != null) {
                SceneController.Instance.LoadNextScene();
            } else {
                Debug.LogError("[DialogueManager] SceneController instance is missing!");
            }
        }

        /// <summary>
        /// Displays the current line with processed tokens (e.g., {GuestName})
        /// </summary>
        private void DisplayCurrentLine() {
            // Validate story and line index
            if (currentStory == null || currentStory.lines == null || currentStory.lines.Count == 0) return;

            // End story if all lines are shown
            if (currentLineIndex >= currentStory.lines.Count) {
                EndStory();
                return;
            }

            DialogueLine rawLine = currentStory.lines[currentLineIndex];

            // Get the guest name for token replacement
            string guestName = GetCurrentGuestName();

            // Process tokens in speaker and content
            DialogueLine processedLine = new DialogueLine {
                speaker = ProcessTextTokens(rawLine.speaker, guestName),
                content = ProcessTextTokens(rawLine.content, guestName),
                avatar = rawLine.avatar,
                position = rawLine.position,
                background = rawLine.background,
                sound = rawLine.sound
            };

            // Display the line (flag first line for special handling)
            if (dialogueUI != null) {
                bool isFirstLine = (currentLineIndex == 0);
                dialogueUI.DisplayLine(processedLine, isFirstLine);
            }
        }

        /// <summary>
        /// Replaces text tokens like {GuestName} with actual values
        /// </summary>
        private string ProcessTextTokens(string text, string replacementName) {
            if (string.IsNullOrEmpty(text)) return string.Empty;

            return text.Replace("{GuestName}", replacementName);
        }

        /// <summary>
        /// Gets the current guest name (fallback to "Guest")
        /// </summary>
        private string GetCurrentGuestName() {
            if (GuestDataManager.Instance != null && !string.IsNullOrEmpty(GuestDataManager.Instance.GuestName)) {
                return GuestDataManager.Instance.GuestName;
            }

            return "Guest"; // Fallback name
        }
        #endregion
    }
}