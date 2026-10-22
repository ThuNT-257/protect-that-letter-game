using ProtectThatLetter.Definitions;
using UnityEngine;

namespace ProtectThatLetter.Gameplay
{
    /// <summary>
    /// Runtime behavior for a single obstacle instance.
    /// Uses ObstacleData to configure movement, visuals, and collision.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(SpriteRenderer))]
    public class Obstacle : MonoBehaviour
    {
        #region Serialized Fields
        [SerializeField] private ObstacleData obstacleData; // Configuration data
        [SerializeField] private float despawnY = -6f;
        [SerializeField] private float despawnX = 10f;
        #endregion

        #region Private Fields
        private Rigidbody2D rb;                 // Cached Rigidbody2D
        private SpriteRenderer spriteRenderer;  // Cached SpriteRenderer
        private Collider2D col;                 // Cached Collider2D

        private float currentSpeed;             // Randomized fall speed
        private float spawnTime;                // Time when spawned (for sway)
        private float startX;                   // Initial X position (for sway)
        private Transform targetTransform;      // Target to aim at (Normal/Tackle)

        private Vector2 moveDirection;          // Movement direction (Normal variant)
        private bool useDirectMove = false;     // True if moving toward target
        #endregion

        #region Properties
        /// <summary>
        /// Public accessor for the obstacle's data.
        /// </summary>
        public ObstacleData Data => obstacleData;
        #endregion

        #region Unity Lifecycle
        /// <summary>
        /// Caches component references on Awake
        /// </summary>
        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            col = GetComponent<Collider2D>();

            rb.collisionDetectionMode = CollisionDetectionMode2D.Discrete;
        }

        /// <summary>
        /// Applies movement logic every physics step
        /// </summary>
        private void FixedUpdate()
        {
            if (obstacleData == null) return;

            float deltaTime = Time.fixedDeltaTime;
            Vector2 pos = rb.position;

            switch (obstacleData.obstacleType)
            {
                case ObstacleType.Normal:
                    if (useDirectMove)
                    {
                        pos += moveDirection * currentSpeed * deltaTime;
                    }
                    else
                    {
                        pos.y -= currentSpeed * deltaTime;
                    }
                    break;

                case ObstacleType.Swaying:
                    pos.y -= currentSpeed * deltaTime;
                    float timeElapsed = Time.time - spawnTime;
                    pos.x = startX + Mathf.Sin(timeElapsed * obstacleData.swayFrequency) * obstacleData.swayAmplitude;
                    break;

                case ObstacleType.Tackle:
                    if (targetTransform != null)
                    {
                        Vector2 direction = ((Vector2)targetTransform.position - pos).normalized;
                        pos += direction * obstacleData.tackleSpeed * deltaTime;
                    }
                    else
                    {
                        pos.y -= currentSpeed * deltaTime;
                    }
                    break;

                case ObstacleType.Boss:
                    pos.y -= currentSpeed * deltaTime;
                    break;
            }

            rb.MovePosition(pos);

            // Recycle if it leaves the play area
            if (transform.position.y < despawnY || Mathf.Abs(transform.position.x) > despawnX)
            {
                RecycleToPool();
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Initializes the obstacle with configuration data and a target.
        /// </summary>
        /// <param name="data">The ScriptableObject configuration</param>
        /// <param name="target">Optional target for aiming (Normal/Tackle)</param>
        public void Initialize(ObstacleData data, Transform target = null)
        {
            obstacleData = data;
            targetTransform = target;

            if (obstacleData == null) return;

            // Apply sprite
            if (spriteRenderer != null && obstacleData.obstacleSprite != null)
            {
                spriteRenderer.sprite = obstacleData.obstacleSprite;
            }

            // Apply physics material
            if (col != null && obstacleData.physicsMaterial != null)
            {
                col.sharedMaterial = obstacleData.physicsMaterial;
            }

            // Randomize speed and store spawn info
            currentSpeed = Random.Range(obstacleData.minFallSpeed, obstacleData.maxFallSpeed);
            startX = transform.position.x;
            spawnTime = Time.time;

            // Reset physics state
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.gravityScale = (obstacleData.obstacleType == ObstacleType.Tackle) ? 0f : 1f;
            useDirectMove = false;

            // Configure Normal obstacles (may aim toward target)
            if (data.obstacleType == ObstacleType.Normal)
            {
                InitializeNormalObstacleData();
            }

            gameObject.SetActive(true);
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Decides whether a Normal obstacle should move toward the target or straight down.
        /// </summary>
        private void InitializeNormalObstacleData()
        {
            float randomSpawnPos = Random.value;

            // 50% chance: aim toward target if available
            if (randomSpawnPos < 0.5f && targetTransform != null)
            {
                useDirectMove = true;
                moveDirection = ((Vector2)targetTransform.position - (Vector2)transform.position).normalized;
            }
            else
            {
                useDirectMove = false;
                moveDirection = Vector2.down;
            }
        }

        /// <summary>
        /// Returns the obstacle to the pool (deactivates it)
        /// </summary>
        private void RecycleToPool()
        {
            useDirectMove = false;
            targetTransform = null;
            gameObject.SetActive(false);
        }
        #endregion

        #region Unity Messages
        /// <summary>
        /// Recycles the obstacle if it hits the pigeon
        /// </summary>
        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag(GameDefinitions.Tags.PIGEON))
            {
                RecycleToPool();
            }
        }
        #endregion
    }
}