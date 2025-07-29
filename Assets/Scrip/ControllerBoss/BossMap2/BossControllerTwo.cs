using System.Collections;
using UnityEngine;

public class BossControllerTwo : MonoBehaviour
{
    public Transform player;
    public float speed = 2f;
    public float attackRange = 5f;
    public float rangedAttackRange = 8f;
    public float meleeAttackCooldown = 2f;
    public float rangedAttackCooldown = 4f;
    public int attackDamage = 10;
    public float jumpBackDistance = 2f;
    public float jumpSpeed = 4f;

    public Transform[] teleportPoints;
    public float teleportCooldown = 5f;

    public GameObject attackProjectile;
    public Transform attackSpawnPoint;

    public float dashCooldown = 8f;

    private bool isAttacking = false;
    private bool isFiring = false;

    private Animator animator;
    private float lastMeleeAttackTime;
    private float lastRangedAttackTime;
    private float lastTeleportTime;
    private float lastDashTime;

    private enum BossState { Idle, Chasing, Attacking, RangedAttacking, Dashing, Teleporting, Patrolling }
    private BossState currentState = BossState.Idle;

    private EnemyHealthTwo enemyHealthTwo;
    private int patrolIndex = 0;
    private float thinkDelay = 0.5f;
    private float nextThinkTime = 0f;

    void Start()
    {
        animator = GetComponent<Animator>();
        enemyHealthTwo = GetComponent<EnemyHealthTwo>();
        UpdateAnimator();
    }

    void Update()
    {
        if (Time.time < nextThinkTime || isAttacking || isFiring) return;

        EvaluateState();
        nextThinkTime = Time.time + thinkDelay;
        UpdateAnimator();
    }

    void EvaluateState()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        bool lowHealth = enemyHealthTwo.currentHealth <= enemyHealthTwo.maxHealth * 0.5f;
        bool criticalHealth = enemyHealthTwo.currentHealth <= enemyHealthTwo.maxHealth * 0.3f;

