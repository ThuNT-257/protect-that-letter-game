using UnityEngine;

/// <summary>
/// Automatically scales and positions a background sprite to fit the screen.
/// Dynamically updates when the screen resolution or aspect ratio changes.
/// </summary>
public class BackgroundFitter : MonoBehaviour {
    [SerializeField] private float padding = 1.02f; // Slight oversize to prevent edge bleeding

    private int lastScreenWidth;   // Cached last screen width for change detection
    private int lastScreenHeight;  // Cached last screen height for change detection
    private Camera mainCam;        // Cached main camera reference
    private SpriteRenderer sr;     // Cached SpriteRenderer reference

    /// <summary>
    /// Caches references and fits the background to the screen on initialization
    /// </summary>
    private void Awake() {
        sr = GetComponent<SpriteRenderer>();
        mainCam = Camera.main;

        FitToScreen();
    }

    /// <summary>
    /// Checks for screen resolution changes and refits the background if needed
    /// </summary>
    private void Update() {
        // Only refit when the screen size actually changes
        if (Screen.width != lastScreenWidth || Screen.height != lastScreenHeight) {
            FitToScreen();
        }
    }

    /// <summary>
    /// Scales and positions the sprite to perfectly fill the screen
    /// </summary>
    public void FitToScreen() {
        // Validate references
        if (sr == null || sr.sprite == null) return;
        if (mainCam == null) mainCam = Camera.main;
        if (mainCam == null) return;

        // Cache current screen dimensions
        lastScreenWidth = Screen.width;
        lastScreenHeight = Screen.height;

        // Reset scale to get original sprite size
        transform.localScale = Vector3.one;

        // Center the background on the camera
        Vector3 camPos = mainCam.transform.position;
        transform.position = new Vector3(camPos.x, camPos.y, transform.position.z);

        // Calculate the screen size in world units
        float height = mainCam.orthographicSize * 2f; // Total height of the camera view
        float width = height * mainCam.aspect;        // Total width based on aspect ratio

        // Get the sprite's original size
        Vector2 spriteSize = sr.sprite.bounds.size;

        // Prevent division by zero
        if (spriteSize.x == 0 || spriteSize.y == 0) return;

        // Calculate scale to fit the screen (with padding to prevent edge bleeding)
        float scaleX = (width / spriteSize.x) * padding;
        float scaleY = (height / spriteSize.y) * padding;

        // Apply the scale
        transform.localScale = new Vector3(scaleX, scaleY, 1f);
    }

    /// <summary>
    /// Refits the background in the editor when values change (only during play mode)
    /// </summary>
    private void OnValidate() {
        if (Application.isPlaying && sr != null) {
            FitToScreen();
        }
    }
}