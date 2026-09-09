using System.Collections;
using UnityEngine;
using UnityEngine.Localization;
using ProtectThatLetter.Managers;

namespace ProtectThatLetter.UI {
    [DisallowMultipleComponent]
    public class DialogueManager : MonoBehaviour {
        #region Serialized Fields
        [Header("UI References")]
        [SerializeField] private DialogueUI dialogueUI;

        [Header("Intro Settings")]
        [SerializeField] private float introDelayDuration = 2.5f; 
        #endregion

        #region Private Fields
        private StoryData currentStory;
        private int currentLineIndex = 0;
        private Coroutine startStoryCoroutine;
        #endregion

        #region Lifecycle
        private void Start() {
            LoadStoryData();
            if (startStoryCoroutine != null) StopCoroutine(startStoryCoroutine);
            startStoryCoroutine = StartCoroutine(StartStoryRoutine());
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
        private IEnumerator StartStoryRoutine() {
            if (currentStory == null || currentStory.lines == null || currentStory.lines.Count == 0) {
                Debug.LogError("[DialogueManager] Invalid story data!");
                yield break;
            }

            if (introDelayDuration > 0f) {
                dialogueUI.HideDialogueOverlay();

                DialogueLine firstLine = currentStory.lines[0];
                dialogueUI.PlayIntroMedia(firstLine.background, firstLine.sound);

                yield return new WaitForSeconds(introDelayDuration);
            }

            DisplayCurrentLine();
        }

        private void OnLanguageChanged(Locale newLocale) {
            LoadStoryData();
            DisplayCurrentLine();
        }

        private void LoadStoryData() {
            string storyFileName = SceneManager.Instance != null
                ? SceneManager.Instance.GetCurrentStoryFileName()
                : "IntroStory";

            string lang = LocalizationManager.Instance != null
                ? LocalizationManager.Instance.CurrentLanguageCode
                : LocalizationManager.VIETNAMESE;

            if (!string.IsNullOrEmpty(lang) && lang.Contains("-")) {
                lang = lang.Split('-')[0];
            }

            string fullPath = $"StoryData/{storyFileName}_{lang}";
            TextAsset jsonFile = Resources.Load<TextAsset>(fullPath);

            if (jsonFile == null) {
                fullPath = $"StoryData/{storyFileName}";
                jsonFile = Resources.Load<TextAsset>(fullPath);
            }

            if (jsonFile == null) {
                fullPath = storyFileName;
                jsonFile = Resources.Load<TextAsset>(fullPath);
            }

            if (jsonFile != null) {
                currentStory = JsonUtility.FromJson<StoryData>(jsonFile.text);
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
            if (SceneManager.Instance != null) {
                SceneManager.Instance.LoadNextScene();
            } else {
                Debug.LogError("[DialogueManager] SceneController instance is missing!");
            }
        }

        private void DisplayCurrentLine() {
            if (currentStory == null || currentStory.lines == null || currentStory.lines.Count == 0) return;

            if (currentLineIndex >= currentStory.lines.Count) {
                EndStory();
                return;
            }

            DialogueLine line = currentStory.lines[currentLineIndex];
            if (dialogueUI != null) {
                bool isFirstLine = (currentLineIndex == 0);
                dialogueUI.DisplayLine(line, isFirstLine);
            }
        }
        #endregion
    }
}