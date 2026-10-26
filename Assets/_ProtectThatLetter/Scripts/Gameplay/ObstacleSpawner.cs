using System.Collections;
using System.Collections.Generic;
using ProtectThatLetter.Definitions;
using ProtectThatLetter.Managers;
using UnityEngine;

namespace ProtectThatLetter.Gameplay
{
    /// <summary>
    /// Manages obstacle spawning logic, screen bounds calculation, and wave pacing.
    /// Interfaces directly with ObjectPool for zero-lag recycling and GameManager integration.
    /// </summary>
    [DisallowMultipleComponent]
    public class ObstacleSpawner : MonoBehaviour
    {
        public static ObstacleSpawner Instance { get; private set; }

        #region Serialized Fields
        [Header("References")]
        [SerializeField] private Camera mainCamera;         // Camera reference (fallback to Camera.main)
        [SerializeField] private Transform targetTransform; // Target for aiming obstacles

        [Header("Settings")]
        [SerializeField] private float spawnPadding = 0.5f;           // Horizontal inset from screen edge
        [SerializeField] private float referenceWorldWidth = 10f;     // Reference width for pacing scaling
        [SerializeField] private float extraTopBuffer = 2.5f;         // Extra Y offset above screen top
        #endregion

        #region Private Fields
        private float minX, maxX, screenTopY;    // Cached spawn bounds
        private bool isSpawning = false;         // True while spawning is active
        private Coroutine spawnCoroutine;        // Reference to spawn loop
        private LevelConfig currentConfig;       // Current level configuration
        private float dynamicSpawnInterval;      // Adjusted interval based on screen width

        // Active obstacles (fast lookup + cleanup)
        private readonly HashSet<GameObject> activeSpawnedObstacles = new HashSet<GameObject>();

        // Cache of prefab's Y offset (based on sprite/collider extents)
        private readonly Dictionary<GameObject, float> prefabHeightOffsetCache = new Dictionary<GameObject, float>();
        #endregion

        #region Properties
        /// <summary>
        /// Number of currently active spawned obstacles (auto-prunes inactive entries).
        /// </summary>
        public int ActiveObstacleCount
        {
            get
            {
                activeSpawnedObstacles.RemoveWhere(obj => obj == null || !obj.activeInHierarchy);
                return activeSpawnedObstacles.Count;
            }
        }
        #endregion

        #region Unity Lifecycle
        /// <summary>
        /// Caches the camera reference
        /// </summary>
        private void Awake()
        {
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }
        }

