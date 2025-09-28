using UnityEngine;

public class BossTwo : MonoBehaviour
{
    public LevelSystem levelSystem; // Kéo vào từ Inspector
    public void Start()
    {

        if (levelSystem == null)
        {
            levelSystem = FindObjectOfType<LevelSystem>();
        }
    }
    private void OnDestroy() // Khi boss chết
    {
        if (levelSystem != null)
        {
            int expGained = Random.Range(10000, 20000); 
            levelSystem.GainExp(expGained);
        }
    }
}