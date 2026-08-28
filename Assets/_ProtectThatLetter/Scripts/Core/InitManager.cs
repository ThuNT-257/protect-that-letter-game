using System.Collections;
using UnityEngine;

public class InitManager : MonoBehaviour {
    #region Serialized Fields
    [Header("UI Components")]
    [SerializeField] private CanvasGroup logoCanvasGroup;

    [Header("Timing Settings")]
    [SerializeField] private float fadeInDuration = 1.0f;
    [SerializeField] private float minimumDisplayTime = 1.5f;
    [SerializeField] private float fadeOutDuration = 1.0f;
    #endregion

    #region Lifecycle
    private void Start() {
        if (logoCanvasGroup != null) {
            logoCanvasGroup.alpha = 1f;
        }

        StartCoroutine(InitAndSplashRoutine());
    }
    #endregion

    #region Private Methods
    private IEnumerator InitAndSplashRoutine() {
        if (UIFadeManager.Instance != null) {
            yield return StartCoroutine(UIFadeManager.Instance.FadeInRoutine(fadeInDuration));
        }

        float startTime = Time.time;

        yield return StartCoroutine(LoadGameManagersAndData());

        float elapsedTime = Time.time - startTime;
        if (elapsedTime < minimumDisplayTime) {
            yield return new WaitForSeconds(minimumDisplayTime - elapsedTime);
        }

        if (UIFadeManager.Instance != null) {
            UIFadeManager.Instance.FadeToScene(SceneController.LOGIN_SCENE, fadeOutDuration);
        }
    }

    private IEnumerator LoadGameManagersAndData() {
        Debug.Log("[InitManager] Start Download...");
        yield return new WaitForSeconds(0.5f);
        Debug.Log("[InitManager] Successful!");
    }
    #endregion
}