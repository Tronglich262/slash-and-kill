using UnityEngine;

public class LoadMapByButton : MonoBehaviour
{
    public void LoadTargetScene(string sceneName)
    {
        SceneLoader.Instance.LoadScene(sceneName);
    }
}
