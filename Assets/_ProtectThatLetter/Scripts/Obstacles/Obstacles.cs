using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Obstacles : MonoBehaviour {
    #region Serialized Fields
    [Header("Physics Settings")]
    [SerializeField] private float mass = 8.0f;
    [SerializeField] private float gravityScale = 1.0f;
    [SerializeField] private float linearDrag = 3.0f;
    [SerializeField] private float angularDrag = 20.0f;
    [SerializeField] private float maxAngularVelocity = 60f;

    [Header("Off-Screen Auto Destroy Settings")]
    [SerializeField] private float offScreenLimitDuration = 3.0f;
    [SerializeField] private float viewportBuffer = 0.15f;
    #endregion

    #region Private Fields
    private Rigidbody2D rb;
    private Camera mainCamera;
    private float offScreenTimer = 0f;

    private Vector3 initialLocalPosition;
    private Quaternion initialLocalRotation;
    private bool hasStoredInitialTransform = false;
    private bool isChildObject = false;
    #endregion

    #region Lifecycle
    private void Awake() {
        rb = GetComponent<Rigidbody2D>();
        mainCamera = Camera.main;

        if (transform.parent != null && transform.parent.GetComponent<ObstacleSpawner>() == null) {
            isChildObject = true;
            if (!hasStoredInitialTransform) {
                initialLocalPosition = transform.localPosition;
                initialLocalRotation = transform.localRotation;
                hasStoredInitialTransform = true;
            }
        }

        SetupPhysics();
    }

    private void FixedUpdate() {
        LimitAngularVelocity();
    }

    private void Update() {
        CheckOffScreenStatus();
    }
    #endregion

    #region Public Methods
    public void OnSpawned() {
        if (rb == null) rb = GetComponent<Rigidbody2D>();

        if (isChildObject && hasStoredInitialTransform) {
            transform.localPosition = initialLocalPosition;
            transform.localRotation = initialLocalRotation;
        }

        if (rb != null) {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        offScreenTimer = 0f;
        SetupPhysics();
    }
    #endregion

    #region Private Methods
    private void SetupPhysics() {
        if (rb == null) return;

        rb.mass = mass;
        rb.gravityScale = gravityScale;
        rb.angularDamping = angularDrag;
        rb.linearDamping = linearDrag;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    private void LimitAngularVelocity() {
        if (rb == null) return;

        if (Mathf.Abs(rb.angularVelocity) > maxAngularVelocity) {
            rb.angularVelocity = Mathf.Sign(rb.angularVelocity) * maxAngularVelocity;
        }
    }

    private void CheckOffScreenStatus() {
        if (mainCamera == null) mainCamera = Camera.main;
        if (mainCamera == null) return;

        Vector3 viewportPos = mainCamera.WorldToViewportPoint(transform.position);

        bool currentlyOffScreen = viewportPos.x < -viewportBuffer || viewportPos.x > 1f + viewportBuffer ||
                                  viewportPos.y < -viewportBuffer || viewportPos.y > 1f + viewportBuffer;

        if (currentlyOffScreen) {
            offScreenTimer += Time.deltaTime;
            if (offScreenTimer >= offScreenLimitDuration) {
                Despawn();
            }
        } else {
            offScreenTimer = 0f;
        }
    }

    private void Despawn() {
        gameObject.SetActive(false);

        if (transform.parent != null) {
            bool hasActiveSibling = false;
            foreach (Transform child in transform.parent) {
                if (child.gameObject.activeSelf) {
                    hasActiveSibling = true;
                    break;
                }
            }
            if (!hasActiveSibling) {
                transform.parent.gameObject.SetActive(false);
            }
        }
    }
    #endregion
}