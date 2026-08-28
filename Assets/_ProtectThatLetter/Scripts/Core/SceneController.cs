using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages scene navigation throughout the application.
/// </summary>
public class SceneController : MonoBehaviour {
    #region Constants
    public const string INIT_SCENE = "InitScene";
    public const string LOGIN_SCENE = "LoginScene";
    public const string STORY_SCENE = "StoryScene";
    public const string PLAY_SCENE = "PlayScene";
    #endregion

    #region Instance
    public static SceneController Instance { get; private set; }
    #endregion

    #region Lifecycle
    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Loads a scene by name. Calls UIFadeManager if available for smooth transitions.
    /// </summary>
    public void LoadScene(string sceneName, float fadeDuration = -1f) {
        if (UIFadeManager.Instance != null) {
            UIFadeManager.Instance.FadeToScene(sceneName, fadeDuration);
        } else {
            StartCoroutine(LoadSceneAsyncCoroutine(sceneName));
        }
    }

    /// <summary>
    /// Fallback coroutine to load scene directly if FadeManager is missing
    /// </summary>
    private IEnumerator LoadSceneAsyncCoroutine(string sceneName) {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);

        while (!asyncLoad.isDone) {
            yield return null;
        }
    }
    #endregion
}