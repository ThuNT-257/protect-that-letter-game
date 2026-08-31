using UnityEngine;

public class BirdController : MonoBehaviour
{
    #region Constants
    private const string OBSTACLE_TAG = "Obstacles";
    #endregion

    #region Serialized Fields
    [Header("UI References")]
    [SerializeField] private CollisionUI collisionUI;
    [SerializeField] private GameOverUI gameOverUI;

    [Header("Settings")]
    [SerializeField] private bool pauseOnCollision = true;
    #endregion

    #region Private Fields
    private int collisionCount = 0;
    #endregion

    #region Unity Physics Events
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag(OBSTACLE_TAG))
        {
            HandleCollision();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(OBSTACLE_TAG))
        {
            HandleCollision();
        }
    }
    #endregion

    #region Public Methods
    public void ResetCollisionCount()
    {
        collisionCount = 0;
    }
    #endregion

    #region Private Methods
    private void HandleCollision()
    {
        collisionCount++;

        if (collisionCount == 1)
        {
            if (collisionUI != null)
            {
                collisionUI.ShowPanel();
            }
        }
        else if (collisionCount >= 2)
        {
            if (gameOverUI != null)
            {
                gameOverUI.ShowPanel();
            }
        }

        if (pauseOnCollision)
        {
            Time.timeScale = 0f;
        }
    }
    #endregion
}