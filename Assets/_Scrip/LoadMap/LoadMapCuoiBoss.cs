using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadMapCuoiBoss : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Đã va chạm với Player");
            SceneManager.LoadScene("MapBoss");
        }
    }
}
