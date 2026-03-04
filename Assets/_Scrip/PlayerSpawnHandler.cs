using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerSpawnHandler : MonoBehaviour
{
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!string.IsNullOrEmpty(SpawnManager.nextSpawnPoint))
        {
            GameObject spawnPoint = GameObject.Find(SpawnManager.nextSpawnPoint);

            if (spawnPoint != null)
            {
                transform.position = spawnPoint.transform.position;
            }
         
        }
    }
}