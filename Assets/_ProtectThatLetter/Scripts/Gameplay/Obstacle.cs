using ProtectThatLetter.Definitions;
using UnityEngine;

namespace ProtectThatLetter.Gameplay
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(SpriteRenderer))]
    public class Obstacle : MonoBehaviour
    {
        [SerializeField] private ObstacleData obstacleData;

        private Rigidbody2D rb;
        private SpriteRenderer spriteRenderer;
        private Collider2D col;

        private float currentSpeed;
        private float spawnTime;
        private float startX;
        private Transform targetTransform;

        private Vector2 moveDirection;
        private bool isDirectMove = false;

        public ObstacleData Data => obstacleData;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            col = GetComponent<Collider2D>();

            rb.collisionDetectionMode = CollisionDetectionMode2D.Discrete;
        }
    }
}

