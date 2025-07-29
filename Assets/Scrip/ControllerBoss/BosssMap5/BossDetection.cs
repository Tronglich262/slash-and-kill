/*
using UnityEngine;

public class BossDetection : MonoBehaviour
{
    public EnemyAIV enemyALV;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            enemyALV.SetPlayerInRange(true, other.transform);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            enemyALV.SetPlayerInRange(false, null);
        }
    }
}
*/
