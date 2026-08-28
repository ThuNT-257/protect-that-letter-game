using System.Collections;
using UnityEngine;

public class ObstacleSpawner : MonoBehaviour {
    #region Serialized Fields
    [Header("References")]
    [SerializeField] private Camera mainCamera;

    [Header("Settings")]
    [SerializeField] private float spawnPadding = 0.5f;
    [SerializeField] private float referenceWorldWidth = 10f;
    #endregion

    #region Private Fields
    private float minX, maxX, spawnY;
    private bool isSpawning = false;
    private Coroutine spawnCoroutine;
    private LevelConfig currentConfig;
    private float dynamicSpawnInterval;
    #endregion

    #region Lifecycle
    private void Awake() {
        if (mainCamera == null) mainCamera = Camera.main;
        CalculateSpawnArea();
    }
    #endregion

    #region Public Methods
    public void StartLevel(LevelConfig config) {
        currentConfig = config;
        isSpawning = true;
        CalculateSpawnArea();

        if (spawnCoroutine != null) StopCoroutine(spawnCoroutine);
        spawnCoroutine = StartCoroutine(SpawnRoutine());
    }

    public void StopSpawning() {
        isSpawning = false;
        if (spawnCoroutine != null) {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
    }

    public void ResetSpawner() {
        StopSpawning();

        GameObject[] activeObstacles = GameObject.FindGameObjectsWithTag("Obstacles");
        foreach (var obstacle in activeObstacles) {
            Destroy(obstacle);
        }
    }
    #endregion

    #region Private Methods
    private void CalculateSpawnArea() {
        if (!ValidateAndGetCamera()) return;

        Vector3 topLeft = mainCamera.ViewportToWorldPoint(new Vector3(0, 1, mainCamera.nearClipPlane));
        Vector3 topRight = mainCamera.ViewportToWorldPoint(new Vector3(1, 1, mainCamera.nearClipPlane));

        minX = topLeft.x + spawnPadding;
        maxX = topRight.x - spawnPadding;
        spawnY = topLeft.y + 1f;

        float currentWorldWidth = maxX - minX;

        if (currentConfig != null && referenceWorldWidth > 0f) {
            float widthRatio = currentWorldWidth / referenceWorldWidth;
            dynamicSpawnInterval = currentConfig.spawnInterval / Mathf.Max(widthRatio, 0.5f);
        } else if (currentConfig != null) {
            dynamicSpawnInterval = currentConfig.spawnInterval;
        }
    }

    private IEnumerator SpawnRoutine() {
        while (isSpawning && currentConfig != null && currentConfig.obstaclePrefabs != null && currentConfig.obstaclePrefabs.Length > 0) {
            float interval = dynamicSpawnInterval > 0f ? dynamicSpawnInterval : currentConfig.spawnInterval;
            yield return new WaitForSeconds(interval);

            if (!isSpawning) yield break; 

            float randomX = Random.Range(minX, maxX);
            Vector3 spawnPos = new Vector3(randomX, spawnY, 0f);

            int randomIndex = Random.Range(0, currentConfig.obstaclePrefabs.Length);
            GameObject prefabToSpawn = currentConfig.obstaclePrefabs[randomIndex];

            if (prefabToSpawn != null) {
                Quaternion randomRotation = Quaternion.Euler(0, 0, Random.Range(-45f, 45f));
                Instantiate(prefabToSpawn, spawnPos, randomRotation);
            }
        }
    }

    private bool ValidateAndGetCamera() {
        if (mainCamera == null) {
            mainCamera = Camera.main;
        }

        if (mainCamera == null) {
            Debug.LogError("[ObstacleSpawner] Missing Main Camera in Scene!");
            return false;
        }

        return true;
    }
    #endregion

    #region Configuration Classes
    [System.Serializable]
    public class LevelConfig {
        public string levelName = "Level 1";
        public float duration = 10f;
        public float spawnInterval = 0.8f;
        public GameObject[] obstaclePrefabs;
    }
    #endregion
}