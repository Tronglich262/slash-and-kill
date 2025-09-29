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

    public Transform pointA; // Điểm A xác định khu vực
    public Transform pointB; // Điểm B xác định khu vực

    private bool isAttacking = false;
    private bool isFiring = false;

    private Animator animator;
    private float lastMeleeAttackTime;
    private float lastRangedAttackTime;
    private float lastTeleportTime;
    private float lastDashTime;

    private enum BossState { Idle, Chasing, MeleeAttacking, RangedAttacking, Dashing, Teleporting, Patrolling }
    private BossState currentState = BossState.Idle;

    private EnemyHealth enemyHealthTwo;
    private int patrolIndex = 0;
    private float thinkDelay = 0.2f;
    private float nextThinkTime = 0f;

    private float aggressionLevel = 1f;
    private float randomDecisionFactor = 0.3f;
    private float restTimeAfterAction = 1f; // Thời gian nghỉ sau hành động

    void Start()
    {
        animator = GetComponent<Animator>();
        enemyHealthTwo = GetComponent<EnemyHealth>();
        UpdateAnimator();

        if (player == null)
        {
            GameObject foundPlayer = GameObject.FindWithTag("Player");
            if (foundPlayer != null)
                player = foundPlayer.transform;
        }
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
        if (player == null || !IsPlayerInArea())
        {
            ChangeState(BossState.Idle);
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        bool lowHealth = enemyHealthTwo.currentHealth <= enemyHealthTwo.maxHealth * 0.5f;
        bool criticalHealth = enemyHealthTwo.currentHealth <= enemyHealthTwo.maxHealth * 0.3f;

        aggressionLevel = criticalHealth ? 1.5f : lowHealth ? 1.2f : 1f;

        if ((criticalHealth || (lowHealth && Random.value < randomDecisionFactor)) && Time.time - lastTeleportTime >= teleportCooldown / aggressionLevel)
        {
            ChangeState(BossState.Teleporting);
            isAttacking = true;
            StartCoroutine(TeleportRandomly());
            lastTeleportTime = Time.time;
            return;
        }

        if (distanceToPlayer <= attackRange && Time.time - lastMeleeAttackTime >= meleeAttackCooldown / aggressionLevel)
        {
            ChangeState(BossState.MeleeAttacking);
            FlipTowardsPlayer();
            isAttacking = true;
            StartCoroutine(ApproachAndMeleeAttack());
            return;
        }

        if (distanceToPlayer <= rangedAttackRange && Time.time - lastRangedAttackTime >= rangedAttackCooldown / aggressionLevel)
        {
            ChangeState(BossState.RangedAttacking);
            FlipTowardsPlayer();
            isFiring = true;
            StartCoroutine(FireMultipleProjectiles());
            return;
        }

        if (distanceToPlayer <= 10f && Time.time - lastDashTime >= dashCooldown / aggressionLevel && (!lowHealth || Random.value < randomDecisionFactor))
        {
            ChangeState(BossState.Dashing);
            FlipTowardsPlayer();
            isAttacking = true;
            StartCoroutine(DashToPlayer());
            lastDashTime = Time.time;
            return;
        }

        if (distanceToPlayer <= 12f)
        {
            ChangeState(BossState.Chasing);
            ChasePlayer();
            return;
        }
        else
        {
            ChangeState(BossState.Idle); // Đứng im và chuyển về Idle khi player ra khỏi phạm vi 12f
        }
    }

    bool IsPlayerInArea()
    {
        if (pointA == null || pointB == null || player == null) return true; // Nếu không set điểm, luôn true

        float minX = Mathf.Min(pointA.position.x, pointB.position.x);
        float maxX = Mathf.Max(pointA.position.x, pointB.position.x);
        float playerX = player.position.x;

        return playerX >= minX && playerX <= maxX;
    }

    void ChangeState(BossState newState)
    {
        if (currentState != newState)
        {
            currentState = newState;
            UpdateAnimator();
        }
    }

    void UpdateAnimator()
    {
        bool isMoving = currentState == BossState.Chasing || currentState == BossState.Dashing || currentState == BossState.Patrolling;
        animator.SetBool("Run", isMoving);
        animator.SetBool("Jump", currentState == BossState.Teleporting);
        // Removed animator.SetBool("Idle", ...) since the parameter doesn't exist
    }

    void FlipTowardsPlayer()
    {
        if (player == null) return;
        float direction = player.position.x > transform.position.x ? 1f : -1f;
        transform.localScale = new Vector3(direction * Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
    }

    void ChasePlayer()
    {
        if (player == null || !IsPlayerInArea())
        {
            ChangeState(BossState.Idle);
            return;
        }
        FlipTowardsPlayer();
        transform.position = Vector2.MoveTowards(transform.position, player.position, speed * aggressionLevel * Time.deltaTime);
    }

    void Patrol()
    {
        if (teleportPoints.Length == 0) return;
        Transform patrolPoint = teleportPoints[patrolIndex];
        Flip(patrolPoint.position.x);
        transform.position = Vector2.MoveTowards(transform.position, patrolPoint.position, speed * Time.deltaTime);

        if (Vector2.Distance(transform.position, patrolPoint.position) < 0.5f)
        {
            patrolIndex = (patrolIndex + 1) % teleportPoints.Length;
            ChangeState(BossState.Idle);
            nextThinkTime = Time.time + restTimeAfterAction;
        }
    }

    IEnumerator ApproachAndMeleeAttack()
    {
        animator.SetBool("Run", true);

        while (Vector2.Distance(transform.position, player.position) > attackRange * 0.5f && currentState == BossState.MeleeAttacking && IsPlayerInArea())
        {
            FlipTowardsPlayer();
            transform.position = Vector2.MoveTowards(transform.position, player.position, speed * aggressionLevel * Time.deltaTime);
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

        ChangeState(BossState.Idle);
        nextThinkTime = Time.time + restTimeAfterAction;
    }

    IEnumerator FireMultipleProjectiles()
    {
        ResetAttackAnimations();
        animator.SetBool("Attack", true);

        for (int i = 0; i < 3; i++)
        {
            yield return new WaitForSeconds(0.2f / aggressionLevel);
            if (player == null || !IsPlayerInArea()) yield break;
            GameObject projectile = Instantiate(attackProjectile, attackSpawnPoint.position, Quaternion.identity);
            Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
            Vector2 direction = (player.position - transform.position).normalized;
            rb.linearVelocity = direction * 10f * aggressionLevel;
        }

        yield return new WaitForSeconds(0.3f);
        animator.SetBool("Attack", false);
        lastRangedAttackTime = Time.time;
        isFiring = false;

        ChangeState(BossState.Idle);
        nextThinkTime = Time.time + restTimeAfterAction;

        if (enemyHealthTwo.currentHealth <= enemyHealthTwo.maxHealth * 0.5f && Random.value < 0.4f)
        {
            yield return StartCoroutine(TeleportRandomly());
        }
    }

    IEnumerator JumpBack()
    {
        animator.SetBool("Jump", true);
        if (player == null) yield break;
        float direction = transform.position.x > player.position.x ? 1f : -1f;
        Vector3 jumpTarget = new Vector3(transform.position.x + direction * jumpBackDistance, transform.position.y, transform.position.z);

        while (Vector3.Distance(transform.position, jumpTarget) > 0.1f && IsPlayerInArea())
        {
            transform.position = Vector3.MoveTowards(transform.position, jumpTarget, jumpSpeed * Time.deltaTime);
            yield return null;
        }

        animator.SetBool("Jump", false);
        ChangeState(BossState.Idle);
    }

    IEnumerator TeleportRandomly()
    {
        animator.SetBool("Jump", true);
        yield return new WaitForSeconds(0.3f);
        if (teleportPoints.Length > 0)
        {
            Transform targetPoint = teleportPoints[Random.Range(0, teleportPoints.Length)];
            transform.position = targetPoint.position;
        }
        FlipTowardsPlayer();
        animator.SetBool("Jump", false);
        isAttacking = false;

        ChangeState(BossState.Idle);
        nextThinkTime = Time.time + restTimeAfterAction;

        if (Random.value < 0.5f && Time.time - lastRangedAttackTime >= rangedAttackCooldown / 2f)
        {
            yield return StartCoroutine(FireMultipleProjectiles());
        }
    }

    IEnumerator DashToPlayer()
    {
        if (player == null) yield break;
        Vector2 target = player.position;
        float dashSpeed = 10f * aggressionLevel;

        animator.SetBool("Run", true);

        while (Vector2.Distance(transform.position, target) > 0.5f && IsPlayerInArea())
        {
            FlipTowardsPlayer();
            transform.position = Vector2.MoveTowards(transform.position, target, dashSpeed * Time.deltaTime);
            yield return null;
        }

        animator.SetBool("Run", false);

        if (Vector2.Distance(transform.position, player.position) <= attackRange && IsPlayerInArea())
        {
            yield return StartCoroutine(ApproachAndMeleeAttack());
        }

        isAttacking = false;
        ChangeState(BossState.Idle);
        nextThinkTime = Time.time + restTimeAfterAction;
    }

    void ResetAttackAnimations()
    {
        animator.SetBool("Attack", false);
        animator.SetBool("Attack1", false);
        animator.SetBool("Attack2", false);
        animator.SetBool("Jump", false);
        animator.SetBool("Run", false); // Ensure Run is reset
    }

    void Flip(float targetX)
    {
        float direction = targetX > transform.position.x ? 1f : -1f;
        transform.localScale = new Vector3(direction * Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
    }
}