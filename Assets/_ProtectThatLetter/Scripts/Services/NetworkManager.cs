using System;
using System.Collections;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;

#region API Models
[Serializable]
public class ApiResponse<T> {
    public bool success;
    public string message;
    public string errorCode;
    public T data;
}

[Serializable]
public class ConfirmationRequest {
    public string code;
    public bool isAttending;
    public string note;

    public ConfirmationRequest() { }

    public ConfirmationRequest(string code, bool isAttending, string note) {
        this.code = code;
        this.isAttending = isAttending;
        this.note = note;
    }
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
    [SerializeField] private string defaultLocalUrl = "https://api.lynxworld.space";

    private string baseUrl;
    public bool IsInitialized { get; private set; } = false;

    private IEnumerator Start() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            yield return StartCoroutine(InitBaseUrlCoroutine());
        } else {
            Destroy(gameObject);
        }
    }

    private IEnumerator InitBaseUrlCoroutine() {
        string rawPath = Path.Combine(Application.streamingAssetsPath, "env.json");
        string configPath = rawPath.Replace("\\", "/");

        configPath = $"{configPath}?t={DateTime.UtcNow.Ticks}";

        if (!configPath.StartsWith("http://") && !configPath.StartsWith("https://") && !configPath.StartsWith("file://")) {
            configPath = "file://" + configPath;
        }

        using (UnityWebRequest request = UnityWebRequest.Get(configPath)) {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success) {
                try {
                    EnvConfig config = JsonConvert.DeserializeObject<EnvConfig>(request.downloadHandler.text);
                    if (config != null && !string.IsNullOrEmpty(config.baseUrl)) {
                        baseUrl = config.baseUrl.TrimEnd('/');
                    }
                } catch (Exception ex) {
                    Debug.LogError($"[NetworkManager] Failed to parse env.json: {ex.Message}");
                }
            } else {
                Debug.LogWarning($"[NetworkManager] Could not load env.json ({request.error}). Fallback to defaultLocalUrl.");
            }
        }

        if (string.IsNullOrEmpty(baseUrl)) {
            baseUrl = defaultLocalUrl.TrimEnd('/');
        }

        Debug.Log($"[NetworkManager] Initialized with BaseUrl: '{baseUrl}'");
        IsInitialized = true;
    }

    [Serializable]
    private class EnvConfig {
        public string baseUrl;
    }

    private string BuildUrl(string endpoint) {
        if (string.IsNullOrEmpty(endpoint)) return baseUrl;
        string formattedEndpoint = endpoint.StartsWith("/") ? endpoint : "/" + endpoint;
        return baseUrl + formattedEndpoint;
    }

    public IEnumerator GetRequest(string endpoint, Action<bool, string> onComplete) {
        while (!IsInitialized) yield return null;

        string url = BuildUrl(endpoint);
        Debug.Log($"[NetworkManager] GET '{url}'");

        using (UnityWebRequest request = UnityWebRequest.Get(url)) {
            request.timeout = 10;
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success) {
                onComplete?.Invoke(true, null);
            } else {
                Debug.LogError($"[NetworkManager] GET Failed '{url}': {request.error}");
                onComplete?.Invoke(false, ErrorCodes.ERROR_DATABASE_OFFLINE);
            }
        }
    }

    public IEnumerator PostRequest<TRequest, TResponse>(
        string endpoint,
        TRequest requestData,
        Action<bool, TResponse, string> onComplete) {

        string jsonPayload = JsonConvert.SerializeObject(requestData);
        yield return StartCoroutine(PostRequest<TResponse>(endpoint, jsonPayload, onComplete));
    }

    public IEnumerator PostRequest<TResponse>(
        string endpoint,
        string jsonPayload,
        Action<bool, TResponse, string> onComplete) {

        while (!IsInitialized) yield return null;

        string url = BuildUrl(endpoint);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);

        Debug.Log($"[NetworkManager] POST '{url}' | Payload: {jsonPayload}");

        using (UnityWebRequest request = new UnityWebRequest(url, "POST")) {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.timeout = 10;

            yield return request.SendWebRequest();

            string jsonResponse = request.downloadHandler?.text;

            if (!string.IsNullOrEmpty(jsonResponse)) {
                try {
                    var response = JsonConvert.DeserializeObject<ApiResponse<TResponse>>(jsonResponse);
                    if (response != null && response.success) {
                        onComplete?.Invoke(true, response.data, null);
                    } else {
                        string errCode = response != null && !string.IsNullOrEmpty(response.errorCode)
                            ? response.errorCode
                            : ErrorCodes.ERROR_INTERNAL_SERVER;
                        Debug.LogWarning($"[NetworkManager] POST API Error '{url}': {errCode}");
                        onComplete?.Invoke(false, default, errCode);
                    }
                } catch (Exception ex) {
                    Debug.LogError($"[NetworkManager] JSON Parsing Exception '{url}': {ex.Message}");
                    onComplete?.Invoke(false, default, ErrorCodes.ERROR_INTERNAL_SERVER);
                }
            } else {
                Debug.LogError($"[NetworkManager] POST Failed '{url}': {request.error}");
                onComplete?.Invoke(false, default, ErrorCodes.ERROR_DATABASE_OFFLINE);
            }
        }
    }
}