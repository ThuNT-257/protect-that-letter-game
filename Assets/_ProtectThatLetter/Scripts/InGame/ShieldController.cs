using UnityEngine;
using UnityEngine.InputSystem;

public class ShieldController : MonoBehaviour
{
    #region Serialized Fields
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private float throwForceMultiplier = 1f;
    [SerializeField] private float objectRadius = 0.6f;
    #endregion

    #region Private Fields
    private bool isDragging = false;
    private Vector2 offset;
    private Vector2 lastMouseWorldPos;

    private Vector2 minBounds;
    private Vector2 maxBounds;
    private int lastScreenWidth;
    private int lastScreenHeight;
    #endregion

    #region Lifecycle
    private void Awake()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }
        CheckScreenBounds();
    }

    private void Update()
    {
        CheckScreenBounds();

        if (Pointer.current == null)
        {
            return;
        }

        Vector2 currentMousePos = GetMouseWorldPosition();

        if (Pointer.current.press.wasPressedThisFrame)
        {
            Collider2D hitCollider = Physics2D.OverlapPoint(currentMousePos);
            if (hitCollider != null && hitCollider.gameObject == gameObject)
            {
                isDragging = true;
                offset = (Vector2)transform.position - currentMousePos;
                lastMouseWorldPos = currentMousePos;
                rb.linearVelocity = Vector2.zero;
            }
        }

        if (Pointer.current.press.wasReleasedThisFrame && isDragging)
        {
            isDragging = false;
            Vector2 throwDirection = (currentMousePos - lastMouseWorldPos).normalized;
            rb.linearVelocity = throwDirection * throwForceMultiplier;
        }

        if (isDragging)
        {
            lastMouseWorldPos = currentMousePos;
        }
    }

    private void FixedUpdate()
    {
        if (isDragging)
        {
            Vector2 targetPos = GetMouseWorldPosition() + offset;

            targetPos.x = Mathf.Clamp(targetPos.x, minBounds.x, maxBounds.x);
            targetPos.y = Mathf.Clamp(targetPos.y, minBounds.y, maxBounds.y);

            rb.MovePosition(targetPos);
        } 
        else
        {
            Vector2 currentPos = rb.position;

            if (currentPos.x <= minBounds.x || currentPos.x >= maxBounds.x)
            {
                currentPos.x = Mathf.Clamp(currentPos.x, minBounds.x, maxBounds.x);
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
                rb.position = currentPos;
            }

            if (currentPos.y <= minBounds.y || currentPos.y >= maxBounds.y)
            {
                currentPos.y = Mathf.Clamp(currentPos.y, minBounds.y, maxBounds.y);
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
                rb.position = currentPos;
            }
        }
    }
    #endregion

    #region Private Methods
    private void CheckScreenBounds()
    {
        if (Screen.width != lastScreenWidth || Screen.height != lastScreenHeight)
        {
            lastScreenWidth = Screen.width;
            lastScreenHeight = Screen.height;
            CalculateScreenBounds();
        }
    }

    private void CalculateScreenBounds()
    {
        Vector3 bottomLeft = mainCamera.ViewportToWorldPoint(new Vector3(0, 0, mainCamera.nearClipPlane));
        Vector3 topRight = mainCamera.ViewportToWorldPoint(new Vector3(1, 1, mainCamera.nearClipPlane));

        minBounds = new Vector2(bottomLeft.x + objectRadius, bottomLeft.y + objectRadius);
        maxBounds = new Vector2(topRight.x - objectRadius, topRight.y - objectRadius);
    }

    private Vector2 GetMouseWorldPosition()
    {
        Vector2 pointerPos = Pointer.current.position.ReadValue();
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(pointerPos);
        return new Vector2(worldPos.x, worldPos.y);
    }
    #endregion
}
