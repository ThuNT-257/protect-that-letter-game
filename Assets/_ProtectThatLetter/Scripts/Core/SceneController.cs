using System.Collections;
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
    public static SceneController Instance { get; private set; }
    #endregion

    #region Lifecycle
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Loads the login scene after one frame to ensure all systems are initialized
    /// </summary>
    private IEnumerator Start()
    {
        yield return null;

        LoadScene(LOGIN_SCENE);
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Loads a scene asynchronously by name
    /// </summary>
    /// <param name="sceneName">Name of the scene to load</param>
    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadSceneAsyncCoroutine(sceneName));
    }

    /// <summary>
    /// Coroutine that loads a scene asynchronously and waits for completion
    /// </summary>
    /// <param name="sceneName">Name of the scene to load</param>
    private IEnumerator LoadSceneAsyncCoroutine(string sceneName)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);

        //wait until the scene is fully loaded
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        //extra frame for any post-load initialization
        yield return null;
    }
    #endregion
}
