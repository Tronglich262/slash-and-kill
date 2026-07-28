using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance;

    private string nextSceneName;
    public bool IsLoading { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LoadScene(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogError("Cannot load an empty scene name.");
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError($"Scene '{sceneName}' is not available in Build Settings.");
            if (GameManager.Instance != null)
                GameManager.Instance.ShowNotification($"Map '{sceneName}' chưa khả dụng.");
            return;
        }

        if (IsLoading)
            return;

        if (SceneManager.GetActiveScene().name == sceneName)
            return;

        nextSceneName = sceneName;
        IsLoading = true;
        StartCoroutine(LoadLoadingSceneAsync());
    }

    private System.Collections.IEnumerator LoadLoadingSceneAsync()
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync("Load");
        while (!operation.isDone)
            yield return null;
    }

    public string GetNextSceneName()
    {
        return nextSceneName;
    }

    public void FinishLoading()
    {
        IsLoading = false;
    }
}
