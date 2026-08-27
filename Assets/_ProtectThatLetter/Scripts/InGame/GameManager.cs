using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static ObstacleSpawner;

public class GameManager : MonoBehaviour
{
    #region Serialized Fields
    [SerializeField] private Slider timebarSlider;
    [SerializeField] private ObstacleSpawner spawner;
    [SerializeField] private float transitionDelay = 5f;

    [Header("Level Configurations")]
    [SerializeField] private List<LevelConfig> levels = new List<LevelConfig>();
    #endregion

    #region Private Field
    private float totalGameDuration = 0f;
    private float currentGameTime = 0f;
    #endregion

    #region Lifecycle
    private void Start()
    {
        foreach (var level in levels)
        {
            totalGameDuration += level.duration;
        }

        if (timebarSlider != null)
        {
            timebarSlider.minValue = 0f;
            timebarSlider.maxValue = 1f;
            timebarSlider.value = 0f;
        }

        StartCoroutine(GameLoopRoutine());
    }

    private void Update()
    {
        if (currentGameTime < totalGameDuration)
        {
            currentGameTime += Time.deltaTime;
            if (timebarSlider != null)
            {
                timebarSlider.value = Mathf.Clamp01(currentGameTime / totalGameDuration);
            }
        }
    }
    #endregion

    #region Private Methods
    private IEnumerator GameLoopRoutine()
    {
        for (int i = 0; i < levels.Count; i++)
        {
            LevelConfig currentLevel = levels[i];
            Debug.Log($"[GameManager] - Start {currentLevel.levelName}");

            spawner.StartLevel(currentLevel);

            yield return new WaitForSeconds(currentLevel.duration);

            spawner.StopSpawning();
            Debug.Log($"[GameManager] - Finish {currentLevel.levelName}! Waiting for transition...");

            if (i < levels.Count - 1)
            {
                yield return new WaitForSeconds(transitionDelay);
            }
        }

        Debug.Log("[GameManager] - WIN!");
    }
    #endregion
}
