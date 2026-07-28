using UnityEngine;
using UnityEngine.UI;


public class Check : MonoBehaviour
{
    public Slider healthSlider;
    [SerializeField] public float detectRadius = 2f;
    [SerializeField] private float checkInterval = 0.1f;
    private int enemyLayerMask;
    private bool? wasEnemyNearby;
    private float nextCheckTime;

    private void Awake()
    {
        enemyLayerMask = LayerMask.GetMask("Enemy");
    }

    void Update()
    {
        if (Time.time < nextCheckTime)
            return;

        nextCheckTime = Time.time + checkInterval;
        CheckEnemyDistance();
    }

    void CheckEnemyDistance()
    {
        Collider2D enemy = Physics2D.OverlapCircle(transform.position, detectRadius, enemyLayerMask);
        bool isEnemyNearby = enemy != null;

        if (healthSlider != null && wasEnemyNearby != isEnemyNearby)
        {
            healthSlider.gameObject.SetActive(isEnemyNearby);
            wasEnemyNearby = isEnemyNearby;
        }
    }

    void OnDrawGizmosSelected()
    {
        //check enemy
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectRadius);
    }
}
