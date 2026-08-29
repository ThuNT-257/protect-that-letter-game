using System.Collections;
using UnityEngine;

/// <summary>
/// Manages the initialization/splash screen sequence including fade effects and data loading
/// </summary>
public class InitManager : MonoBehaviour 
{
    #region Serialized Fields
    [Header("Time Settings")]
    [SerializeField] private float fadeInDuration = 1.0f;
    [SerializeField] private float minimumDisplayTime = 1.5f;
    [SerializeField] private float fadeOutDuration = 1.0f;
    #endregion

    #region Lifecycle
    /// <summary>
    /// Starts the initialization routine
    /// </summary>
    private void Start() 
    {
        StartCoroutine(InitAndSplashRoutine());
    }
    #endregion

    #region Private Methods
    /// <summary>
    /// Main initialization coroutine that handles splash sequence, data loading, and scene transition
    /// </summary>
    private IEnumerator InitAndSplashRoutine()
    {
        //track when the process started
        float startTime = Time.time;

        //1. fade in the splash screen
        if (UIFadeManager.Instance != null)
        {
            yield return StartCoroutine(UIFadeManager.Instance.FadeInRoutine(fadeInDuration));
        }

        //2. load game data and managers
        yield return StartCoroutine(LoadGameManagersAndData());

        //3. ensure minimum display time before transitioning
        float elapsedTime = Time.time - startTime;
        if (elapsedTime < minimumDisplayTime)
        {
            yield return new WaitForSeconds(minimumDisplayTime - elapsedTime);
        }

        //4. transition to the next scene
        if (UIFadeManager.Instance != null)
        {
            UIFadeManager.Instance.FadeToScene(SceneController.LOGIN_SCENE, fadeOutDuration);
        }
        else if (SceneController.Instance != null)
        {
            SceneController.Instance.LoadScene(SceneController.LOGIN_SCENE);
        }
    }

    /// <summary>
    /// Simulates loading game managers and data
    /// 
    /// TODO: Connect to database, check api blah blah
    /// </summary>
    private IEnumerator LoadGameManagersAndData() 
    {
        Debug.Log("[InitManager] Start Download...");
        yield return new WaitForSeconds(0.5f);
        Debug.Log("[InitManager] Successful!");
    }
    #endregion
}