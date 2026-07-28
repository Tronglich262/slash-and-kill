using UnityEngine;

[DefaultExecutionOrder(-10000)]
public class Main : MonoBehaviour
{
    private static Main instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            // This root owns the lifetime of Player, HUD and gameplay managers.
            // Child managers must not call DontDestroyOnLoad individually.
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject); // Nếu đã có instance thì phá bỏ cái mới
        }
    }

    private void OnApplicationPause(bool isPaused)
    {
        if (isPaused)
            PlayerPrefs.Save();
    }

    private void OnApplicationQuit()
    {
        PlayerPrefs.Save();
    }
}
