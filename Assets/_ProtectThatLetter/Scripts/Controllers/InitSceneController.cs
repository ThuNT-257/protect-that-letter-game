using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class InitSceneController : MonoBehaviour {
    [Header("Settings")]
    [SerializeField] private string healthCheckUrl = "http://<IP_VPS_CUA_BAN>:8080/api/health";
    [SerializeField] private float timeOutSeconds = 10f;
    [SerializeField] private string nextSceneName = "MainMenuScene";

    private void Start() {
        StartCoroutine(CheckServerAndLoad());
    }

    private IEnumerator CheckServerAndLoad() {
        using (UnityWebRequest request = UnityWebRequest.Get(healthCheckUrl)) {
            request.timeout = Mathf.RoundToInt(timeOutSeconds);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success) {
                Debug.Log("[INIT] Connect Successfully.");
                SceneManager.LoadScene(nextSceneName);
            } else {
                Debug.LogError($"[INIT] Can not connect| Error: {request.error}");
            }
        }
    }
}