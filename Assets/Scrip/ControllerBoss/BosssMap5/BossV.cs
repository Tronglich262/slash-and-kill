using UnityEngine;

public class BossV : MonoBehaviour
{
    public LevelSystem levelSystem; 

    private void OnDestroy()
    {
        if (levelSystem != null)
        {
            int expGained = Random.Range(1000, 2000); // EXP ngẫu nhiên từ 1000-2000
            levelSystem.GainExp(expGained);
        }
    }
}