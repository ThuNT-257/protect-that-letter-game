using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    private static SceneController instance;

    public static SceneController Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindAnyObjectByType<SceneController>();
                if (instance != null)
                {
                    Debug.Log("There is no SceneController in Scene.");
                }
            }
            return instance;
        }
    }

    [Header("Scene Names")]
    public const string LOGIN_SCENE = "LoginScene";
    public const string STORY_SCENE = "StoryScene";

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        instance = this;
    }

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadSceneAsync(sceneName);
    }
}
