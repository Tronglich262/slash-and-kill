using UnityEngine;

public class BossTwo : MonoBehaviour
{
    public LevelSystem levelSystem; // Kéo vào từ Inspector

    private void OnDestroy() // Khi boss chết
    {
        if (levelSystem != null)
        {
            int expGained = Random.Range(10000, 20000); 
            levelSystem.GainExp(expGained);
        }
    }
}