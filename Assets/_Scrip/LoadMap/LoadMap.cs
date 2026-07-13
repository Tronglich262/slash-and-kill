using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadMap : MonoBehaviour
{
    [SerializeField] private string targetScene;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (SceneLoader.Instance == null)
            {
                Debug.LogError("SceneLoader is missing.");
                return;
            }

            SceneLoader.Instance.LoadScene(targetScene);
        }
    }
}
