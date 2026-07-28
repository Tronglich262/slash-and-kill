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
            SceneLoader.Instance?.FinishLoading();
            return;
        }

        StartCoroutine(LoadSceneAsync(targetScene));
    }

    IEnumerator LoadSceneAsync(string sceneName)
    {
        if (loadingUI != null)
            loadingUI.SetActive(true);

        // Let the loading canvas render before starting expensive asset work.
        yield return null;

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false;
        bool activationRequested = false;
        int lastDisplayedPercent = -1;

        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            if (progressBar != null)
                progressBar.value = progress;

            int displayedPercent = Mathf.RoundToInt(progress * 100f);
            if (loadingText != null && displayedPercent != lastDisplayedPercent)
            {
                loadingText.SetText("Đang tải... {0}%", displayedPercent);
                lastDisplayedPercent = displayedPercent;
            }

            if (operation.progress >= 0.9f && !activationRequested)
            {
                activationRequested = true;
                if (loadingText != null)
                    loadingText.text = "Đã sẵn sàng!";
                yield return new WaitForSeconds(0.3f);
                // This object is destroyed during scene activation, so release
                // the transition lock before activating the destination scene.
                SceneLoader.Instance?.FinishLoading();
                operation.allowSceneActivation = true;
            }

            yield return null;
        }

    }
}
