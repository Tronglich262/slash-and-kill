using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public static string nextSpawnPoint;

    public static void SetSpawnPoint(string pointName)
    {
        nextSpawnPoint = pointName;
    }
}