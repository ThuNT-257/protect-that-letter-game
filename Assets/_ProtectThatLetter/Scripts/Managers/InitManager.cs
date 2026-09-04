using System.Collections;
using UnityEngine;

/// <summary>
/// Manages the initialization/splash screen sequence including fade effects
/// </summary>
public class InitManager : MonoBehaviour {
    #region Serialized Fields
    [Header("Time Settings")]
    [SerializeField] private float fadeInDuration = 1.0f;
    [SerializeField] private float minimumDisplayTime = 1.5f; // Minimum time to show splash
    [SerializeField] private float fadeOutDuration = 1.0f;
    #endregion

    #region Lifecycle
    /// <summary>
    /// Starts the initialization routine when the scene loads
    /// </summary>
    private void Start() {
        StartCoroutine(InitAndSplashRoutine());
    }
    #endregion

    #region Private Methods
    /// <summary>
    /// Main initialization coroutine that handles splash sequence and scene transition
    /// </summary>
    private IEnumerator InitAndSplashRoutine() {
        float startTime = Time.time;

        // Step 1: Fade in the splash screen
        if (UIFadeManager.Instance != null) {
            yield return StartCoroutine(UIFadeManager.Instance.FadeInRoutine(fadeInDuration));
        }

        // Step 2: Load local systems/managers or offline data
        yield return StartCoroutine(LoadGameManagersAndData());

        // Step 3: Ensure minimum display time before transitioning
        float elapsedTime = Time.time - startTime;
        if (elapsedTime < minimumDisplayTime) {
            yield return new WaitForSeconds(minimumDisplayTime - elapsedTime);
        }

        // Step 4: Transition to the login scene
        if (UIFadeManager.Instance != null) {
            // Use fade manager for smooth transition
            UIFadeManager.Instance.FadeToScene(SceneController.LOGIN_SCENE, fadeOutDuration);
        } else if (SceneController.Instance != null) {
            // Fallback to direct scene loading
            SceneController.Instance.LoadScene(SceneController.LOGIN_SCENE);
        }
    }

    /// <summary>
    /// Loads local systems/managers or offline data required before entering the game
    /// </summary>
    private IEnumerator LoadGameManagersAndData() {
        Debug.Log("[InitManager] Initializing local managers and offline resources...");
        yield return null; // Placeholder for actual initialization logic
    }
    #endregion
}