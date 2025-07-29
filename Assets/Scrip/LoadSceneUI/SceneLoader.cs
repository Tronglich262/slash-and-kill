using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance;

    private string nextSceneName;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // Giữ lại khi load scene mới
    }

    public void LoadScene(string sceneName)
    {
        nextSceneName = sceneName;
        SceneManager.LoadScene("Load"); // luôn gọi scene loading
    }

    public string GetNextSceneName()
    {
        return nextSceneName;
    }
}
