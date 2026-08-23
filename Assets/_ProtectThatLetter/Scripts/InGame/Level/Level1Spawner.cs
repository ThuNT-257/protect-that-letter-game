using System.Collections;
using UnityEngine;

public class Level1Spawner : MonoBehaviour
{
    #region Serialized Fields
    [Header("References")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private GameObject leafPrefab;
    [SerializeField] private GameObject paperPrefab;

    [Header("Spawn Settings")]
    [SerializeField] private float spawnInterval = 0.8f;
    [SerializeField] private float leafChance = 0.7f;
    [SerializeField] private float spawnPadding = 0.5f;
    [SerializeField] private float levelDuration = 10f;
    [SerializeField] private float delayToNextLevel = 5f;
    #endregion

    #region Private Fields
    private float minX, maxX, spawnY;
    private bool isSpawning = true;
    #endregion

    #region Lifecycle
    private void Awake()
    {
        if(mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        CalculateSpawnArea();
    }

    private void Start()
    {
        StartCoroutine(LevelSequenceRoutine());
    }
    #endregion

    #region Public Methods
    public void StopSpawning()
    {
        isSpawning = false;
        Debug.Log("[Level1Spawner] - Stop Spawning");
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

    private IEnumerator LevelSequenceRoutine()
    {
        StartCoroutine(SpawnRoutine());
        
        yield return new WaitForSeconds(levelDuration);

        Debug.Log("[Level1Spawner] - Stop Spawning");

        StopSpawning();

        yield return new WaitForSeconds(delayToNextLevel);

        Debug.Log("[Level1Spawner] - Go to next Level or Scene :v");
    }

    private IEnumerator SpawnRoutine()
    {
        while (isSpawning)
        {
            yield return new WaitForSeconds(spawnInterval);

            float randomX = Random.Range(minX, maxX);
            Vector3 spawnPos = new Vector3(randomX, spawnY, 0f);

            GameObject prefabToSpawn = (Random.value < leafChance) ? leafPrefab : paperPrefab;

            Quaternion randomRotation = Quaternion.Euler(0, 0, Random.Range(-45f, 45f));
            Instantiate(prefabToSpawn, spawnPos, randomRotation);
        }
    }
    #endregion
}
