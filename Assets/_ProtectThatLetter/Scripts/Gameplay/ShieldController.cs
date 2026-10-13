using ProtectThatLetter.Definitions;
using ProtectThatLetter.Managers;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ProtectThatLetter.Gameplay
{
    /// <summary>
    /// Handles the player's shield: drag input, physics-based throwing,
    /// screen-bound clamping, and collision SFX. Optimized for WebGL/itch.io.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class ShieldController : MonoBehaviour
    {
        #region Serialized Fields
        [Header("References")]
        [SerializeField] private Camera mainCamera;    // Camera reference (fallback to Camera.main)
        [SerializeField] private Rigidbody2D rb;       // Rigidbody2D reference (auto-found)

        [Header("Drag & Throw Settings")]
        [SerializeField] private float throwForceMultiplier = 1.2f; // Multiplier for throw velocity
        [SerializeField] private float dragSpeed = 25f;              // Follow speed while dragging
        [SerializeField] private float maxThrowVelocity = 25f;       // Maximum throw velocity cap
        #endregion

        #region Private Fields
        private bool isDragging = false;               // True while the player is dragging
        private Vector2 offset;                        // Offset between mouse and object center
        private Vector2 lastMouseWorldPos;             // Last mouse position (for throw vector)
        private Vector2 cachedMouseWorldPos;           // Cached world position updated once per frame

        private Vector2 minBounds;                     // Bottom-left world bounds
        private Vector2 maxBounds;                     // Top-right world bounds
        private int lastScreenWidth;                   // Cached screen width (change detection)
        private int lastScreenHeight;                  // Cached screen height (change detection)

        private Vector3 initialPosition;               // Starting position (for reset)
        private Quaternion initialRotation;            // Starting rotation (for reset)
        private Collider2D shieldCollider;             // Collider reference (for bounds calculation)
        #endregion

        #region Unity Lifecycle
        /// <summary>
        /// Caches references, saves initial state, and calculates screen bounds
        /// </summary>
        private void Awake()
        {
            if (mainCamera == null) mainCamera = Camera.main;
            if (rb == null) rb = GetComponent<Rigidbody2D>();

            shieldCollider = GetComponentInChildren<Collider2D>();

            initialPosition = transform.position;
            initialRotation = transform.rotation;

            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            CalculateScreenBounds();
        }

        /// <summary>
        /// Releases drag when the application loses focus (WebGL-safe)
        /// </summary>
        private void OnApplicationFocus(bool focus)
        {
            if (!focus && isDragging)
            {
                isDragging = false;
                if (rb != null) rb.linearVelocity = Vector2.zero;
            }
        }

        /// <summary>
        /// Clears the drag state when disabled
        /// </summary>
        private void OnDisable()
        {
            isDragging = false;
        }

        /// <summary>
        /// Handles input: drag start, throw release, and tracking
        /// </summary>
        private void Update()
        {
            CheckScreenBounds();

            // Guard: no pointer available
            if (Pointer.current == null)
            {
                if (isDragging) isDragging = false;
                return;
            }

            // WebGL safety: release drag if the global press state is lost
            if (isDragging && !Pointer.current.press.isPressed)
            {
                isDragging = false;
                return;
            }

            cachedMouseWorldPos = GetMouseWorldPosition();

            // --- Handle Pointer Down ---
            if (Pointer.current.press.wasPressedThisFrame)
            {
                Collider2D hitCollider = Physics2D.OverlapPoint(cachedMouseWorldPos);

                // Only start dragging if the pointer is over this shield
                if (hitCollider != null && (hitCollider.gameObject == gameObject || hitCollider.transform.IsChildOf(transform)))
                {
                    isDragging = true;

                    offset = (Vector2)transform.position - cachedMouseWorldPos;
                    lastMouseWorldPos = cachedMouseWorldPos;

                    // Trigger game start on first interaction
                    if (GameManager.Instance != null && !GameManager.Instance.HasGameStarted)
                    {
                        GameManager.Instance.StartGame();
                    }
                }
            }

            // --- Handle Pointer Up (throw) ---
            if (Pointer.current.press.wasReleasedThisFrame && isDragging)
            {
                isDragging = false;

                // Calculate throw velocity from drag delta
                Vector2 throwVector = cachedMouseWorldPos - lastMouseWorldPos;
                Vector2 throwVelocity = throwVector * (throwForceMultiplier * 10f);

                if (rb != null)
                {
                    rb.linearVelocity = Vector2.ClampMagnitude(throwVelocity, maxThrowVelocity);
                }
            }

            // --- Track mouse position while dragging ---
            if (isDragging)
            {
                lastMouseWorldPos = cachedMouseWorldPos;
            }
        }

        /// <summary>
        /// Applies physics-based movement and screen-bound clamping
        /// </summary>
        private void FixedUpdate()
        {
            if (rb == null) return;

            if (isDragging)
            {
                // While dragging: follow the pointer
                Vector2 targetPos = cachedMouseWorldPos + offset;
                targetPos.x = Mathf.Clamp(targetPos.x, minBounds.x, maxBounds.x);
                targetPos.y = Mathf.Clamp(targetPos.y, minBounds.y, maxBounds.y);

                Vector2 velocityToTarget = (targetPos - rb.position) * dragSpeed;
                rb.linearVelocity = velocityToTarget;
            }
            else
            {
                // While free: stop velocity at bounds to prevent sticking
                Vector2 currentPos = rb.position;
                Vector2 currentVel = rb.linearVelocity;

                if (currentPos.x <= minBounds.x && currentVel.x < 0) currentVel.x = 0f;
                if (currentPos.x >= maxBounds.x && currentVel.x > 0) currentVel.x = 0f;
                if (currentPos.y <= minBounds.y && currentVel.y < 0) currentVel.y = 0f;
                if (currentPos.y >= maxBounds.y && currentVel.y > 0) currentVel.y = 0f;

                rb.linearVelocity = currentVel;

                // Clamp position
                rb.position = new Vector2(
                    Mathf.Clamp(currentPos.x, minBounds.x, maxBounds.x),
                    Mathf.Clamp(currentPos.y, minBounds.y, maxBounds.y)
                );
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Stops all movement and disables dragging
        /// </summary>
        public void FreezeShield()
        {
            isDragging = false;
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }
        }

        /// <summary>
        /// Resets the shield to its initial position, rotation, and state
        /// </summary>
        public void ResetShield()
        {
            gameObject.SetActive(true);
            FreezeShield();

            transform.position = initialPosition;
            transform.rotation = initialRotation;

            if (rb != null)
            {
                rb.position = initialPosition;
            }

            CalculateScreenBounds();
        }

        /// <summary>
        /// Freezes and hides the shield
        /// </summary>
        public void HideShield()
        {
            FreezeShield();
            gameObject.SetActive(false);
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Recalculates bounds if screen size or transform changed
        /// </summary>
        private void CheckScreenBounds()
        {
            if (Screen.width != lastScreenWidth || Screen.height != lastScreenHeight || transform.hasChanged)
            {
                lastScreenWidth = Screen.width;
                lastScreenHeight = Screen.height;
                CalculateScreenBounds();
            }
        }

        /// <summary>
        /// Calculates screen bounds in world space, accounting for the shield's collider size
        /// </summary>
        private void CalculateScreenBounds()
        {
            if (mainCamera == null) mainCamera = Camera.main;
            if (mainCamera == null) return;

            // Camera viewport in world space
            float camHeight = mainCamera.orthographicSize;
            float camWidth = camHeight * mainCamera.aspect;
            Vector3 camPos = mainCamera.transform.position;

            // Shield radius (based on collider)
            float radiusX = 0.1f;
            float radiusY = 0.1f;

            if (shieldCollider == null) shieldCollider = GetComponentInChildren<Collider2D>();

            if (shieldCollider != null)
            {
                radiusX = shieldCollider.bounds.extents.x;
                radiusY = shieldCollider.bounds.extents.y;
            }

            // Final bounds (inset by collider radius)
            minBounds = new Vector2((camPos.x - camWidth) + radiusX, (camPos.y - camHeight) + radiusY);
            maxBounds = new Vector2((camPos.x + camWidth) - radiusX, (camPos.y + camHeight) - radiusY);
        }

        /// <summary>
        /// Converts the pointer screen position to world space
        /// </summary>
        private Vector2 GetMouseWorldPosition()
        {
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
                if (mainCamera == null)
                {
                    return Vector2.zero;
                }
            }

            Vector2 pointerPos = Pointer.current.position.ReadValue();
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(new Vector3(
                pointerPos.x, pointerPos.y, Mathf.Abs(mainCamera.transform.position.z)));
            return new Vector2(worldPos.x, worldPos.y);
        }
        #endregion

        #region Unity Messages
        /// <summary>
        /// Plays hit SFX on collision with obstacles
        /// </summary>
        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag(GameDefinitions.Tags.OBSTACLES))
            {
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySFX(GameDefinitions.SfxNames.SFX_SHIELD_HIT);
                }
            }
        }
        #endregion
    }
}