        /// <summary>
        /// Calculates spawn area bounds on scene start
        /// </summary>
        private void Start()
        {
            CalculateSpawnArea();
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Starts spawning obstacles based on the provided level configuration.
        /// </summary>
        public void StartLevel(LevelConfig config)
        {
            currentConfig = config;
            isSpawning = true;

            CalculateSpawnArea();

            // Stop any existing coroutine
            if (spawnCoroutine != null)
            {
                StopCoroutine(spawnCoroutine);
            }

            if (currentConfig.spawnType == LevelSpawnType.StaticGroup)
            {
                SpawnStaticGroup();
            }
            else
            {
                spawnCoroutine = StartCoroutine(SpawnRoutine());
            }
        }

        /// <summary>
        /// Stops all active spawning routines.
        /// </summary>
        public void StopSpawning()
        {
            isSpawning = false;
            if (spawnCoroutine != null)
            {
                StopCoroutine(spawnCoroutine);
                spawnCoroutine = null;
            }
        }

        /// <summary>
        /// Resets the spawner state and clears active objects.
        /// </summary>
        public void ResetSpawner()
        {
            StopSpawning();
            ClearAllActiveObstacles();
        }

        /// <summary>
        /// Returns the collection of currently active obstacles for GameManager validation.
        /// </summary>
        public HashSet<GameObject> GetActiveObstacles()
        {
            activeSpawnedObstacles.RemoveWhere(obj => obj == null || !obj.activeInHierarchy);
            return activeSpawnedObstacles;
        }

        /// <summary>
        /// Clears all active obstacles by returning them to the pool.
        /// </summary>
        public void ClearAllActiveObstacles()
        {
            StopSpawning();

            // Copy to list to avoid mutation during iteration
            List<GameObject> activeList = new List<GameObject>(activeSpawnedObstacles);
            foreach (var obstacle in activeList)
            {
                if (obstacle != null && obstacle.activeSelf)
                {
                    ObjectPool.Instance.ReturnToPool(obstacle);
                }
            }
            activeSpawnedObstacles.Clear();
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Calculates spawn bounds based on camera view and dynamic interval scaling.
        /// </summary>
        private void CalculateSpawnArea()
        {
            if (mainCamera == null) mainCamera = Camera.main;
            if (mainCamera == null) return;

            if (mainCamera.orthographic)
            {
                // Orthographic camera bounds
                float cameraVertExtent = mainCamera.orthographicSize;
                float cameraHorizExtent = cameraVertExtent * mainCamera.aspect;

                Vector3 camPos = mainCamera.transform.position;
                minX = (camPos.x - cameraHorizExtent) + spawnPadding;
                maxX = (camPos.x + cameraHorizExtent) - spawnPadding;
                screenTopY = camPos.y + cameraVertExtent;
            }
            else
            {
                // Perspective camera bounds at z = 0
                float distanceToZ0 = Mathf.Abs(mainCamera.transform.position.z);
                Vector3 topLeft = mainCamera.ViewportToWorldPoint(new Vector3(0, 1, distanceToZ0));
                Vector3 topRight = mainCamera.ViewportToWorldPoint(new Vector3(1, 1, distanceToZ0));

                minX = topLeft.x + spawnPadding;
                maxX = topRight.x - spawnPadding;
                screenTopY = topLeft.y;
            }

            // Adjust spawn interval based on screen width
            float currentWorldWidth = maxX - minX;

            if (currentConfig != null && referenceWorldWidth > 0f)
            {
                float widthRatio = currentWorldWidth / referenceWorldWidth;
                float intervalMultiplier = Mathf.Clamp(1f / widthRatio, 0.5f, 2.0f);
                dynamicSpawnInterval = currentConfig.spawnInterval * intervalMultiplier;
            }
            else if (currentConfig != null)
            {
                dynamicSpawnInterval = currentConfig.spawnInterval;
            }
        }

        /// <summary>
        /// Calculates the Y spawn position above the screen, accounting for prefab height.
        /// </summary>
        private float GetSpawnYForPrefab(GameObject prefab)
        {
            // Return cached offset if available
            if (prefabHeightOffsetCache.TryGetValue(prefab, out float cachedOffset))
            {
                return screenTopY + cachedOffset + extraTopBuffer;
            }

            // Calculate offset from sprite or collider extents
            float objectHeightOffset = 0f;
            SpriteRenderer sr = prefab.GetComponentInChildren<SpriteRenderer>();
            Collider2D col = prefab.GetComponentInChildren<Collider2D>();

            if (sr != null)
            {
                objectHeightOffset = sr.bounds.extents.y;
            }
            else if (col != null)
            {
                objectHeightOffset = col.bounds.extents.y;
            }

            prefabHeightOffsetCache[prefab] = objectHeightOffset;
            return screenTopY + objectHeightOffset + extraTopBuffer;
        }

        /// <summary>
        /// Spawns a static group of obstacles (single prefab at center top).
        /// </summary>
        private void SpawnStaticGroup()
        {
            if (currentConfig.staticGroupPrefab == null) return;

            float spawnY = GetSpawnYForPrefab(currentConfig.staticGroupPrefab);
            Vector3 spawnPos = new Vector3(0f, spawnY, 0f);

            GameObject spawnedObj = ObjectPool.Instance.GetPooledObject(
                currentConfig.staticGroupPrefab, spawnPos, Quaternion.identity);

            if (spawnedObj != null)
            {
                activeSpawnedObstacles.Add(spawnedObj);
            }

            // Initialize group component if present
            ObstacleGroup groupComponent = spawnedObj.GetComponent<ObstacleGroup>();
            if (groupComponent != null)
            {
                groupComponent.InitGroup(2f);
            }
        }

        /// <summary>
        /// Registers a spawned obstacle and initializes it with its data.
        /// </summary>
        private void RegisterAndInitializeObstacle(GameObject spawnedObj)
        {
            if (spawnedObj == null) return;

            activeSpawnedObstacles.Add(spawnedObj);

            Obstacle obstacleComponent = spawnedObj.GetComponent<Obstacle>();
            if (obstacleComponent != null)
            {
                obstacleComponent.Initialize(obstacleComponent.Data, targetTransform);
            }
        }
        #endregion

        #region Coroutines
        /// <summary>
        /// Main spawn loop: spawns random obstacles at dynamic intervals.
        /// </summary>
        private IEnumerator SpawnRoutine()
        {
            float elapsedTime = 0f;
            float levelDuration = currentConfig.duration > 0 ? currentConfig.duration : float.MaxValue;

            while (isSpawning && elapsedTime < levelDuration)
            {
                // Guard: no prefabs configured
                if (currentConfig.obstaclePrefabs == null || currentConfig.obstaclePrefabs.Length == 0) yield break;

                // Wait for the dynamic interval
                float interval = dynamicSpawnInterval > 0f ? dynamicSpawnInterval : currentConfig.spawnInterval;
                yield return new WaitForSeconds(interval);
                elapsedTime += interval;

                if (!isSpawning) yield break;

                // Pick a random prefab
                int randomIndex = Random.Range(0, currentConfig.obstaclePrefabs.Length);
                GameObject prefabToSpawn = currentConfig.obstaclePrefabs[randomIndex];

                if (prefabToSpawn != null)
                {
                    float spawnY = GetSpawnYForPrefab(prefabToSpawn);
                    float randomX;
                    float roll = Random.value;

                    // Weighted X distribution (center-weighted)
                    float midLeftBound = minX + (maxX - minX) * 0.25f;
                    float midRightBound = maxX - (maxX - minX) * 0.25f;

                    if (roll < 0.5f)
                    {
                        randomX = Random.Range(midLeftBound, midRightBound);  // 50% center
                    }
                    else if (roll < 0.75f)
                    {
                        randomX = Random.Range(minX, midLeftBound);           // 25% left
                    }
                    else
                    {
                        randomX = Random.Range(midRightBound, maxX);          // 25% right
                    }

                    // Spawn position and random rotation
                    Vector3 spawnPos = new Vector3(randomX, spawnY, 0f);
                    Quaternion randomRotation = Quaternion.Euler(0, 0, Random.Range(-45f, 45f));

                    GameObject spawnedObj = ObjectPool.Instance.GetPooledObject(prefabToSpawn, spawnPos, randomRotation);
                    RegisterAndInitializeObstacle(spawnedObj);
                }
            }

            isSpawning = false;
        }
        #endregion
    }
}

#region Configuration Classes
/// <summary>
/// Defines the type of spawn behavior for a level.
/// </summary>
public enum LevelSpawnType
{
    DynamicLoop,  // Continuous spawning at intervals
    StaticGroup   // Single group spawned once
}

/// <summary>
/// Configuration for a level's obstacle spawning.
/// </summary>
[System.Serializable]
public class LevelConfig
{
    public string levelName = "Level 0";                              // Level display name
    public LevelSpawnType spawnType = LevelSpawnType.DynamicLoop;     // Spawn behavior type
    public float duration = 10f;                                      // Level duration (0 = infinite)

    public GameObject staticGroupPrefab;                              // Prefab for StaticGroup mode

    public float spawnInterval = 0.8f;                                // Time between spawns
    public GameObject[] obstaclePrefabs;                              // Obstacle prefabs to spawn
}
#endregion