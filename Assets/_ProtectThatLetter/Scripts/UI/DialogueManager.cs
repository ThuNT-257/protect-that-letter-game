using ProtectThatLetter.Definitions;
using ProtectThatLetter.Managers;
using System;
using System.Collections;
using UnityEngine;

namespace ProtectThatLetter.UI
{
    /// <summary>
    /// Manages dialogue flow: loads story data, displays lines,
    /// and handles next/skip interactions.
    /// Broadcasts events upon completion or failure.
    /// </summary>
    [DisallowMultipleComponent]
    public class DialogueManager : MonoBehaviour
    {
        #region Serialized Fields
        [Header("UI References")]
        [SerializeField] private DialogueUI dialogueUI;            // Reference to the dialogue UI

        [Header("Intro Settings")]
        [SerializeField] private float introDelayDuration = 2.5f;  // Delay before first line
        #endregion

        #region Private Fields
        private StoryData currentStory;              // Currently loaded story
        private int currentLineIndex = 0;            // Current line index
        private Coroutine startStoryCoroutine;       // Startup coroutine reference
        private string currentStoryFileName;         // Cached story file name
        #endregion

        #region Events
        /// <summary>
        /// Fired when all dialogue lines have been completed successfully.
        /// </summary>
        public static event Action OnStoryCompleted;

        /// <summary>
        /// Fired when story JSON fails to load or contains no valid lines.
        /// Parameters: storyFileName, errorMessage
        /// </summary>
        public static event Action<string, string> OnStoryLoadFailed;
        #endregion

        #region Unity Lifecycle
        /// <summary>
        /// Loads and starts the story on scene start
        /// </summary>
        private void Start()
        {
            InitAndStartStory();
        }

        /// <summary>
        /// Subscribes to events when enabled
        /// </summary>
        private void OnEnable()
        {
            DialogueUI.OnNextButtonClicked += OnNextClicked;
            LocalizationManager.OnLanguageChanged += OnLanguageChanged;
        }

        /// <summary>
        /// Unsubscribes from events and stops pending coroutines
        /// </summary>
        private void OnDisable()
        {
            DialogueUI.OnNextButtonClicked -= OnNextClicked;
            LocalizationManager.OnLanguageChanged -= OnLanguageChanged;

            StopPendingStoryRoutine();
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Initializes and starts the story if loading succeeds
        /// </summary>
        private void InitAndStartStory()
        {
            if (TryLoadStoryData())
            {
                StopPendingStoryRoutine();
                startStoryCoroutine = StartCoroutine(StartStoryRoutine());
            }
            else
            {
                Debug.LogError($"[DialogueManager] Aborting story playback due to load failure: '{currentStoryFileName}'");
            }
        }

        /// <summary>
        /// Stops any pending startup coroutine
        /// </summary>
        private void StopPendingStoryRoutine()
        {
            if (startStoryCoroutine != null)
            {
                StopCoroutine(startStoryCoroutine);
                startStoryCoroutine = null;
            }
        }

        /// <summary>
        /// Attempts to load story JSON. Returns true if successful.
        /// </summary>
        private bool TryLoadStoryData()
        {
            // Get the story filename from StoryManager (fallback to IntroStory)
            currentStoryFileName = (StoryManager.Instance != null) 
                ? StoryManager.Instance.GetCurrentStoryFileName() : GameDefinitions.Story.INTRO_STORY_FILE;

            // Get current language (fallback to English)
            string lang = (LocalizationManager.Instance != null)
                ? LocalizationManager.Instance.CurrentLanguageCode
                : GameDefinitions.Languages.DEFAULT_LANGUAGE;

            // Normalize locale codes like "en-US" → "en"
            if (!string.IsNullOrEmpty(lang) && lang.Contains("-"))
            {
                lang = lang.Split('-')[0];
            }

            // Try path 1: Localized file (e.g., StoryData/IntroStory_en)
            TextAsset jsonFile = Resources.Load<TextAsset>($"{GameDefinitions.Story.STORY_DATA_FOLDER}/{currentStoryFileName}_{lang}");

            // Try path 2: Non-localized (e.g., StoryData/IntroStory)
            if (jsonFile == null)
            {
                jsonFile = Resources.Load<TextAsset>(
                    $"{GameDefinitions.Story.STORY_DATA_FOLDER}/{currentStoryFileName}");
            }

            // Try path 3: Root fallback (e.g., IntroStory)
            if (jsonFile == null)
            {
                jsonFile = Resources.Load<TextAsset>(currentStoryFileName);
            }

            // Parse if found
            if (jsonFile != null)
            {
                currentStory = JsonUtility.FromJson<StoryData>(jsonFile.text);
                if (currentStory != null && currentStory.lines != null && currentStory.lines.Count > 0)
                {
                    return true;
                }
            }

            // Report failure
            string errorMsg = $"Story file '{currentStoryFileName}' could not be found or parsed.";
            Debug.LogError($"[DialogueManager] FAIL: {errorMsg}");
            OnStoryLoadFailed?.Invoke(currentStoryFileName, errorMsg);

            return false;
        }

        /// <summary>
        /// Ends the story cleanly and notifies registered listeners.
        /// Pure event-driven implementation (no direct scene loading).
        /// </summary>
        private void EndStory()
        {
            Debug.Log($"[DialogueManager] Story '{currentStoryFileName}' completed.");
            OnStoryCompleted?.Invoke();
        }

        /// <summary>
        /// Displays the current line or ends the story if all lines are shown
        /// </summary>
        private void DisplayCurrentLine()
        {
            // Validate story data
            if (currentStory == null || currentStory.lines == null || currentStory.lines.Count == 0) return;

            // End story when all lines are shown
            if (currentLineIndex >= currentStory.lines.Count)
            {
                EndStory();
                return;
            }

            DialogueLine rawLine = currentStory.lines[currentLineIndex];

            if (dialogueUI != null)
            {
                bool isFirstLine = (currentLineIndex == 0);
                dialogueUI.DisplayLine(rawLine, isFirstLine);
            }
        }
        #endregion

        #region Coroutines
        /// <summary>
        /// Plays intro media, waits, then displays the first line
        /// </summary>
        private IEnumerator StartStoryRoutine()
        {
            // Validate story data
            if (currentStory == null || currentStory.lines == null || currentStory.lines.Count == 0)
            {
                Debug.LogError("[DialogueManager] Story data is null or contains no lines!");
                OnStoryLoadFailed?.Invoke(currentStoryFileName, "Story contains no lines.");
                yield break;
            }

            // Play intro media (background + sound) if a delay is set
            if (introDelayDuration > 0f)
            {
                if (dialogueUI != null)
                {
                    dialogueUI.HideDialogueOverlay();
                    DialogueLine firstLine = currentStory.lines[0];
                    dialogueUI.PlayIntroMedia(firstLine.background, firstLine.sound);
                }

                yield return new WaitForSeconds(introDelayDuration);
            }

            DisplayCurrentLine();
        }
        #endregion

        #region Event Handlers
        /// <summary>
        /// Advances to the next line when Next is clicked
        /// </summary>
        private void OnNextClicked()
        {
            currentLineIndex++;
            DisplayCurrentLine();
        }

        /// <summary>
        /// Reloads and redisplays the current line when the language changes
        /// </summary>
        private void OnLanguageChanged(string newLanguageCode)
        {
            if (TryLoadStoryData())
            {
                DisplayCurrentLine();
            }
        }
        #endregion
    }
}