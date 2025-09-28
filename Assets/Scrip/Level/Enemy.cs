using UnityEngine;

public class Enemy : MonoBehaviour
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
            int expGained = Random.Range(500, 1000);
            levelSystem.GainExp(expGained);
        }
    }
}