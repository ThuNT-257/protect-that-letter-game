using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace ProtectThatLetter.UI
{
    public class DialogueTypewriter : MonoBehaviour
    {
        #region Serialized Fields
        [SerializeField] private TextMeshProUGUI dialogueLineText;
        [SerializeField] private float typingSpeed = 0.03f;
        #endregion

        #region Events
        public event Action OnTypewriterCompleted;
        #endregion

        #region Public Properties
        public bool IsTyping {  get; private set; }
        #endregion

        #region Private Fields
        private Coroutine typewriterCoroutine;
        #endregion

        #region Unity Lifecycle
        private void OnDisable()
        {
            StopTypewriter();
        }
        #endregion

        #region Public Methods
        public void StopTypewriter()
        {
            if (typewriterCoroutine != null)
            {
                StopCoroutine(typewriterCoroutine);
                typewriterCoroutine = null;
            }
            IsTyping = false;
        }

        public void StartTypewriter(string content)
        {
            StopTypewriter();

            if (dialogueLineText == null) return;

            dialogueLineText.text = content ?? "";
            typewriterCoroutine = StartCoroutine(TypewriterCoroutine());
        }
        #endregion

        #region Private Methods
        private IEnumerator TypewriterCoroutine()
        {
            IsTyping = true;
            dialogueLineText.ForceMeshUpdate();

            int totalVisibleCharacters = dialogueLineText.textInfo.characterCount;
            dialogueLineText.maxVisibleCharacters = 0;

            for(int visibleCount = 1; visibleCount <= totalVisibleCharacters; visibleCount++)
            {
                dialogueLineText.maxVisibleCharacters = visibleCount;
                yield return new WaitForSeconds(typingSpeed);
            }

            FinishTypewriter();
        }

        private void FinishTypewriter()
        {
            IsTyping = false;
            OnTypewriterCompleted?.Invoke();
        }
        #endregion
    }
}
