using System;
using UnityEngine;
using UnityEngine.UI;

namespace ProtectThatLetter.UI {
    /// <summary>
    /// Handles the Login UI: raises an event when submit is clicked.
    /// Delegates all game logic to external controllers.
    /// </summary>
    [DisallowMultipleComponent]
    public class LoginUI : MonoBehaviour {
        #region Serialized Fields
        [Header("Login Form")]
        [SerializeField] private Button submitButton; // Submit button
        #endregion

        #region Events
        /// <summary>
        /// Raised when the submit button is clicked.
        /// </summary>
        public event Action OnSubmitRequested;
        #endregion

        #region Unity Lifecycle
        /// <summary>
        /// Registers the button listener on startup
        /// </summary>
        private void Awake() {
            if (submitButton != null) {
                submitButton.onClick.AddListener(HandleSubmitClicked);
            }
        }

        /// <summary>
        /// Removes the button listener to prevent memory leaks
        /// </summary>
        private void OnDestroy() {
            if (submitButton != null) {
                submitButton.onClick.RemoveListener(HandleSubmitClicked);
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Enables or disables the submit button interaction
        /// </summary>
        /// <param name="interactable">True to enable, false to disable</param>
        public void SetInputInteractable(bool interactable) {
            if (submitButton != null) {
                submitButton.interactable = interactable;
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Handles the submit button click: disables input and raises the event
        /// </summary>
        private void HandleSubmitClicked() {
            SetInputInteractable(false);   // Prevent double-clicks
            OnSubmitRequested?.Invoke();   // Notify listeners
        }
        #endregion
    }
}