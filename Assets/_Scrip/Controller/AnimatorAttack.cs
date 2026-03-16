using UnityEngine;

public class AnimatorAttack : MonoBehaviour
{
    public Transform attackPoint;
    [SerializeField] public float attackRange = 1f;
    public LayerMask enemyLayer;
    public LevelSystem levelSystem;

    [Header("Mana Restore")]
    public int manaRestoreOnHit = 5; // Lượng mana hồi khi đánh trúng

    [Header("Critical Settings")]
    public float criticalChance = 0.2f; // 20% tỉ lệ chí mạng
    public float criticalMultiplier = 2f; // 2x damage khi chí mạng

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
            Debug.LogError("attackPoint chưa được gán trong Inspector!");
            return;
        }

        bool hitAnyEnemy = false;
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer);

        foreach (Collider2D enemy in hitEnemies)
        {
            EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                // tính dame và kiểm tra chí mạng
                float baseDamage = levelSystem != null ? levelSystem.attack : 10f;
                bool isCritical = Random.value < criticalChance;
                float finalDamage = isCritical ? baseDamage * criticalMultiplier : baseDamage;
                
                enemyHealth.TakeDamage(finalDamage, isCritical);
                hitAnyEnemy = true;
            }
        }

        // đánh trúng quái hồi mana
        if (hitAnyEnemy)
        {
            RestoreMana();
        }
    }

    // hồi mana khi tấn công trúng quái
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