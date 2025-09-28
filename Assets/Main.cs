using UnityEngine;

public class Main : MonoBehaviour
{
    private static Main instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Giữ lại khi load scene
        }
        else
        {
            Destroy(gameObject); // Nếu đã có instance thì phá bỏ cái mới
        }
    }
}
