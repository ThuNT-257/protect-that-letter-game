using ProtectThatLetter.Managers;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ProtectThatLetter.Controllers {
    [RequireComponent(typeof(Rigidbody2D))]
    public class ShieldController : MonoBehaviour {
        #region Serialized Fields
        [SerializeField] private Camera mainCamera;
        [SerializeField] private Rigidbody2D rb;
        [SerializeField] private float throwForceMultiplier = 1.2f;
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
        private Collider2D shieldCollider;
        #endregion

        #region Lifecycle
        private void Awake() {
            if (mainCamera == null) mainCamera = Camera.main;
            if (rb == null) rb = GetComponent<Rigidbody2D>();

            shieldCollider = GetComponentInChildren<Collider2D>();

            initialPosition = transform.position;
            initialRotation = transform.rotation;

            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            CalculateScreenBounds();
        }

        private void Update() {
            CheckScreenBounds();

            if (Pointer.current == null) return;

            Vector2 currentMousePos = GetMouseWorldPosition();

            // Handle Touch / Mouse Down
            if (Pointer.current.press.wasPressedThisFrame) {
                Collider2D hitCollider = Physics2D.OverlapPoint(currentMousePos);
                if (hitCollider != null && (hitCollider.gameObject == gameObject || hitCollider.transform.IsChildOf(transform))) {
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
        public void FreezeShield() {
            isDragging = false;
            if (rb != null) {
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }
        }

        public void ResetShield() {
            gameObject.SetActive(true);
            FreezeShield();

            transform.position = initialPosition;
            transform.rotation = initialRotation;

            if (rb != null) {
                rb.position = initialPosition;
            }

            CalculateScreenBounds();
        }

        public void HideShield() {
            FreezeShield();
            gameObject.SetActive(false);
        }
        #endregion

        #region Private Methods
        private void CheckScreenBounds() {
            if (Screen.width != lastScreenWidth || Screen.height != lastScreenHeight || transform.hasChanged) {
                lastScreenWidth = Screen.width;
                lastScreenHeight = Screen.height;
                CalculateScreenBounds();
            }
        }

        private void CalculateScreenBounds() {
            if (mainCamera == null) mainCamera = Camera.main;
            if (mainCamera == null) return;

            float camHeight = mainCamera.orthographicSize;
            float camWidth = camHeight * mainCamera.aspect;
            Vector3 camPos = mainCamera.transform.position;

            float radiusX = 0.1f;
            float radiusY = 0.1f;

            if (shieldCollider == null) shieldCollider = GetComponentInChildren<Collider2D>();

            if (shieldCollider != null) {
                radiusX = shieldCollider.bounds.extents.x;
                radiusY = shieldCollider.bounds.extents.y;
            }

            minBounds = new Vector2((camPos.x - camWidth) + radiusX, (camPos.y - camHeight) + radiusY);
            maxBounds = new Vector2((camPos.x + camWidth) - radiusX, (camPos.y + camHeight) - radiusY);
        }

        private Vector2 GetMouseWorldPosition() {
            Vector2 pointerPos = Pointer.current.position.ReadValue();
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(new Vector3(pointerPos.x, pointerPos.y, Mathf.Abs(mainCamera.transform.position.z)));
            return new Vector2(worldPos.x, worldPos.y);
        }

        private void OnCollisionEnter2D(Collision2D collision) {
            if (collision.gameObject.CompareTag("Obstacles")) {
                if (AudioManager.Instance != null) {
                    AudioManager.Instance.PlaySFX("hit");
                }
            }
        }
        #endregion
    }
}