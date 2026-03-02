using UnityEngine;

public class Enemy : MonoBehaviour
{
    public LevelSystem levelSystem;
    [SerializeField] public int min = 500;
    [SerializeField] public int max = 1000;
    public void Start()
    {

        if (levelSystem == null)
        {
            levelSystem = FindFirstObjectByType<LevelSystem>();
        }
    }
    private void OnDestroy() 
    {
        if (levelSystem != null)
        {
            int expGained = Random.Range(min, max);
            levelSystem.GainExp(expGained);
        }
    }
}