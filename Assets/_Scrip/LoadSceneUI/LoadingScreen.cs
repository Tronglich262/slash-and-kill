using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class LoadingScreen : MonoBehaviour
{
    public GameObject loadingUI;
    public Slider progressBar;
    public TextMeshProUGUI loadingText;

    void Start()
    {
        string targetScene = SceneLoader.Instance?.GetNextSceneName();

        if (string.IsNullOrEmpty(targetScene))
        {
            Debug.LogError("Scene đích không xác định!");
            return;
        }

        StartCoroutine(LoadSceneAsync(targetScene));
    }

    IEnumerator LoadSceneAsync(string sceneName)
    {
        loadingUI.SetActive(true);
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false;

        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            progressBar.value = progress;
            loadingText.text = $"Đang tải... {progress * 100f:F0}%";

            if (operation.progress >= 0.9f)
            {
                loadingText.text = "Đã sẵn sàng!";
                yield return new WaitForSeconds(0.3f);
                operation.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}
