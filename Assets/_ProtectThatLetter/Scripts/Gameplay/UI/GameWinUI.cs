using ProtectThatLetter.Managers;
using System.Collections;
using TMPro;
using UnityEngine;

public class GameWinUI : MonoBehaviour {
    #region Serialized Fields
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI winTitleText;

    [Header("Settings")]
    [SerializeField] private float displayDuration = 2.0f;
    [SerializeField] private float fadeDuration = 1.0f;
    #endregion

    #region Lifecycle
    private void Awake() {
        HideTitle();
    }
    #endregion

    #region Public Methods
    public void ShowWinAndTransition() {
        if (winTitleText != null) {
            winTitleText.gameObject.SetActive(true);
        }

        StartCoroutine(TransitionRoutine());
    }

    public void HideTitle() {
        if (winTitleText != null) {
            winTitleText.gameObject.SetActive(false);
        }
    }
    #endregion

    #region Private Methods
    private IEnumerator TransitionRoutine() {
        yield return new WaitForSeconds(displayDuration);

        if (SceneManager.Instance != null) {
            SceneManager.Instance.LoadNextScene(fadeDuration);
        }
    }
    #endregion
}