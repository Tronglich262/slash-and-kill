using UnityEngine;

public class AnimatorAttack : MonoBehaviour
{
    public Transform attackPoint;
    [SerializeField] public float attackRange = 1f;
    public LayerMask enemyLayer;
    public LevelSystem levelSystem;

    [Header("Mana Restore")]
    public int manaRestoreOnHit = 5; // Lượng mana hồi khi đánh trúng

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            TriggerAttack();
        }
    }

    public void TriggerAttack()
    {
        if (attackPoint == null)
        {
            Debug.LogError("⚠ attackPoint chưa được gán trong Inspector!");
            return;
        }

        bool hitAnyEnemy = false;

        // Tất cả các map dùng chung logic
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer);

        foreach (Collider2D enemy in hitEnemies)
        {
            EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                float totalDamage = LevelSystem.Instance != null ? LevelSystem.Instance.attack : 10f;
                enemyHealth.TakeDamage(levelSystem.attack);
                hitAnyEnemy = true;
            }
        }

        // Hồi mana khi đánh trúng enemy
        if (hitAnyEnemy)
        {
            RestoreMana();
        }
    }

    // Hồi mana khi tấn công trúng
    private void RestoreMana()
    {
        if (HealthSystem.Instance != null && manaRestoreOnHit > 0)
        {
            HealthSystem.Instance.RestoreMP(manaRestoreOnHit);
            Debug.Log($"Hồi {manaRestoreOnHit} mana khi tấn công!");
        }
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}