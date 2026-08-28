using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages scene navigation throughout the application.
/// </summary>
public class SceneController : MonoBehaviour
{
    #region Constants
    public const string LOGIN_SCENE = "LoginScene";
    public const string STORY_SCENE = "StoryScene";
    public const string PLAY_SCENE = "PlayScene";
    #endregion

    #region Instance
    private static SceneController instance;

    public static SceneController Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindAnyObjectByType<SceneController>();
                if (instance == null)
                {
                    Debug.LogError("There is no SceneController in Scene.");
                }
            }
            return instance;
        }
    }
    #endregion

    #region Lifecycle
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Loads a scene asynchronously by name.
    /// </summary>
    /// <param name="sceneName">The name of the scene to load</param>
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadSceneAsync(sceneName);
    }

    /// <summary>
    /// Reloads the currently active scene asynchronously.
    /// </summary>
    public void ReloadCurrentScene() {
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadSceneAsync(currentSceneName);
    }
    #endregion
}
