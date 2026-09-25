using ProtectThatLetter.Controllers;
using ProtectThatLetter.Definitions;
using ProtectThatLetter.Managers;
using System;
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

    #region Private Fields
    private Coroutine winTransitionCoroutine; // Quản lý Coroutine Win
    #endregion

    #region Data DTOs
    [Serializable]
    private class CompleteGameRequestData {
        public string code;
    }

    [Serializable]
    public class CompleteGameResponseData {
        public int inviteId;
        public bool hasCompletedGame;
        public string guestName;
        public string guestNickname;
        public bool isLetterRead;
        public string letterContentVn;
        public string letterContentEn;
        public string imageUrl;
        public bool? isAttending;
        public string guestNote;
    }
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

        // Hủy Coroutine cũ nếu có trước khi chạy mới
        if (winTransitionCoroutine != null) {
            StopCoroutine(winTransitionCoroutine);
        }
        winTransitionCoroutine = StartCoroutine(CompleteGameAndTransitionRoutine());
    }

    public void HideTitle() {
        if (winTitleText != null) {
            winTitleText.gameObject.SetActive(false);
        }
    }

    // HÀM MỚI: Reset toàn bộ trạng thái Win khi Replay / Restart / Game Over
    public void ResetWinUI() {
        if (winTransitionCoroutine != null) {
            StopCoroutine(winTransitionCoroutine);
            winTransitionCoroutine = null;
        }
        HideTitle();
    }
    #endregion

    #region Private Methods
    private IEnumerator CompleteGameAndTransitionRoutine() {
        yield return new WaitForSeconds(displayDuration);

        string accessCode = GuestDataManager.Instance != null ? GuestDataManager.Instance.AccessCode : string.Empty;
        if (string.IsNullOrEmpty(accessCode)) {
            accessCode = PlayerPrefs.GetString("SavedAccessCode", string.Empty);
        }

        if (string.IsNullOrEmpty(accessCode)) {
            if (SceneController.Instance != null) {
                SceneController.Instance.LoadSceneByName(SceneName.LOGIN_SCENE, fadeDuration);
            }
            yield break;
        }

        if (SceneController.Instance != null) {
            SceneController.Instance.LoadNextScene(fadeDuration);
        }
    }
    #endregion
}