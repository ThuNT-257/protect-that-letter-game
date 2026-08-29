using UnityEngine;

public class BirdController : MonoBehaviour
{
    #region Constants
    private const string OBSTACLE_TAG = "Obstacles";
    #endregion

    #region Serialized Fields
    [Header("UI Reference")]
    [SerializeField] private CollisionUI collisionUI;

    [Header("Settings")]
    [SerializeField] private bool pauseOnCollision = true;
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

    #region Private Methods
    private void HandleCollision()
    {
        if (collisionUI != null)
        {
            collisionUI.ShowPanel();
        }

        if (pauseOnCollision)
        {
            Time.timeScale = 0f;
        }
    }
    #endregion
}
