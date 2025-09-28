using UnityEngine;

public class LoadMap1 : MonoBehaviour
{
    public string targetScene = "Map1";  // Tên map cần load
    public string spawnPointName;        // Tên điểm spawn trong scene mới

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Lưu lại spawnPointName để scene mới biết spawn ở đâu
            SpawnManager.nextSpawnPoint = spawnPointName;

            // Load scene mới
            SceneLoader.Instance.LoadScene(targetScene);
        }
    }
}
