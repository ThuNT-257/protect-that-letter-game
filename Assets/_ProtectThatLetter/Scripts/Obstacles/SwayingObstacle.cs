using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class SwayingObstacle : MonoBehaviour
{
    #region Serialized Fields
    [Header("Components")]
    [SerializeField] private Rigidbody2D rb;

    [Header("Sway Settings")]
    [SerializeField] private float swaySpeed = 3f;
    [SerializeField] private float swayWidth = 1.5f;
    #endregion

    #region Private Fields
    private float randomOffset;
    private float destroyYBoundary;
    #endregion

    #region Lifecycle
    private void Awake()
    {
        if(rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }

        randomOffset = Random.Range(0f, 100f);

        if(Camera.main != null)
        {
            destroyYBoundary = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, Camera.main.nearClipPlane)).y - 2f;
        }
    }

    private void FixedUpdate()
    {
        float horizontalSway = Mathf.Sin((Time.time + randomOffset) * swaySpeed) * swayWidth;

        rb.linearVelocity = new Vector2(horizontalSway, rb.linearVelocity.y);
    }

    private void Update()
    {
        if(transform.position.y < destroyYBoundary)
        {
            Destroy(gameObject);
        }
    }
    #endregion
}