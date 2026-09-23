using UnityEngine;

[ExecuteAlways]
public class ScreenResponsiveScaler : MonoBehaviour {
    [Header("Scaling Mode")]
    [SerializeField] private bool isDynamicObject = false;

    [Header("Portrait Settings")]
    [Range(0.01f, 1f)]
    [SerializeField] private float portraitWidthPercent = 0.12f;

    [Header("Landscape Settings")]
    [Range(0.01f, 1f)]
    [SerializeField] private float landscapeWidthPercent = 0.05f;

    [Header("Scaling Base Option")]
    [SerializeField] private bool fitToHeightInLandscape = true;

    private int lastScreenWidth;
    private int lastScreenHeight;
    private bool isScaledForCurrentSpawn = false;

    private void Awake() {
        isScaledForCurrentSpawn = false;
        AdjustScale();
    }

    private void OnEnable() {
        isScaledForCurrentSpawn = false;
        AdjustScale();
    }

    private void Update() {
        if (isDynamicObject) {
            if (Screen.width != lastScreenWidth || Screen.height != lastScreenHeight) {
                AdjustScale();
            }
        }
    }

    [ContextMenu("Force Adjust Scale")]
    public void AdjustScale() {
        if (!isDynamicObject && isScaledForCurrentSpawn && Application.isPlaying) return;

        Camera mainCam = Camera.main;
        if (mainCam == null || !mainCam.orthographic) return;

        lastScreenWidth = Screen.width;
        lastScreenHeight = Screen.height;

        bool isPortrait = Screen.height > Screen.width;
        float screenAspect = (float)Screen.width / Screen.height;
        float cameraHeight = mainCam.orthographicSize * 2f;
        float cameraWidth = cameraHeight * screenAspect;

        SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null && sr.sprite != null) {
            float spriteUnscaledWidth = sr.sprite.rect.width / sr.sprite.pixelsPerUnit;
            if (spriteUnscaledWidth <= 0) return;

            float targetWorldWidth;

            if (isPortrait) {
                targetWorldWidth = cameraWidth * portraitWidthPercent;
            } else {
                if (fitToHeightInLandscape) {
                    targetWorldWidth = cameraHeight * landscapeWidthPercent;
                } else {
                    targetWorldWidth = cameraWidth * landscapeWidthPercent;
                }
            }

            float finalScale = targetWorldWidth / spriteUnscaledWidth;

            if (transform.parent != null) {
                float parentScaleX = transform.parent.lossyScale.x;
                if (parentScaleX > 0) {
                    finalScale /= parentScaleX;
                }
            }

            transform.localScale = new Vector3(finalScale, finalScale, transform.localScale.z);

            isScaledForCurrentSpawn = true;
        }
    }
}