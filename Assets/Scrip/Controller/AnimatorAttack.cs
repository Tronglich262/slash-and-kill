using UnityEngine;

public class AnimatorAttack : MonoBehaviour
{
    public Transform attackPoint;
    public float attackRange = 1f;
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
//map2
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer);

        foreach (Collider2D enemy in hitEnemies)
        {
            SkeletonsHealth skeleton = enemy.GetComponent<SkeletonsHealth>();
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
            EnemyHealthTwo boss2 = enemy.GetComponent<EnemyHealthTwo>();
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
            EnemyHealthOneFor skeleton1 = enemy.GetComponent<EnemyHealthOneFor>();
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
            EnemyHealthOne skeleton0 = enemy.GetComponent<EnemyHealthOne>();
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
            EnemyHealthFor skeleton4 = enemy.GetComponent<EnemyHealthFor>();
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
            EnemyHealthV skeleton5 = enemy.GetComponent<EnemyHealthV>();
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
            EnemyHealthVI skeleton6 = enemy.GetComponent<EnemyHealthVI>();
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
            EnemyHealthVII skeleton7 = enemy.GetComponent<EnemyHealthVII>();
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