using ProtectThatLetter.Definitions;
using ProtectThatLetter.Managers;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProtectThatLetter.UI {
    /// <summary>
    /// Handles the Login UI: plays BGM and triggers scene transition on submit.
    /// </summary>
    [DisallowMultipleComponent]
    public class LoginUI : MonoBehaviour {
        #region Serialized Fields
        [Header("Login Form")]
        [SerializeField] private Button submitButton; // Submit button
        #endregion

        #region Unity Lifecycle
        /// <summary>
        /// Registers button listener on startup
        /// </summary>
        private void Awake() {
            if (submitButton != null) {
                submitButton.onClick.AddListener(OnSubmitClicked);
            }
        }

        /// <summary>
        /// Plays the login BGM when the scene starts
        /// </summary>
        private void Start() {
            PlayLoginBGM();
        }

        /// <summary>
        /// Removes button listener to prevent memory leaks
        /// </summary>
        private void OnDestroy() {
            if (submitButton != null) {
                submitButton.onClick.RemoveListener(OnSubmitClicked);
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Plays the default BGM if AudioManager is available
        /// </summary>
        private void PlayLoginBGM() {
            if (AudioManager.Instance != null) {
                AudioManager.Instance.PlayBGM();
            } else {
                Debug.LogWarning($"[{nameof(LoginUI)}] AudioManager.Instance is null. Cannot play BGM.");
            }
        }

        /// <summary>
        /// Handles the submit button click: disables button, plays SFX, loads next scene
        /// </summary>
        private void OnSubmitClicked() {
            // Disable the button to prevent double-clicks
            if (submitButton != null) {
                submitButton.interactable = false;
            }

            // Play click SFX
            if (AudioManager.Instance != null) {
                AudioManager.Instance.PlaySFX("button_click");
            }

            // Load the next scene
            if (SceneController.Instance != null) {
                SceneController.Instance.LoadNextScene();
            } else {
                Debug.LogError($"[{nameof(LoginUI)}] SceneController.Instance is null. Cannot load next scene.");
                if (submitButton != null) submitButton.interactable = true; // Re-enable on failure
            }
        }
        #endregion
    }
}