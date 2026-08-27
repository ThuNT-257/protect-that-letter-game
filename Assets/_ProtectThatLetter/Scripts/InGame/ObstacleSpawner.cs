using System.Collections;
using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    #region Serialized Fields
    [Header("References")]
    [SerializeField] private Camera mainCamera;

    [Header("Settings")]
    [SerializeField] private float spawnPadding = 0.5f;
    #endregion

    #region Private Fields
    private float minX, maxX, spawnY;
    private bool isSpawning = false;
    private Coroutine spawnCoroutine;
    private LevelConfig currentConfig;
    #endregion

    #region Lifecycle
    private void Awake()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        CalculateSpawnArea();
    }
    #endregion

    #region Public Methods
    public void StartLevel(LevelConfig config)
    {
        currentConfig = config;
        isSpawning = true;

        if (spawnCoroutine != null) StopCoroutine(spawnCoroutine);
        spawnCoroutine = StartCoroutine(SpawnRoutine());
    }

    public void StopSpawning()
    {
        isSpawning = false;
        if (spawnCoroutine != null) StopCoroutine(spawnCoroutine);
    }
    #endregion

    #region Private Methods
    private void CalculateSpawnArea()
    {
        Vector3 topLeft = mainCamera.ViewportToWorldPoint(new Vector3(0, 1, mainCamera.nearClipPlane));
        Vector3 topRight = mainCamera.ViewportToWorldPoint(new Vector3(1, 1, mainCamera.nearClipPlane));

        minX = topLeft.x + spawnPadding;
        maxX = topRight.x - spawnPadding;
        spawnY = topLeft.y + 1f;
    }

    private IEnumerator SpawnRoutine()
    {
        while (isSpawning && currentConfig != null && currentConfig.obstaclePrefabs.Length > 0)
        {
            yield return new WaitForSeconds(currentConfig.spawnInterval);

            float randomX = Random.Range(minX, maxX);
            Vector3 spawnPos = new Vector3(randomX, spawnY, 0f);

            int randomIndex = Random.Range(0, currentConfig.obstaclePrefabs.Length);
            GameObject prefabToSpawn = currentConfig.obstaclePrefabs[randomIndex];

            Quaternion randomRotation = Quaternion.Euler(0, 0, Random.Range(-45f, 45f));
            Instantiate(prefabToSpawn, spawnPos, randomRotation);
        }
    }
    #endregion

    #region Configuration Classes
    [System.Serializable]
    public class LevelConfig
    {
        public string levelName = "Level 1";
        public float duration = 10f;
        public float spawnInterval = 0.8f;
        public GameObject[] obstaclePrefabs;
    }
    #endregion
}