        if (criticalHealth && Time.time - lastTeleportTime >= teleportCooldown)
        {
            ChangeState(BossState.Teleporting);
            isAttacking = true;
            StartCoroutine(TeleportRandomly());
            lastTeleportTime = Time.time;
        }
        else if (distanceToPlayer <= attackRange && Time.time - lastMeleeAttackTime >= meleeAttackCooldown)
        {
            ChangeState(BossState.Attacking);
            Flip(player.position.x);
            isAttacking = true;
            StartCoroutine(ApproachAndAttack());
        }
        else if (distanceToPlayer <= rangedAttackRange && Time.time - lastRangedAttackTime >= rangedAttackCooldown)
        {
            ChangeState(BossState.RangedAttacking);
            Flip(player.position.x);
            isFiring = true;
            StartCoroutine(FireMultipleProjectiles());
        }
        else if (distanceToPlayer <= 10f && Time.time - lastDashTime >= dashCooldown && !lowHealth)
        {
            ChangeState(BossState.Dashing);
            Flip(player.position.x);
            isAttacking = true;
            StartCoroutine(DashToPlayer());
            lastDashTime = Time.time;
        }
        else if (distanceToPlayer <= 12f)
        {
            ChangeState(BossState.Chasing);
            ChasePlayer();
        }
        else
        {
            ChangeState(BossState.Patrolling);
            Patrol();
        }
    }

    void ChangeState(BossState newState)
    {
        if (currentState != newState)
            currentState = newState;
    }

    void UpdateAnimator()
    {
        animator.SetBool("Run", currentState == BossState.Chasing || currentState == BossState.Dashing || currentState == BossState.Teleporting || currentState == BossState.Patrolling);
    }

    void Flip(float targetX)
    {
        if (targetX > transform.position.x)
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        else
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
    }

    void ChasePlayer()
    {
        Flip(player.position.x);
        transform.position = Vector2.MoveTowards(transform.position, player.position, speed * Time.deltaTime);

        // BỔ SUNG DÒNG NÀY
        animator.SetBool("Run", true);
    }


    void Patrol()
    {
        Transform patrolPoint = teleportPoints[patrolIndex];
        transform.position = Vector2.MoveTowards(transform.position, patrolPoint.position, speed * Time.deltaTime);

        if (Vector2.Distance(transform.position, patrolPoint.position) < 0.5f)
        {
            patrolIndex = (patrolIndex + 1) % teleportPoints.Length;
        }
    }

    IEnumerator ApproachAndAttack()
    {
        animator.SetBool("Run", true);

        while (Vector2.Distance(transform.position, player.position) > 1f)
        {
            transform.position = Vector2.MoveTowards(transform.position, player.position, speed * Time.deltaTime);
            yield return null;
        }

        animator.SetBool("Run", false);
        ResetAttackAnimations();

        int randomAttack = Random.Range(0, 3);
        switch (randomAttack)
        {
            case 0: animator.SetBool("Attack", true); break;
            case 1: animator.SetBool("Attack1", true); break;
            case 2: animator.SetBool("Attack2", true); break;
        }

        yield return new WaitForSeconds(0.3f);

        if (Vector2.Distance(transform.position, player.position) <= attackRange)
        {
            HealthSystem playerHealth = player.GetComponent<HealthSystem>();
            if (playerHealth != null)
                playerHealth.TakeDamage(attackDamage);
        }

        yield return new WaitForSeconds(0.3f);
        ResetAttackAnimations();
        yield return StartCoroutine(JumpBack());
        lastMeleeAttackTime = Time.time;
        isAttacking = false;
    }

    IEnumerator FireMultipleProjectiles()
    {
        animator.SetBool("Attack", true);

        for (int i = 0; i < 2; i++)
        {
            yield return new WaitForSeconds(0.2f);
            GameObject projectile = Instantiate(attackProjectile, attackSpawnPoint.position, Quaternion.identity);
            Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
            Vector2 direction = (player.position - transform.position).normalized;
            rb.linearVelocity = direction * 10f;
        }

        yield return new WaitForSeconds(0.3f);
        animator.SetBool("Attack", false);
        lastRangedAttackTime = Time.time;
        isFiring = false;
    }

    IEnumerator JumpBack()
    {
        animator.SetBool("Jump", true);
        float direction = transform.position.x > player.position.x ? 1f : -1f;
        Vector3 jumpTarget = new Vector3(transform.position.x + direction * jumpBackDistance, transform.position.y, transform.position.z);

        while (Vector3.Distance(transform.position, jumpTarget) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, jumpTarget, jumpSpeed * Time.deltaTime);
            yield return null;
        }

        animator.SetBool("Jump", false);
    }

    IEnumerator TeleportRandomly()
    {
        animator.SetBool("Jump", true);
        yield return new WaitForSeconds(0.3f);
        Transform targetPoint = teleportPoints[Random.Range(0, teleportPoints.Length)];
        transform.position = targetPoint.position;
        Flip(player.position.x);
        animator.SetBool("Jump", false);
        isAttacking = false;
    }

    IEnumerator DashToPlayer()
    {
        Vector2 target = player.position;
        float dashSpeed = 10f;

        while (Vector2.Distance(transform.position, target) > 0.5f)
        {
            transform.position = Vector2.MoveTowards(transform.position, target, dashSpeed * Time.deltaTime);
            yield return null;
        }

        if (Vector2.Distance(transform.position, player.position) <= attackRange)
        {
            yield return StartCoroutine(ApproachAndAttack());
        }

        isAttacking = false;
    }

    void ResetAttackAnimations()
    {
        animator.SetBool("Attack", false);
        animator.SetBool("Attack1", false);
        animator.SetBool("Attack2", false);
        animator.SetBool("Jump", false);
    }
}
