using ProtectThatLetter.Managers;
using System.Collections;
using UnityEngine;

public class BirdController : MonoBehaviour {
    #region Constants
    private const string OBSTACLE_TAG = "Obstacles";
    #endregion

    #region Serialized Fields
    [Header("UI References")]
    [SerializeField] private CollisionUI collisionUI;
    [SerializeField] private GameOverUI gameOverUI;

    [Header("Settings")]
    [SerializeField] private bool pauseOnCollision = true;

    [Header("Win Animation Settings")]
    [SerializeField] private float flyUpSpeed = 6f;
    [SerializeField] private Vector3 winTargetScale = new Vector3(1.3f, 1.3f, 1f);
    [SerializeField] private float scaleDuration = 0.5f;
    #endregion

    #region Private Fields
    private int collisionCount = 0;
    private bool isFlyingUp = false;
    private bool isHandlingCollision = false;
    #endregion

    private void Update() {
        if (isFlyingUp) {
            transform.Translate(Vector3.up * flyUpSpeed * Time.deltaTime, Space.World);
        }
    }

    #region Unity Physics Events
    private void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.CompareTag(OBSTACLE_TAG)) {
            HandleCollision();
        }
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag(OBSTACLE_TAG)) {
            HandleCollision();
        }
    }
    #endregion

    #region Public Methods
    public void ResetCollisionCount() {
        collisionCount = 0;
        isFlyingUp = false;
        isHandlingCollision = false;
    }

    public void PlayWinFlyAnimation() {
        StartCoroutine(WinFlyRoutine());
    }
    #endregion

    #region Private Methods
    private void HandleCollision() {
        if (isHandlingCollision || Time.timeScale == 0f) return;
        isHandlingCollision = true;

        bool hasDoneQuiz = (GameManager.Instance != null) && GameManager.Instance.HasCompletedQuiz;

        if (!hasDoneQuiz && collisionCount == 0) {
            collisionCount = 1;

            if (CollisionUI.Instance != null) {
                CollisionUI.Instance.ShowPanel();
            } else if (collisionUI != null) {
                collisionUI.ShowPanel();
            } else {
                CollisionUI ui = FindFirstObjectByType<CollisionUI>(FindObjectsInactive.Include);
                if (ui != null) ui.ShowPanel();
            }
        } else {
            collisionCount = 2;

            if (GameManager.Instance != null) {
                GameManager.Instance.GameOver();
            }

            if (gameOverUI != null) {
                gameOverUI.ShowPanel();
            } else {
                GameOverUI ui = FindFirstObjectByType<GameOverUI>(FindObjectsInactive.Include);
                if (ui != null) ui.ShowPanel();
            }
        }

        if (pauseOnCollision) {
            Time.timeScale = 0f;
        }

        isHandlingCollision = false;
    }

    private IEnumerator WinFlyRoutine() {
        if (AudioManager.Instance != null) {
            AudioManager.Instance.StopLoopSFX();
            AudioManager.Instance.PlaySFX("fat_dove_take_off");
        }
        Vector3 initialScale = transform.localScale;
        float timer = 0f;

        while (timer < scaleDuration) {
            timer += Time.deltaTime;
            transform.localScale = Vector3.Lerp(initialScale, winTargetScale, timer / scaleDuration);
            yield return null;
        }

        isFlyingUp = true;
    }
    #endregion
}