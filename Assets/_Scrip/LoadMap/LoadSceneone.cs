
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadSceneone : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (SceneLoader.Instance != null)
                SceneLoader.Instance.LoadScene("Map2");
        }
    }
}
