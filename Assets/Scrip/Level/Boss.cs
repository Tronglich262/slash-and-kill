using UnityEngine;

public class Boss : MonoBehaviour
{
    public LevelSystem levelSystem; // Kéo vào từ Inspector

    private void OnDestroy() // Khi boss chết
    {
        if (levelSystem != null)
        {
            int expGained = Random.Range(5000, 10000); // EXP ngẫu nhiên từ 1000-2000
            levelSystem.GainExp(expGained);
        }
    }
}