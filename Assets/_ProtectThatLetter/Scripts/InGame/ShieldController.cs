using UnityEngine;
using UnityEngine.InputSystem;

namespace ProtectThatLetter.Controllers {
    [RequireComponent(typeof(Rigidbody2D))]
    public class ShieldController : MonoBehaviour {
        #region Serialized Fields
        [SerializeField] private Camera mainCamera;
        [SerializeField] private Rigidbody2D rb;
        [SerializeField] private float throwForceMultiplier = 1.2f;
        [SerializeField] private float objectRadius = 0.6f;
        [SerializeField] private float dragSpeed = 25f;
        [SerializeField] private float maxThrowVelocity = 25f;
        #endregion

        #region Private Fields
        private bool isDragging = false;
        private Vector2 offset;
        private Vector2 lastMouseWorldPos;

        private Vector2 minBounds;
        private Vector2 maxBounds;
        private int lastScreenWidth;
        private int lastScreenHeight;

        private Vector3 initialPosition;
        private Quaternion initialRotation;
        #endregion

        #region Lifecycle
        private void Awake() {
            if (mainCamera == null) {
                mainCamera = Camera.main;
            }

            if (rb == null) {
                rb = GetComponent<Rigidbody2D>();
            }

            initialPosition = transform.position;
            initialRotation = transform.rotation;

            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            CheckScreenBounds();
        }

        private void Update() {
            CheckScreenBounds();

            if (Pointer.current == null) return;

            Vector2 currentMousePos = GetMouseWorldPosition();

            // Handle Touch / Mouse Down
            if (Pointer.current.press.wasPressedThisFrame) {
                Collider2D hitCollider = Physics2D.OverlapPoint(currentMousePos);
                if (hitCollider != null && hitCollider.gameObject == gameObject) {
                    isDragging = true;

                    if (GameManager.Instance != null && !GameManager.Instance.HasGameStarted) {
                        GameManager.Instance.StartGame();
                    }
                }
            }

            // Handle Touch / Mouse Up
            if (Pointer.current.press.wasReleasedThisFrame && isDragging) {
                isDragging = false;

                Vector2 throwVector = currentMousePos - lastMouseWorldPos;

                Vector2 throwVelocity = throwVector * (throwForceMultiplier * 10f);

                rb.linearVelocity = Vector2.ClampMagnitude(throwVelocity, maxThrowVelocity);
            }

            if (isDragging) {
                lastMouseWorldPos = currentMousePos;
            }
        }

        private void FixedUpdate() {
            if (isDragging) {
                Vector2 targetPos = GetMouseWorldPosition() + offset;
                targetPos.x = Mathf.Clamp(targetPos.x, minBounds.x, maxBounds.x);
                targetPos.y = Mathf.Clamp(targetPos.y, minBounds.y, maxBounds.y);

                Vector2 velocityToTarget = (targetPos - rb.position) * dragSpeed;
                rb.linearVelocity = velocityToTarget;
            } else {
                Vector2 currentPos = rb.position;
                Vector2 currentVel = rb.linearVelocity;

                if (currentPos.x <= minBounds.x && currentVel.x < 0) currentVel.x = 0f;
                if (currentPos.x >= maxBounds.x && currentVel.x > 0) currentVel.x = 0f;
                if (currentPos.y <= minBounds.y && currentVel.y < 0) currentVel.y = 0f;
                if (currentPos.y >= maxBounds.y && currentVel.y > 0) currentVel.y = 0f;

                rb.linearVelocity = currentVel;
                rb.position = new Vector2(
                    Mathf.Clamp(currentPos.x, minBounds.x, maxBounds.x),
                    Mathf.Clamp(currentPos.y, minBounds.y, maxBounds.y)
                );
            }
        }
        #endregion

        #region Public Methods
        public void ResetShield() {
            gameObject.SetActive(true);
            isDragging = false;

            transform.position = initialPosition;
            transform.rotation = initialRotation;

            if (rb != null) {
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
                rb.position = initialPosition;
            }
        }

        public void HideShield() {
            isDragging = false;
            if (rb != null) {
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }
            gameObject.SetActive(false);
        }
        #endregion

        #region Private Methods
        private void CheckScreenBounds() {
            if (Screen.width != lastScreenWidth || Screen.height != lastScreenHeight) {
                lastScreenWidth = Screen.width;
                lastScreenHeight = Screen.height;
                CalculateScreenBounds();
            }
        }

        private void CalculateScreenBounds() {
            Vector3 bottomLeft = mainCamera.ViewportToWorldPoint(new Vector3(0, 0, mainCamera.nearClipPlane));
            Vector3 topRight = mainCamera.ViewportToWorldPoint(new Vector3(1, 1, mainCamera.nearClipPlane));

            minBounds = new Vector2(bottomLeft.x + objectRadius, bottomLeft.y + objectRadius);
            maxBounds = new Vector2(topRight.x - objectRadius, topRight.y - objectRadius);
        }

        private Vector2 GetMouseWorldPosition() {
            Vector2 pointerPos = Pointer.current.position.ReadValue();
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(pointerPos);
            return new Vector2(worldPos.x, worldPos.y);
        }

        private void OnCollisionEnter2D(Collision2D collision) {
            if (collision.gameObject.tag == "Obstacles") {
                if(AudioManager.Instance != null) {
                    AudioManager.Instance.PlaySFX("hit");
                }
            }
        }
        #endregion
    }
}