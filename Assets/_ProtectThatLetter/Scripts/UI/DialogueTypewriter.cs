using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace ProtectThatLetter.UI {
    /// <summary>
    /// Handles the typewriter effect for dialogue text.
    /// Fires OnTypewriterCompleted when done (or when skipped).
    /// </summary>
    public class DialogueTypewriter : MonoBehaviour {
        #region Serialized Fields
        [SerializeField] private TextMeshProUGUI dialogueLineText; // Text component to animate
        [SerializeField] private float typingSpeed = 0.03f;        // Delay per character
        #endregion

        #region Events
        /// <summary>
        /// Raised when the typewriter finishes (naturally or via CompleteImmediately).
        /// </summary>
        public event Action OnTypewriterCompleted;
        #endregion

        #region Properties
        /// <summary>
        /// True while the typewriter is actively typing.
        /// </summary>
        public bool IsTyping { get; private set; }
        #endregion

        #region Private Fields
        private Coroutine typewriterCoroutine; // Reference to the running typewriter coroutine
        #endregion

        #region Unity Lifecycle
        /// <summary>
        /// Stops the typewriter when disabled to prevent leaks
        /// </summary>
        private void OnDisable() {
            StopTypewriter();
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Stops the typewriter coroutine and resets the typing flag
        /// </summary>
        public void StopTypewriter() {
            if (typewriterCoroutine != null) {
                StopCoroutine(typewriterCoroutine);
                typewriterCoroutine = null;
            }
            IsTyping = false;
        }

        /// <summary>
        /// Starts the typewriter effect for the given content
        /// </summary>
        /// <param name="content">The text to reveal character by character</param>
        public void StartTypewriter(string content) {
            StopTypewriter();

            if (dialogueLineText == null) return;

            dialogueLineText.text = content ?? "";
            typewriterCoroutine = StartCoroutine(TypewriterCoroutine());
        }

        /// <summary>
        /// Immediately completes the typewriter effect and fires the completion event
        /// </summary>
        public void CompleteImmediately() {
            StopTypewriter();

            if (dialogueLineText != null) {
                dialogueLineText.maxVisibleCharacters = dialogueLineText.textInfo.characterCount;
            }

            FinishTypewriter();
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Fires the completion event and resets the typing flag
        /// </summary>
        private void FinishTypewriter() {
            IsTyping = false;
            OnTypewriterCompleted?.Invoke();
        }
        #endregion

        #region Coroutines
        /// <summary>
        /// Coroutine that reveals characters one by one over time
        /// </summary>
        private IEnumerator TypewriterCoroutine() {
            IsTyping = true;
            dialogueLineText.ForceMeshUpdate();

            int totalVisibleCharacters = dialogueLineText.textInfo.characterCount;
            dialogueLineText.maxVisibleCharacters = 0;

            for (int visibleCount = 1; visibleCount <= totalVisibleCharacters; visibleCount++) {
                dialogueLineText.maxVisibleCharacters = visibleCount;
                yield return new WaitForSeconds(typingSpeed);
            }

            FinishTypewriter();
        }
        #endregion
    }
}