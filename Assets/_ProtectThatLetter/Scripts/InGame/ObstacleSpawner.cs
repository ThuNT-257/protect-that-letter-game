using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleSpawner : MonoBehaviour {
    #region Serialized Fields
    [Header("References")]
    [SerializeField] private Camera mainCamera;

    [Header("Settings")]
    [SerializeField] private float spawnPadding = 0.5f;
    [SerializeField] private float referenceWorldWidth = 10f;
    [SerializeField] private float extraTopBuffer = 1.5f; 

    [Header("Pool Limits")]
    [SerializeField] private int maxPoolSizePerPrefab = 15;
    #endregion

    #region Private Fields
    private float minX, maxX, screenTopY;
    private bool isSpawning = false;
    private Coroutine spawnCoroutine;
    private LevelConfig currentConfig;
    private float dynamicSpawnInterval;

    private Dictionary<GameObject, Queue<GameObject>> poolDictionary = new Dictionary<GameObject, Queue<GameObject>>();
    private List<GameObject> activeSpawnedObstacles = new List<GameObject>();
    #endregion

    #region Lifecycle
    private void Awake() {
        if (mainCamera == null) mainCamera = Camera.main;
    }

    private void Start() {
        CalculateSpawnArea();
    }
    #endregion

    #region Public Methods
    public void StartLevel(LevelConfig config) {
        currentConfig = config;
        isSpawning = true;

        CalculateSpawnArea();

        if (spawnCoroutine != null) StopCoroutine(spawnCoroutine);

        if (currentConfig.spawnType == LevelSpawnType.StaticGroup) {
            SpawnStaticGroup();
        } else {
            spawnCoroutine = StartCoroutine(SpawnRoutine());
        }
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

        for (int i = activeSpawnedObstacles.Count - 1; i >= 0; i--) {
            if (activeSpawnedObstacles[i] != null) {
                activeSpawnedObstacles[i].SetActive(false);
            }
        }
        activeSpawnedObstacles.Clear();
    }

    public void ReturnToPool(GameObject obj) {
        if (obj == null) return;
        obj.SetActive(false);
        if (activeSpawnedObstacles.Contains(obj)) {
            activeSpawnedObstacles.Remove(obj);
        }
    }
    #endregion

    #region Private Methods
    private void CalculateSpawnArea() {
        if (!ValidateAndGetCamera()) return;

        if (mainCamera.orthographic) {
            float cameraVertExtent = mainCamera.orthographicSize;
            float cameraHorizExtent = cameraVertExtent * mainCamera.aspect;

            Vector3 camPos = mainCamera.transform.position;
            minX = (camPos.x - cameraHorizExtent) + spawnPadding;
            maxX = (camPos.x + cameraHorizExtent) - spawnPadding;

            screenTopY = camPos.y + cameraVertExtent;
        } else {
            float distanceToZ0 = Mathf.Abs(mainCamera.transform.position.z);
            Vector3 topLeft = mainCamera.ViewportToWorldPoint(new Vector3(0, 1, distanceToZ0));
            Vector3 topRight = mainCamera.ViewportToWorldPoint(new Vector3(1, 1, distanceToZ0));

            minX = topLeft.x + spawnPadding;
            maxX = topRight.x - spawnPadding;
            screenTopY = topLeft.y;
        }

        float currentWorldWidth = maxX - minX;

        if (currentConfig != null && referenceWorldWidth > 0f) {
            float widthRatio = currentWorldWidth / referenceWorldWidth;
            dynamicSpawnInterval = currentConfig.spawnInterval / Mathf.Max(widthRatio, 0.5f);
        } else if (currentConfig != null) {
            dynamicSpawnInterval = currentConfig.spawnInterval;
        }
    }

    private float GetSpawnYForPrefab(GameObject prefab) {
        float objectHeightOffset = 0f;

        SpriteRenderer sr = prefab.GetComponentInChildren<SpriteRenderer>();
        Collider2D col = prefab.GetComponentInChildren<Collider2D>();

        if (sr != null) {
            objectHeightOffset = sr.bounds.extents.y;
            Debug.Log($"[Debug Spawn] {prefab.name} Bounds from SpriteRenderer: {sr.bounds.extents.y}");
        } else if (col != null) {
            objectHeightOffset = col.bounds.extents.y;
            Debug.Log($"[Debug Spawn] {prefab.name} Bounds from Collider2D: {col.bounds.extents.y}");
        } else {
            Debug.LogWarning($"[Debug Spawn] {prefab.name} Not found SpriteRenderer or Collider2D!");
        }

        float finalY = screenTopY + objectHeightOffset + extraTopBuffer;
        Debug.Log($"[Debug Spawn] Camera Pos Y: {mainCamera.transform.position.y} | ScreenTopY: {screenTopY} | Final Spawn Y: {finalY}");

        return finalY;
    }

    private void SpawnStaticGroup() {
        if (currentConfig.staticGroupPrefab == null) return;

        float spawnY = GetSpawnYForPrefab(currentConfig.staticGroupPrefab);
        Vector3 spawnPos = new Vector3(0f, spawnY, 0f);
        GetPooledObject(currentConfig.staticGroupPrefab, spawnPos, Quaternion.identity);
    }

    private IEnumerator SpawnRoutine() {
        float elapsedTime = 0f;
        float levelDuration = currentConfig.duration > 0 ? currentConfig.duration : float.MaxValue;

        while (isSpawning && elapsedTime < levelDuration) {
            if (currentConfig.obstaclePrefabs == null || currentConfig.obstaclePrefabs.Length == 0) yield break;

            float interval = dynamicSpawnInterval > 0f ? dynamicSpawnInterval : currentConfig.spawnInterval;
            yield return new WaitForSeconds(interval);
            elapsedTime += interval;

            if (!isSpawning) yield break;

            int randomIndex = Random.Range(0, currentConfig.obstaclePrefabs.Length);
            GameObject prefabToSpawn = currentConfig.obstaclePrefabs[randomIndex];

            if (prefabToSpawn != null) {
                float spawnY = GetSpawnYForPrefab(prefabToSpawn);

                float randomX;
                float roll = Random.value; 

                float midRegionWidth = (maxX - minX) * 0.5f; 
                float midLeftBound = minX + (maxX - minX) * 0.25f;
                float midRightBound = maxX - (maxX - minX) * 0.25f; 

                if (roll < 0.5f) {
                    randomX = Random.Range(midLeftBound, midRightBound);
                } else if (roll < 0.75f) {
                    randomX = Random.Range(minX, midLeftBound);
                } else {
                    randomX = Random.Range(midRightBound, maxX);
                }

                Vector3 spawnPos = new Vector3(randomX, spawnY, 0f);
                Quaternion randomRotation = Quaternion.Euler(0, 0, Random.Range(-45f, 45f));
                GetPooledObject(prefabToSpawn, spawnPos, randomRotation);
            }
        }

        isSpawning = false;
    }

    private GameObject GetPooledObject(GameObject prefab, Vector3 position, Quaternion rotation) {
        if (!poolDictionary.ContainsKey(prefab)) {
            poolDictionary[prefab] = new Queue<GameObject>();
        }

        Queue<GameObject> poolQueue = poolDictionary[prefab];
        GameObject objToUse = null;

        int checkCount = poolQueue.Count;
        for (int i = 0; i < checkCount; i++) {
            GameObject candidate = poolQueue.Dequeue();
            if (candidate != null && !candidate.activeSelf) {
                objToUse = candidate;
                break;
            } else if (candidate != null) {
                poolQueue.Enqueue(candidate);
            }
        }

        if (objToUse == null) {
            if (poolQueue.Count < maxPoolSizePerPrefab) {
                objToUse = Instantiate(prefab, transform);
            } else {
                return null;
            }
        }

        poolQueue.Enqueue(objToUse);

        objToUse.SetActive(false);
        objToUse.transform.position = position;
        objToUse.transform.rotation = rotation;

        Rigidbody2D rb2d = objToUse.GetComponent<Rigidbody2D>();
        if (rb2d != null) {
            rb2d.linearVelocity = Vector2.zero;
            rb2d.angularVelocity = 0f;
        }

        objToUse.SetActive(true);
        ResetAllChildren(objToUse);

        if (!activeSpawnedObstacles.Contains(objToUse)) {
            activeSpawnedObstacles.Add(objToUse);
        }

        return objToUse;
    }

    private void ResetAllChildren(GameObject root) {
        Obstacles rootObstacle = root.GetComponent<Obstacles>();
        if (rootObstacle != null) {
            rootObstacle.OnSpawned();
        }

        Obstacles[] childObstacles = root.GetComponentsInChildren<Obstacles>(true);
        foreach (var child in childObstacles) {
            child.gameObject.SetActive(true);
            child.OnSpawned();
        }
    }

    private bool ValidateAndGetCamera() {
        if (mainCamera == null) {
            mainCamera = Camera.main;
        }
        return mainCamera != null;
    }
    #endregion
}

#region Configuration Classes
public enum LevelSpawnType { DynamicLoop, StaticGroup }

[System.Serializable]
public class LevelConfig {
    public string levelName = "Level 0";
    public LevelSpawnType spawnType = LevelSpawnType.DynamicLoop;
    public float duration = 10f;

    public GameObject staticGroupPrefab;

    public float spawnInterval = 0.8f;
    public GameObject[] obstaclePrefabs;
}
#endregion