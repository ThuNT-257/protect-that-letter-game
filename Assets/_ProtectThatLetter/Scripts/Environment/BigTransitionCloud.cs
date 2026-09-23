using UnityEngine;

public class BigTransitionCloud : MonoBehaviour {
    #region Serialized Fields
    [SerializeField] private float moveSpeed = 3.5f;
    [SerializeField] private float widthPadding = 1.1f;

    [Header("Child References")]
    [SerializeField] private SpriteRenderer cloudRenderer;
    [SerializeField] private SpriteRenderer maskRenderer;
    #endregion

    #region Private Fields
    private LevelColorData targetPalette;
    private LevelBackgroundManager bgManager;
    private bool isMoving = false;
    private bool hasTriggeredColorChange = false;

    private float startY;
    private float calculatedDestroyY;
    private float triggerColorY;
    #endregion

    #region Public Methods
    public void InitTransition(LevelColorData newPalette, LevelBackgroundManager manager, float speed) {
        targetPalette = newPalette;
        bgManager = manager;
        moveSpeed = speed;

        FitAndAlignElements();

        isMoving = true;
    }
    #endregion

    #region Unity Lifecycle
    private void Update() {
        if (!isMoving) return;

        transform.Translate(Vector3.down * moveSpeed * Time.deltaTime, Space.World);

        // Kích hoạt đổi màu background ẩn bên dưới khi Cloud vừa trôi qua màn hình (Mask che kín)
        if (!hasTriggeredColorChange && transform.position.y <= triggerColorY) {
            hasTriggeredColorChange = true;
            if (bgManager != null) {
                bgManager.OnTransitionComplete(targetPalette);
            }
        }

        // Tự hủy prefab khi toàn bộ cụm Cloud + Mask trôi hết khỏi đáy màn hình
        if (transform.position.y <= calculatedDestroyY) {
            Destroy(gameObject);
        }
    }
    #endregion

    #region Private Methods
    private void FitAndAlignElements() {
        Camera mainCam = Camera.main;
        if (mainCam == null || !mainCam.orthographic) return;

        float camHeight = mainCam.orthographicSize * 2f;
        float camWidth = camHeight * mainCam.aspect;

        if (cloudRenderer == null) cloudRenderer = GetComponentInChildren<SpriteRenderer>();

        // 1. Scale Đám Mây giữ nguyên tỷ lệ aspect ratio
        if (cloudRenderer != null && cloudRenderer.sprite != null) {
            float cloudSpriteWidth = cloudRenderer.sprite.rect.width / cloudRenderer.sprite.pixelsPerUnit;
            if (cloudSpriteWidth > 0) {
                float cloudScale = (camWidth / cloudSpriteWidth) * widthPadding;
                cloudRenderer.transform.localScale = new Vector3(cloudScale, cloudScale, 1f);
                cloudRenderer.transform.localPosition = Vector3.zero;
            }
        }

        // 2. Scale và nối Mask ngay sát phía trên Đám Mây
        if (maskRenderer != null && maskRenderer.sprite != null) {
            float maskSpriteWidth = maskRenderer.sprite.rect.width / maskRenderer.sprite.pixelsPerUnit;
            float maskSpriteHeight = maskRenderer.sprite.rect.height / maskRenderer.sprite.pixelsPerUnit;

            if (maskSpriteWidth > 0 && maskSpriteHeight > 0) {
                float maskScaleX = (camWidth / maskSpriteWidth) * widthPadding;
                float maskScaleY = (camHeight * 1.2f) / maskSpriteHeight;

                maskRenderer.transform.localScale = new Vector3(maskScaleX, maskScaleY, 1f);

                float cloudHalfHeight = cloudRenderer != null ? cloudRenderer.bounds.extents.y : 0f;
                float maskHalfHeight = maskRenderer.bounds.extents.y;

                maskRenderer.transform.localPosition = new Vector3(0f, maskHalfHeight + cloudHalfHeight, 0f);
            }
        }

        // 3. Tính toán các điểm tọa độ Y chính xác theo Bounds
        float camTopY = mainCam.transform.position.y + mainCam.orthographicSize;
        float camBottomY = mainCam.transform.position.y - mainCam.orthographicSize;
        float cloudHalfHeightExt = cloudRenderer != null ? cloudRenderer.bounds.extents.y : 0f;
        float totalHeight = GetTotalBoundsHeight();

        // Mép dưới Cloud nằm hoàn toàn ngoài phía trên Camera
        startY = camTopY + cloudHalfHeightExt + 0.2f;
        transform.position = new Vector3(mainCam.transform.position.x, startY, 0f);

        // Đổi màu khi Cloud vừa trôi qua mép dưới Camera
        triggerColorY = camBottomY - cloudHalfHeightExt;

        // Điểm tự hủy khi cả cụm trôi hết hoàn toàn
        calculatedDestroyY = camBottomY - totalHeight - 0.5f;
    }

    private float GetTotalBoundsHeight() {
        float height = 0f;
        if (cloudRenderer != null) height += cloudRenderer.bounds.size.y;
        if (maskRenderer != null) height += maskRenderer.bounds.size.y;
        return height > 0 ? height : 15f;
    }
    #endregion
}