using UnityEngine;

public class AnimatorAttack : MonoBehaviour
{
    public Transform attackPoint;
    [SerializeField] public float attackRange = 1f;
    public LayerMask enemyLayer;
    public LevelSystem levelSystem;
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
       // map2
                Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer);

        foreach (Collider2D enemy in hitEnemies)
        {
            EnemyHealth skeleton = enemy.GetComponent<EnemyHealth>();
            if (skeleton != null)
            {
                float totalDamage = LevelSystem.Instance != null ? LevelSystem.Instance.attack : 10f;
                skeleton.TakeDamage(levelSystem.attack);

            }
        }
        //map2Boss
        //map2
        Collider2D[] hitEnemies2 = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer);

        foreach (Collider2D enemy in hitEnemies)
        {
            EnemyHealth boss2 = enemy.GetComponent<EnemyHealth>();
            if (boss2 != null)
            {
                float totalDamage = LevelSystem.Instance != null ? LevelSystem.Instance.attack : 10f;
                boss2.TakeDamage(levelSystem.attack);

            }
        }
        //map11
        Collider2D[] hitEnemies1 = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer);

        foreach (Collider2D enemy in hitEnemies)
        {
            EnemyHealth skeleton1 = enemy.GetComponent<EnemyHealth>();
            if (skeleton1 != null)
            {
                float totalDamage = LevelSystem.Instance != null ? LevelSystem.Instance.attack : 10f;
                skeleton1.TakeDamage(levelSystem.attack);

            }
        }
        //map1
        Collider2D[] hitEnemies0 = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer);

        foreach (Collider2D enemy in hitEnemies)
        {
            EnemyHealth skeleton0 = enemy.GetComponent<EnemyHealth>();
            if (skeleton0 != null)
            {
                float totalDamage = LevelSystem.Instance != null ? LevelSystem.Instance.attack : 10f;
                skeleton0.TakeDamage(levelSystem.attack);

            }
        }
        //map4
        Collider2D[] hitEnemies4 = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer);

        foreach (Collider2D enemy in hitEnemies)
        {
            EnemyHealth skeleton4 = enemy.GetComponent<EnemyHealth>();
            if (skeleton4 != null)
            {
                float totalDamage = LevelSystem.Instance != null ? LevelSystem.Instance.attack : 10f;
                skeleton4.TakeDamage(levelSystem.attack);

            }
        }
        //MAP5
        Collider2D[] hitEnemies5 = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer);

        foreach (Collider2D enemy in hitEnemies)
        {
            EnemyHealth skeleton5 = enemy.GetComponent<EnemyHealth>();
            if (skeleton5 != null)
            {
                float totalDamage = LevelSystem.Instance != null ? LevelSystem.Instance.attack : 10f;
                skeleton5.TakeDamage(levelSystem.attack);

            }
        }
        //MAP6
        Collider2D[] hitEnemies6 = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer);

        foreach (Collider2D enemy in hitEnemies)
        {
            EnemyHealth skeleton6 = enemy.GetComponent<EnemyHealth>();
            if (skeleton6 != null)
            {
                float totalDamage = LevelSystem.Instance != null ? LevelSystem.Instance.attack : 10f;
                skeleton6.TakeDamage(levelSystem.attack);

            }
        }
        //MAP7
        Collider2D[] hitEnemies7 = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer);

        foreach (Collider2D enemy in hitEnemies)
        {
            EnemyHealth skeleton7 = enemy.GetComponent<EnemyHealth>();
            if (skeleton7 != null)
            {
                float totalDamage = LevelSystem.Instance != null ? LevelSystem.Instance.attack : 10f;
                skeleton7.TakeDamage(levelSystem.attack);

            }
        }
        //MAP8
        Collider2D[] hitEnemies8 = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer);

        foreach (Collider2D enemy in hitEnemies)
        {
            EnemyHealth skeleton8 = enemy.GetComponent<EnemyHealth>();
            if (skeleton8 != null)
            {
                float totalDamage = LevelSystem.Instance != null ? LevelSystem.Instance.attack : 10f;
                skeleton8.TakeDamage(levelSystem.attack);

            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}