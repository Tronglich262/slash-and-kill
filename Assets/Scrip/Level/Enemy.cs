using UnityEngine;

public class Enemy : MonoBehaviour
{
    public LevelSystem levelSystem; 

    private void OnDestroy() 
    {
        if (levelSystem != null)
        {
            int expGained = Random.Range(500, 1000);
            levelSystem.GainExp(expGained);
        }
    }
}