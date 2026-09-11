using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudSpawner : MonoBehaviour {
    [Header("Cloud Visual Settings")]
    [SerializeField] private GameObject cloudPrefab;
    [SerializeField] private Sprite[] cloudSprites;

    [Header("Spawn Density Settings")]
    [SerializeField] private float densityMultiplier = 0.05f;
    [SerializeField] private int minTotalClouds = 5;
    [SerializeField] private int maxTotalClouds = 25;

    [Header("Cloud Attributes")]
    [SerializeField] private float minScale = 0.6f;
    [SerializeField] private float maxScale = 1.4f;
    [SerializeField] private float minSpeed = 0.5f;
    [SerializeField] private float maxSpeed = 1.5f;
    [SerializeField] private float spawnInterval = 1.5f;

    [Header("Camera Reference")]
    [SerializeField] private Camera mainCamera;

    private List<Cloud> cloudPool = new List<Cloud>();
    private float screenLeft, screenRight, screenTop, screenBottom;
    private int calculatedPoolSize;
    private bool isSpawning = false;
    private float spawnTimer = 0f;

    private int lastScreenWidth;
    private int lastScreenHeight;

    private void Start() {
        if (mainCamera == null) mainCamera = Camera.main;

        CalculateScreenBounds();
        InitializePool();
    }

    private void Update() {
        if (Screen.width != lastScreenWidth || Screen.height != lastScreenHeight) {
            OnScreenResolutionChanged();
        }

        if (!isSpawning) return;

        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnInterval) {
            spawnTimer = 0f;
            SpawnCloudFromPool();
        }
    }

    public void StartSpawning() {
        if (isSpawning) return;

        isSpawning = true;
        spawnTimer = spawnInterval;
    }

    public void StopAndResetSpawner() {
        isSpawning = false;
        spawnTimer = 0f;

        foreach (Cloud cloud in cloudPool) {
            if (cloud != null) {
                cloud.gameObject.SetActive(false);
            }
        }
    }

    private void OnScreenResolutionChanged() {
        CalculateScreenBounds();

        int currentCount = cloudPool.Count;
        if (calculatedPoolSize > currentCount) {
            for (int i = 0; i < calculatedPoolSize - currentCount; i++) {
                GameObject obj = Instantiate(cloudPrefab, transform);
                Cloud cloud = obj.GetComponent<Cloud>();
                obj.SetActive(false);
                cloudPool.Add(cloud);
            }
        }
    }

    private void CalculateScreenBounds() {
        if (mainCamera == null) mainCamera = Camera.main;
        if (mainCamera == null) return;

        lastScreenWidth = Screen.width;
        lastScreenHeight = Screen.height;

        float height = 2f * mainCamera.orthographicSize;
        float width = height * mainCamera.aspect;
        float screenArea = width * height;

        calculatedPoolSize = Mathf.Clamp(Mathf.RoundToInt(screenArea * densityMultiplier), minTotalClouds, maxTotalClouds);

        screenLeft = mainCamera.transform.position.x - width / 2f;
        screenRight = mainCamera.transform.position.x + width / 2f;
        screenTop = mainCamera.transform.position.y + height / 2f;
        screenBottom = mainCamera.transform.position.y - height / 2f;
    }

    private void InitializePool() {
        for (int i = 0; i < calculatedPoolSize; i++) {
            GameObject obj = Instantiate(cloudPrefab, transform);
            Cloud cloud = obj.GetComponent<Cloud>();
            obj.SetActive(false);
            cloudPool.Add(cloud);
        }
    }

    private void SpawnCloudFromPool() {
        Cloud availableCloud = GetInactiveCloud();
        if (availableCloud == null) return;

        float randomX = Random.Range(screenLeft, screenRight);
        float spawnY = screenTop + 1.5f;

        availableCloud.transform.position = new Vector3(randomX, spawnY, 0f);
        availableCloud.SetupCloud(cloudSprites, minScale, maxScale, minSpeed, maxSpeed, screenBottom);
    }

    private Cloud GetInactiveCloud() {
        for (int i = 0; i < cloudPool.Count; i++) {
            if (!cloudPool[i].gameObject.activeInHierarchy) {
                return cloudPool[i];
            }
        }
        return null;
    }
}