using System;
using System.Collections;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkManager : MonoBehaviour {
    public static NetworkManager Instance { get; private set; }

    [Header("Server Config")]
    [SerializeField] private string defaultLocalUrl = "http://example:1234";

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

        if (File.Exists(configPath)) {
            string json = File.ReadAllText(configPath);
            EnvConfig config = JsonUtility.FromJson<EnvConfig>(json);
            if (config != null && !string.IsNullOrEmpty(config.baseUrl)) {
                baseUrl = config.baseUrl;
                return;
            }
        }

        baseUrl = defaultLocalUrl;
    }

    [Serializable]
    private class EnvConfig {
        public string baseUrl;
    }

    public IEnumerator GetRequest(string endpoint, Action<bool, string> onComplete) {
        string url = baseUrl + endpoint;

        using (UnityWebRequest request = UnityWebRequest.Get(url)) {
            request.timeout = 10;
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success) {
                onComplete?.Invoke(true, null);
            } else {
                onComplete?.Invoke(false, ErrorCodes.ERROR_DATABASE_OFFLINE);
            }
        }
    }

    public IEnumerator PostRequest<TRequest, TResponse>(
        string endpoint,
        TRequest requestData,
        Action<ApiResponse<TResponse>> onComplete) {
        string url = baseUrl + endpoint;
        string jsonBody = JsonUtility.ToJson(requestData);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);

        using (UnityWebRequest request = new UnityWebRequest(url, "POST")) {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.timeout = 10;

            yield return request.SendWebRequest();

            string jsonResponse = request.downloadHandler?.text;

            if (!string.IsNullOrEmpty(jsonResponse)) {
                try {
                    var response = JsonUtility.FromJson<ApiResponse<TResponse>>(jsonResponse);
                    onComplete?.Invoke(response);
                } catch (Exception ex) {
                    Debug.LogError($"[NetworkManager] Parse Error: {ex.Message}");
                    onComplete?.Invoke(new ApiResponse<TResponse> {
                        success = false,
                        message = "Server error.",
                        errorCode = ErrorCodes.ERROR_INTERNAL_SERVER
                    });
                }
            }
            else {
                Debug.LogError($"[NetworkManager] Network Error: {request.error}");
                onComplete?.Invoke(new ApiResponse<TResponse> {
                    success = false,
                    message = "Can not connect to server.",
                    errorCode = ErrorCodes.ERROR_DATABASE_OFFLINE
                });
            }
        }
    }
}