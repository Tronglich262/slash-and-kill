using UnityEngine;

public class Enemy : MonoBehaviour
{
    // Experience is awarded by EnemyHealth only after a confirmed defeat.
    // Awarding it from OnDestroy also paid experience when a scene unloaded.
}
