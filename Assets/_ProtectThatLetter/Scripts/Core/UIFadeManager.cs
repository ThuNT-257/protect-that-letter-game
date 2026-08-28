using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIFadeManager : MonoBehaviour {
    #region Instance
    private static UIFadeManager instance;
    public static UIFadeManager Instance {
        get {
            if (instance == null) {
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
    private void Awake() {
        if (instance != null && instance != this) {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        if (fadeCanvasGroup != null) {
            fadeCanvasGroup.alpha = 1f;
            fadeCanvasGroup.blocksRaycasts = true;
        }
    }
    #endregion

    #region Public Methods
    public IEnumerator FadeInRoutine(float duration = -1f) {
        float dur = duration > 0 ? duration : defaultFadeDuration;
        yield return StartCoroutine(Fade(1f, 0f, dur));
    }

    public IEnumerator FadeOutRoutine(float duration = -1f) {
        float dur = duration > 0 ? duration : defaultFadeDuration;
        yield return StartCoroutine(Fade(0f, 1f, dur));
    }

    public void FadeToScene(string sceneName, float duration = -1f) {
        StartCoroutine(FadeToSceneRoutine(sceneName, duration));
    }
    #endregion

    #region Private Methods
    private IEnumerator FadeToSceneRoutine(string sceneName, float duration) {
        float dur = duration > 0 ? duration : defaultFadeDuration;

        yield return StartCoroutine(Fade(0f, 1f, dur));

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        while (asyncLoad.progress < 0.9f) {
            yield return null;
        }

        asyncLoad.allowSceneActivation = true;

        yield return null;

        yield return StartCoroutine(Fade(1f, 0f, dur));
    }

    private IEnumerator Fade(float startAlpha, float endAlpha, float duration) {
        if (fadeCanvasGroup == null) yield break;

        fadeCanvasGroup.blocksRaycasts = true;

        float timer = 0f;
        while (timer < duration) {
            timer += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, timer / duration);
            yield return null;
        }

        fadeCanvasGroup.alpha = endAlpha;

        if (endAlpha == 0f) {
            fadeCanvasGroup.blocksRaycasts = false;
        }
    }
    #endregion
}