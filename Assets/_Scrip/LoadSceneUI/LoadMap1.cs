using UnityEngine;

public class LoadMap1 : MonoBehaviour
{
    public string targetScene = "MapBoss";
    public string spawnPointName;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            SpawnManager.SetSpawnPoint(spawnPointName);

            SceneLoader.Instance.LoadScene(targetScene);
        }
    }
}