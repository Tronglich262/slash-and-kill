using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public static string nextSpawnPoint;

    private void Start()
    {
        if (!string.IsNullOrEmpty(nextSpawnPoint))
        {
            GameObject spawnPoint = GameObject.Find(nextSpawnPoint);
            if (spawnPoint != null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    player.transform.position = spawnPoint.transform.position;
                }
            }
        }
    }
}
