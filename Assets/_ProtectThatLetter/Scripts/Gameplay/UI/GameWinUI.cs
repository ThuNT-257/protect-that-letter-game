using ProtectThatLetter.Controllers;
using ProtectThatLetter.Managers;
using System.Collections;
using TMPro;
using UnityEngine;

public class GameWinUI : MonoBehaviour {
    #region Serialized Fields
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI winTitleText;

    [Header("References")]
    [SerializeField] private BirdController birdController;

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

        ShieldController shield = FindAnyObjectByType<ShieldController>();
        if (shield != null) {
            shield.HideShield();
        }

        if (birdController == null) {
            birdController = FindAnyObjectByType<BirdController>();
        }

        if (birdController != null) {
            birdController.PlayWinFlyAnimation();
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

        if (SceneController.Instance != null) {
            SceneController.Instance.LoadNextScene(fadeDuration);
        }
    }
    #endregion
}