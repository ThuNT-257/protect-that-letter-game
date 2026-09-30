using ProtectThatLetter.Managers;
using ProtectThatLetter.UI;
using UnityEngine;
using ProtectThatLetter.Definitions;

namespace ProtectThatLetter.Controllers {
    /// <summary>
    /// Handles the Login scene logic: plays BGM and responds to submit requests.
    /// Decoupled from LoginUI via events.
    /// </summary>
    [DisallowMultipleComponent]
    public class LoginSceneController : MonoBehaviour {
        #region Serialized Fields
        [SerializeField] private LoginUI loginUi; // Reference to the Login UI
        #endregion

        #region Unity Lifecycle
        /// <summary>
        /// Plays the login BGM when the scene starts
        /// </summary>
        private void Start() {
            PlayLoginBGM();
        }

        /// <summary>
        /// Subscribes to the LoginUI submit event when enabled
        /// </summary>
        private void OnEnable() {
            if (loginUi != null) {
                loginUi.OnSubmitRequested += HandleSubmitRequested;
            }
        }

        /// <summary>
        /// Unsubscribes from the LoginUI submit event to prevent memory leaks
        /// </summary>
        private void OnDisable() {
            if (loginUi != null) {
                loginUi.OnSubmitRequested -= HandleSubmitRequested;
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
                Debug.LogWarning($"[{nameof(LoginSceneController)}] AudioManager.Instance is NULL");
            }
        }

        /// <summary>
        /// Handles the submit request: plays SFX and loads the next scene
        /// </summary>
        private void HandleSubmitRequested() {
            // Play click SFX
            if (AudioManager.Instance != null) {
                AudioManager.Instance.PlaySFX(GameDefinitions.SfxNames.SFX_NORMAL_BUTTON_CLICKED);
            }

            // Load the next scene
            if (SceneController.Instance != null) {
                SceneController.Instance.LoadNextScene();
            } else {
                Debug.LogWarning($"[{nameof(LoginSceneController)}] SceneController.Instance is NULL");

                // Re-enable UI on failure so the player can retry
                if (loginUi != null) {
                    loginUi.SetInputInteractable(true);
                }
            }
        }
        #endregion
    }
}