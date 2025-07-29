using UnityEngine;

public class BossI : MonoBehaviour
{
    public LevelSystem levelSystem; 

    private void OnDestroy()
    {
        if (levelSystem != null)
        {
            int expGained = Random.Range(3000, 10000);
            levelSystem.GainExp(expGained);
        }
    }
}