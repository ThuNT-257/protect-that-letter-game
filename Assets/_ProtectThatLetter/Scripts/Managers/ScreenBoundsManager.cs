using System;
using UnityEngine;

namespace ProtectThatLetter.Managers
{
    /// <summary>
    /// Centralized manager for screen-space bounds in world coordinates.
    /// Auto-creates itself if missing, recalculates on resolution changes,
    /// and broadcasts <see cref="OnBoundsChanged"/> so listeners can react.
    /// </summary>
    [DisallowMultipleComponent]
    public class ScreenBoundsManager : MonoBehaviour
    {
        #region Instance
        // Singleton instance (lazily resolved via EnsureInstance)
        private static ScreenBoundsManager instance;

        public static ScreenBoundsManager Instance => instance;

        /// <summary>
        /// Ensures the singleton exists. Auto-creates a persistent instance if needed.
        /// </summary>
        public static ScreenBoundsManager EnsureInstance()
        {
            if (instance == null)
            {
                instance = FindAnyObjectByType<ScreenBoundsManager>();

                if (instance == null)
                {
                    GameObject go = new GameObject("[Auto-Created] ScreenBoundsManager");
                    instance = go.AddComponent<ScreenBoundsManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return instance;
        }
        #endregion

        #region Serialized Fields
        [Header("References")]
        [SerializeField] private Camera mainCamera;               // Camera reference (fallback to Camera.main)

        [Header("Bound Settings")]
        [SerializeField] private float spawnPadding = 0.5f;       // Inset from screen edges for spawning
        [SerializeField] private float despawnBuffer = 2.0f;      // Extra distance outside screen for despawning
        #endregion

        #region Properties
        // Horizontal spawn bounds (inside screen)
        public float MinX { get; private set; }
        public float MaxX { get; private set; }

        // Top of the screen in world space (for spawn Y)
        public float ScreenTopY { get; private set; }

        // Despawn bounds (outside screen)
        public float DespawnBottomY { get; private set; }
        public float DespawnSideX { get; private set; }
        #endregion

        #region Events
        /// <summary>
        /// Fired when the screen bounds change (resolution or ortho size).
        /// </summary>
        public event Action OnBoundsChanged;
        #endregion

        #region Private Fields
        private int lastScreenWidth;             // Cached width (change detection)
        private int lastScreenHeight;            // Cached height (change detection)
        private float lastOrthoSize;             // Cached orthographic size (change detection)
        #endregion

        #region Unity Lifecycle
        /// <summary>
        /// Ensures singleton integrity and calculates initial bounds
        /// </summary>
        private void Awake()
        {
            // Destroy duplicates
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);

            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }

            CalculateBounds();
        }

        /// <summary>
        /// Recalculates bounds when screen size or camera zoom changes
        /// </summary>
        private void Update()
        {
            if (Screen.width != lastScreenWidth
                || Screen.height != lastScreenHeight
                || (mainCamera != null && mainCamera.orthographicSize != lastOrthoSize))
            {
                CalculateBounds();
                OnBoundsChanged?.Invoke();
            }
        }

        /// <summary>
        /// Clears the singleton reference when destroyed
        /// </summary>
        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Recalculates all bounds from the current camera state.
        /// </summary>
        private void CalculateBounds()
        {
            if (mainCamera == null) mainCamera = Camera.main;
            if (mainCamera == null) return;

            // Cache current state for change detection
            lastScreenWidth = Screen.width;
            lastScreenHeight = Screen.height;
            lastOrthoSize = mainCamera.orthographicSize;

            // Only handle orthographic cameras (extend for perspective if needed)
            if (mainCamera.orthographic)
            {
                float vertExtent = mainCamera.orthographicSize;
                float horizExtent = vertExtent * mainCamera.aspect;
                Vector3 camPos = mainCamera.transform.position;

                // Spawn bounds (inside screen)
                MinX = (camPos.x - horizExtent) + spawnPadding;
                MaxX = (camPos.x + horizExtent) - spawnPadding;
                ScreenTopY = camPos.y + vertExtent;

                // Despawn bounds (outside screen)
                DespawnBottomY = camPos.y - vertExtent - despawnBuffer;
                DespawnSideX = horizExtent + despawnBuffer;
            }
        }
        #endregion
    }
}