using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages UI fade transitions for scene loading and splash screens
/// </summary>
public class UIFadeManager : MonoBehaviour 
{
    #region Instance
    private static UIFadeManager instance;
    public static UIFadeManager Instance 
    {
        get 
        {
            if (instance == null) 
            {
                instance = FindAnyObjectByType<UIFadeManager>();
            }
            return instance;
        }
    }
    #endregion

    #region Serialized Fields
    [Header("UI References")]
    [SerializeField] private CanvasGroup fadeCanvasGroup;
    [SerializeField] private float defaultFadeDuration = 1.0f;
    #endregion

    #region Lifecycle
    /// <summary>
    /// Ensures singleton integrity and initializes the fade panel to fully opaque
    /// </summary>
    private void Awake() 
    {
        if (instance != null && instance != this) 
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        //start with a fully opaque black screen blocking raycasts
        if (fadeCanvasGroup != null) 
        {
            fadeCanvasGroup.alpha = 1f;
            fadeCanvasGroup.blocksRaycasts = true;
        }
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Fades the screen from opaque to transparent
    /// </summary>
    /// <param name="duration">Duration of the fade (uses default if -1)</param>
    public IEnumerator FadeInRoutine(float duration = -1f) 
    {
        float dur = duration > 0 ? duration : defaultFadeDuration;
        yield return StartCoroutine(Fade(1f, 0f, dur));
    }

    /// <summary>
    /// Fades the screen from transparent to opaque
    /// </summary>
    /// <param name="duration">Duration of the fade (uses default if -1)</param>
    public IEnumerator FadeOutRoutine(float duration = -1f) 
    {
        float dur = duration > 0 ? duration : defaultFadeDuration;
        yield return StartCoroutine(Fade(0f, 1f, dur));
    }

    /// <summary>
    /// Fades out, loads a scene, then fades in
    /// </summary>
    /// <param name="sceneName">Name of the scene to load</param>
    /// <param name="duration">Duration of the fade (uses default if -1)</param>
    public void FadeToScene(string sceneName, float duration = -1f) 
    {
        StartCoroutine(FadeToSceneRoutine(sceneName, duration));
    }
    #endregion

    #region Private Methods
    /// <summary>
    /// Coroutine that handles the full fade-to-scene transition
    /// </summary>
    private IEnumerator FadeToSceneRoutine(string sceneName, float duration) 
    {
        float dur = duration > 0 ? duration : defaultFadeDuration;

        //1. fade out (to opaque black)
        yield return StartCoroutine(Fade(0f, 1f, dur));

        //2. load scene asynchronously but prevent immediate activation
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        // wait until the scene is 90% loaded (Unity's threshold for activation)
        while (asyncLoad.progress < 0.9f) 
        {
            yield return null;
        }

        asyncLoad.allowSceneActivation = true;

        yield return null;

        //3. fade in (to transparent)
        yield return StartCoroutine(Fade(1f, 0f, dur));
    }

    /// <summary>
    /// Core fading coroutine that interpolates alpha over time
    /// </summary>
    /// <param name="startAlpha">Starting alpha value</param>
    /// <param name="endAlpha">Ending alpha value</param>
    /// <param name="duration">Duration of the fade</param>
    private IEnumerator Fade(float startAlpha, float endAlpha, float duration) 
    {
        if (fadeCanvasGroup == null) yield break;

        fadeCanvasGroup.blocksRaycasts = true;

        float timer = 0f;
        while (timer < duration) 
        {
            timer += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, timer / duration);
            yield return null;
        }

        fadeCanvasGroup.alpha = endAlpha;

        // unblock raycasts
        if (endAlpha == 0f) 
        {
            fadeCanvasGroup.blocksRaycasts = false;
        }
    }
    #endregion
}