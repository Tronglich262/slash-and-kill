using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadMapCuoiBoss : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Đã va chạm với Player");
            SceneManager.LoadScene("MapBoss");
        }
    }
}
