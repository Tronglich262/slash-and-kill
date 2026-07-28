using UnityEngine;
using System.Collections.Generic;

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
    private readonly List<Collider2D> hitEnemies = new List<Collider2D>(16);
    private ContactFilter2D enemyFilter;

    private void Awake()
    {
        enemyFilter = new ContactFilter2D();
        enemyFilter.SetLayerMask(enemyLayer);
        enemyFilter.useTriggers = true;
    }

#if UNITY_EDITOR
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            TriggerAttack();
        }
    }
#endif

    public void TriggerAttack()
    {
        if (attackPoint == null)
        {
            Debug.LogError("attackPoint chưa được gán trong Inspector!");
            return;
        }

        bool hitAnyEnemy = false;
        hitEnemies.Clear();
        Physics2D.OverlapCircle(attackPoint.position, attackRange, enemyFilter, hitEnemies);

        for (int i = 0; i < hitEnemies.Count; i++)
        {
            if (hitEnemies[i].TryGetComponent(out EnemyHealth enemyHealth))
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
#if UNITY_EDITOR
            Debug.Log($"Hồi {manaRestoreOnHit} mana khi tấn công!");
#endif
        }
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}
