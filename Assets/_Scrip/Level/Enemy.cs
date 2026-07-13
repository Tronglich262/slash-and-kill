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
    // Experience is awarded by EnemyHealth only after a confirmed defeat.
    // Awarding it from OnDestroy also paid experience when a scene unloaded.
}
