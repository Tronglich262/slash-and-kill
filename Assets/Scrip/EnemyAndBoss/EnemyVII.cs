using UnityEngine;

public class EnemyVII : MonoBehaviour
{
    public LevelSystem levelSystem;
    public void Start()
    {

        if (levelSystem == null)
        {
            levelSystem = FindObjectOfType<LevelSystem>();
        }
    }
    private void OnDestroy() 
    {
        if (levelSystem != null)
        {
            int expGained = Random.Range(100, 200);
            levelSystem.GainExp(expGained);
        }
    }
}