using System;
using System.Collections;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

#region API Models
[Serializable]
public class ApiResponse<T> {
    public bool success;
    public string message;
    public string errorCode;
    public T data;
}

public static class ErrorCodes {
    public const string ERROR_EMPTY_CODE = "ERROR_EMPTY_CODE";
    public const string ERROR_INVALID_CODE = "ERROR_INVALID_CODE";
    public const string ERROR_DATABASE_OFFLINE = "ERROR_DATABASE_OFFLINE";
    public const string ERROR_INTERNAL_SERVER = "ERROR_INTERNAL_SERVER";
}
#endregion

public class NetworkManager : MonoBehaviour {
    public static NetworkManager Instance { get; private set; }

    [Header("Server Config")]
    [SerializeField] private string defaultLocalUrl = "http://localhost:5000";

    private string baseUrl;

    private void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitBaseUrl();
        } else {
            Destroy(gameObject);
        }
    }

    private void InitBaseUrl() {
        string configPath = Path.Combine(Application.streamingAssetsPath, "env.json");
        Debug.Log($"[NetworkManager] Checking env.json at path: {configPath}");

        if (File.Exists(configPath)) {
            try {
                string json = File.ReadAllText(configPath);
                Debug.Log($"[NetworkManager] Found env.json! Raw Content: '{json}'");

                EnvConfig config = JsonUtility.FromJson<EnvConfig>(json);
                if (config != null && !string.IsNullOrEmpty(config.baseUrl)) {
                    baseUrl = config.baseUrl.TrimEnd('/');
                    Debug.Log($"[NetworkManager] Successfully set baseUrl from env.json -> '{baseUrl}'");
                    return;
                } else {
                    Debug.LogWarning("[NetworkManager] env.json existed but 'baseUrl' field was null or empty.");
                }
            } catch (Exception ex) {
                Debug.LogError($"[NetworkManager] Failed to read or parse env.json: {ex.Message}");
            }
        } else {
            Debug.LogWarning($"[NetworkManager] env.json NOT found at '{configPath}'. Using defaultLocalUrl.");
        }

        baseUrl = defaultLocalUrl.TrimEnd('/');
        Debug.Log($"[NetworkManager] Fallback baseUrl set to -> '{baseUrl}'");
    }

    [Serializable]
    private class EnvConfig {
        public string baseUrl;
    }

    public IEnumerator GetRequest(string endpoint, Action<bool, string> onComplete) {
        string url = baseUrl + endpoint;
        Debug.Log($"[NetworkManager] Sending GET Request to URL: '{url}'");

        using (UnityWebRequest request = UnityWebRequest.Get(url)) {
            request.timeout = 10;
            yield return request.SendWebRequest();

            Debug.Log($"[NetworkManager] Response Status Code: {request.responseCode}, Result: {request.result}");

            if (request.result == UnityWebRequest.Result.Success) {
                Debug.Log($"[NetworkManager] GET Request SUCCESS! Response Body: {request.downloadHandler?.text}");
                onComplete?.Invoke(true, null);
            } else {
                Debug.LogError($"[NetworkManager] GET Request FAILED! Error: '{request.error}', Response Code: {request.responseCode}");
                onComplete?.Invoke(false, ErrorCodes.ERROR_DATABASE_OFFLINE);
            }
        }
    }

    public IEnumerator PostRequest<TRequest, TResponse>(
        string endpoint,
        TRequest requestData,
        Action<bool, TResponse, string> onComplete) {

        string jsonPayload = JsonUtility.ToJson(requestData);
        yield return StartCoroutine(PostRequest<TResponse>(endpoint, jsonPayload, onComplete));
    }

    public IEnumerator PostRequest<TResponse>(
        string endpoint,
        string jsonPayload,
        Action<bool, TResponse, string> onComplete) {

        string url = baseUrl + endpoint;
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);

        Debug.Log($"[NetworkManager] Sending POST Request to URL: '{url}', Body: {jsonPayload}");

        using (UnityWebRequest request = new UnityWebRequest(url, "POST")) {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.timeout = 10;

            yield return request.SendWebRequest();

            string jsonResponse = request.downloadHandler?.text;
            Debug.Log($"[NetworkManager] POST Response Status Code: {request.responseCode}, Result: {request.result}");

            if (!string.IsNullOrEmpty(jsonResponse)) {
                try {
                    Debug.Log($"[NetworkManager] Raw Response Body: {jsonResponse}");
                    var response = JsonUtility.FromJson<ApiResponse<TResponse>>(jsonResponse);

                    if (response != null && response.success) {
                        onComplete?.Invoke(true, response.data, null);
                    } else {
                        string errCode = response != null && !string.IsNullOrEmpty(response.errorCode)
                            ? response.errorCode
                            : ErrorCodes.ERROR_INTERNAL_SERVER;

                        Debug.LogWarning($"[NetworkManager] Server returned Business Error: {errCode}");
                        onComplete?.Invoke(false, default, errCode);
                    }
                } catch (Exception ex) {
                    Debug.LogError($"[NetworkManager] JSON Parse Error: {ex.Message}");
                    onComplete?.Invoke(false, default, ErrorCodes.ERROR_INTERNAL_SERVER);
                }
            } else {
                Debug.LogError($"[NetworkManager] Network Error: {request.error}, Response Code: {request.responseCode}");
                onComplete?.Invoke(false, default, ErrorCodes.ERROR_DATABASE_OFFLINE);
            }
        }
    }
}