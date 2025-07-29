using UnityEngine;

public class LoadMap1 : MonoBehaviour
{
    public string targetScene = "Map1";

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            SceneLoader.Instance.LoadScene(targetScene);
        }
    }
}
