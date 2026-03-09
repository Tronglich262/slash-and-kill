using System.Collections;
using UnityEngine;

public class BossControllerTwo : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public Transform[] teleportPoints;
    public GameObject attackProjectile;
    public Transform attackSpawnPoint;
    public Transform pointA;
    public Transform pointB;

    [Header("Movement Settings")]
    public float speed = 2f;
    public float dashSpeed = 12f;
    public float jumpBackDistance = 3f;
    public float jumpSpeed = 6f;

    [Header("Combat Settings")]
    public float attackRange = 0.5f;           // Khoảng cách tối đa để đánh melee
    public float meleeAttackMinDistance = 0.15f; // Khoảng cách tối thiểu để đánh trúng
    public float rangedAttackRange = 10f;
    public float meleeAttackCooldown = 2.5f;
    public float rangedAttackCooldown = 4f;
    public float dashCooldown = 8f;
    public float teleportCooldown = 6f;
    public int attackDamage = 10;

    [Header("AI Behavior Settings")]
    public float thinkDelay = 0.5f;        // Thời gian "suy nghĩ" giữa các action
    public float restTimeAfterAction = 1f; // Thời gian nghỉ sau action
    public float aggressionBoostOnLowPlayerHealth = 1.5f; // Tăng aggression khi player yếu
    public float defensiveThreshold = 0.4f; // % máu để boss chuyển sang thế phòng thủ

    [Header("Debug")]
    public bool showDebugLogs = false;

    // State variables
    private bool isAttacking = false;
    private bool isFiring = false;
    private bool battleStarted = false;

    private Animator animator;
    private float lastMeleeAttackTime;
    private float lastRangedAttackTime;
    private float lastTeleportTime;
    private float lastDashTime;
    private float nextThinkTime = 0f;

    private EnemyHealth enemyHealth;
    private HealthSystem playerHealth;

    private enum BossState { Idle, Thinking, Chasing, MeleeAttacking, RangedAttacking, Dashing, Teleporting, Defensive, JumpingBack }
    private BossState currentState = BossState.Idle;

    private int patrolIndex = 0;
    private float aggressionLevel = 1f;

    // Track player health state
    private bool playerWasLowHealth = false;
    private bool bossIsLowHealth = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        enemyHealth = GetComponent<EnemyHealth>();

        if (player == null)
        {
            GameObject foundPlayer = GameObject.FindWithTag("Player");
            if (foundPlayer != null)
            {
                player = foundPlayer.transform;
                playerHealth = foundPlayer.GetComponent<HealthSystem>();
            }
        }

        ResetAnimations();
    }

    void Update()
    {
        if (!battleStarted) return;

        // Dừng tấn công nếu player đã chết
        if (player != null && playerHealth != null && playerHealth.isDead)
        {
            // Player đã chết, dừng mọi hành động
            animator.SetBool("Walk", false);
            animator.SetBool("Attack", false);
            return;
        }
        
        // Check health states
        CheckHealthStates();

        if (Time.time < nextThinkTime || isAttacking || isFiring)
        {
            return;
        }

        EvaluateState();
        UpdateAnimator();
    }

    void CheckHealthStates()
    {
        if (enemyHealth != null)
        {
            bossIsLowHealth = enemyHealth.currentHealth <= enemyHealth.maxHealth * defensiveThreshold;
        }

        if (playerHealth != null)
        {
            float playerHealthPercent = (float)playerHealth.currentHP / playerHealth.maxHP;
            bool playerLowHealth = playerHealthPercent <= 0.3f;
            bool playerCriticalHealth = playerHealthPercent <= 0.15f;

            // Tăng aggression khi player yếu
            if (playerCriticalHealth)
                aggressionLevel = 2f;
            else if (playerLowHealth)
                aggressionLevel = 1.5f;
            else if (bossIsLowHealth)
                aggressionLevel = 1.3f;
            else
                aggressionLevel = 1f;
        }
    }

    void EvaluateState()
    {
        if (player == null || !IsPlayerInArea())
        {
            ChangeState(BossState.Idle);
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // === CHIẾN THUẬT PHÒNG THỦ - KHI BOSS YẾU ===
        if (bossIsLowHealth)
        {
            // Ưu tiên teleport ra xa nếu player gần
            if (distanceToPlayer <= attackRange * 1.5f && Time.time - lastTeleportTime >= teleportCooldown)
            {
                ChangeState(BossState.Teleporting);
                isAttacking = true;
                StartCoroutine(TeleportToSafety());
                lastTeleportTime = Time.time;
                return;
            }

            // Bắn đạn từ xa thay vì lao vào đánh
            if (distanceToPlayer > attackRange && distanceToPlayer <= rangedAttackRange && Time.time - lastRangedAttackTime >= rangedAttackCooldown)
            {
                ChangeState(BossState.RangedAttacking);
                FlipTowardsPlayer();
                isFiring = true;
                StartCoroutine(DefensiveRangedAttack());
                return;
            }

            // Nhảy lui sau khi đánh xong
            if (distanceToPlayer <= attackRange && Time.time - lastMeleeAttackTime >= meleeAttackCooldown)
            {
                ChangeState(BossState.MeleeAttacking);
                FlipTowardsPlayer();
                isAttacking = true;
                StartCoroutine(DefensiveMeleeAttack());
                return;
            }

            // Giữ khoảng cách an toàn
            if (distanceToPlayer < rangedAttackRange * 0.7f)
            {
                ChangeState(BossState.JumpingBack);
                StartCoroutine(JumpBackToSafety());
                return;
            }
        }

        // === CHIẾN THUẬT TẤN CÔNG - KHI PLAYER YẾU ===
        if (aggressionLevel > 1.2f)
        {
            // Dash attack khi player yếu - kết liễu
            if (distanceToPlayer <= 8f && distanceToPlayer > attackRange && Time.time - lastDashTime >= dashCooldown / aggressionLevel)
            {
                ChangeState(BossState.Dashing);
                FlipTowardsPlayer();
                isAttacking = true;
                StartCoroutine(AggressiveDashAttack());
                lastDashTime = Time.time;
                return;
            }

            // Teleport sau lưng player để tấn công bất ngờ
            if (distanceToPlayer <= 5f && Time.time - lastTeleportTime >= teleportCooldown / aggressionLevel && Random.value < 0.3f)
            {
                ChangeState(BossState.Teleporting);
                isAttacking = true;
                StartCoroutine(TeleportBehindPlayer());
                lastTeleportTime = Time.time;
                return;
            }
        }

        // === CHIẾN THUẬT BÌNH THƯỜNG ===
        
        // Melee attack khi đủ gần
        if (distanceToPlayer <= attackRange && Time.time - lastMeleeAttackTime >= meleeAttackCooldown / aggressionLevel)
        {
            ChangeState(BossState.MeleeAttacking);
            FlipTowardsPlayer();
            isAttacking = true;
            StartCoroutine(ApproachAndMeleeAttack());
            return;
        }

        // Ranged attack khi ở khoảng cách trung bình
        if (distanceToPlayer > attackRange && distanceToPlayer <= rangedAttackRange && Time.time - lastRangedAttackTime >= rangedAttackCooldown / aggressionLevel)
        {
            ChangeState(BossState.RangedAttacking);
            FlipTowardsPlayer();
            isFiring = true;
            StartCoroutine(FireMultipleProjectiles());
            return;
        }

        // Dash attack thỉnh thoảng
        if (distanceToPlayer <= 7f && distanceToPlayer > attackRange && Time.time - lastDashTime >= dashCooldown && Random.value < 0.15f)
        {
            ChangeState(BossState.Dashing);
            FlipTowardsPlayer();
            isAttacking = true;
            StartCoroutine(DashToPlayer());
            lastDashTime = Time.time;
            return;
        }

        // Teleport ngẫu nhiên để thay đổi vị trí
        if (Time.time - lastTeleportTime >= teleportCooldown && Random.value < 0.08f)
        {
            ChangeState(BossState.Teleporting);
            isAttacking = true;
            StartCoroutine(TeleportRandomly());
            lastTeleportTime = Time.time;
            return;
        }

        // Chase player
        if (distanceToPlayer <= 12f)
        {
            ChangeState(BossState.Chasing);
            ChasePlayer();
            return;
        }

        // Idle khi player quá xa
        ChangeState(BossState.Idle);
    }

    bool IsPlayerInArea()
    {
        if (pointA == null || pointB == null || player == null) return true;

        float minX = Mathf.Min(pointA.position.x, pointB.position.x);
        float maxX = Mathf.Max(pointA.position.x, pointB.position.x);

        return player.position.x >= minX && player.position.x <= maxX;
    }

    void ChangeState(BossState newState)
    {
        if (currentState != newState)
        {
            currentState = newState;
            if (showDebugLogs) Debug.Log($"Boss State: {newState}");
        }
    }

    void UpdateAnimator()
    {
        bool isMoving = currentState == BossState.Chasing || currentState == BossState.Dashing;
        animator.SetBool("Run", isMoving);
        animator.SetBool("Jump", currentState == BossState.Teleporting || currentState == BossState.JumpingBack);
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

    // === MELEE ATTACK ===
    IEnumerator ApproachAndMeleeAttack()
    {
        animator.SetBool("Run", true);

        // Di chuyển đến cực gần player (trong tầm chém)
        while (Vector2.Distance(transform.position, player.position) > meleeAttackMinDistance && currentState == BossState.MeleeAttacking && IsPlayerInArea())
        {
            FlipTowardsPlayer();
            transform.position = Vector2.MoveTowards(transform.position, player.position, speed * aggressionLevel * Time.deltaTime);
            yield return null;
        }

        animator.SetBool("Run", false);
        ResetAnimations();

        // Random attack animation
        int randomAttack = Random.Range(0, 3);
        switch (randomAttack)
        {
            case 0: animator.SetBool("Attack", true); break;
            case 1: animator.SetBool("Attack1", true); break;
            case 2: animator.SetBool("Attack2", true); break;
        }

        yield return new WaitForSeconds(0.3f);

        // Chỉ gây damage khi player thực sự gần (trong tầm chém)
        if (Vector2.Distance(transform.position, player.position) <= meleeAttackMinDistance + 0.1f)
        {
            DealDamageToPlayer();
        }

        yield return new WaitForSeconds(0.3f);
        ResetAnimations();
        
        // Jump back sau khi đánh
        yield return StartCoroutine(JumpBack());

        lastMeleeAttackTime = Time.time;
        isAttacking = false;
        ChangeState(BossState.Idle);
        nextThinkTime = Time.time + restTimeAfterAction;
    }

    // === DEFENSIVE MELEE - Khi boss yếu ===
    IEnumerator DefensiveMeleeAttack()
    {
        FlipTowardsPlayer();
        
        int randomAttack = Random.Range(0, 3);
        switch (randomAttack)
        {
            case 0: animator.SetBool("Attack", true); break;
            case 1: animator.SetBool("Attack1", true); break;
            case 2: animator.SetBool("Attack2", true); break;
        }

        yield return new WaitForSeconds(0.3f);

        if (Vector2.Distance(transform.position, player.position) <= meleeAttackMinDistance + 0.1f)
        {
            DealDamageToPlayer();
        }

        yield return new WaitForSeconds(0.2f);
        ResetAnimations();

        // Nhảy lui ngay lập tức sau khi đánh
        yield return StartCoroutine(JumpBackToSafety());

        lastMeleeAttackTime = Time.time;
        isAttacking = false;
        ChangeState(BossState.Idle);
        nextThinkTime = Time.time + restTimeAfterAction * 1.5f; // Nghỉ lâu hơn
    }

    // === RANGED ATTACK ===
    IEnumerator FireMultipleProjectiles()
    {
        ResetAnimations();
        animator.SetBool("Attack", true);

        // Bắn 3 đạn với delay
        for (int i = 0; i < 3; i++)
        {
            yield return new WaitForSeconds(0.25f);
            if (player == null || !IsPlayerInArea()) yield break;
            
            SpawnProjectile(1f);
        }

        yield return new WaitForSeconds(0.3f);
        animator.SetBool("Attack", false);
        lastRangedAttackTime = Time.time;
        isFiring = false;

        ChangeState(BossState.Idle);
        nextThinkTime = Time.time + restTimeAfterAction;

        // Khi boss yếu, có thể teleport sau khi bắn
        if (bossIsLowHealth && Random.value < 0.5f && Time.time - lastTeleportTime >= teleportCooldown)
        {
            yield return new WaitForSeconds(0.5f);
            StartCoroutine(TeleportToSafety());
            lastTeleportTime = Time.time;
        }
    }

    // === DEFENSIVE RANGED - Khi boss yếu ===
    IEnumerator DefensiveRangedAttack()
    {
        ResetAnimations();
        animator.SetBool("Attack", true);

        // Bắn nhiều đạn hơn và nhanh hơn
        for (int i = 0; i < 5; i++)
        {
            yield return new WaitForSeconds(0.15f);
            if (player == null || !IsPlayerInArea()) yield break;
            
            SpawnProjectile(1.2f);
        }

        yield return new WaitForSeconds(0.3f);
        animator.SetBool("Attack", false);
        lastRangedAttackTime = Time.time;
        isFiring = false;

        // Sau khi bắn, teleport ra xa
        if (Time.time - lastTeleportTime >= teleportCooldown)
        {
            yield return new WaitForSeconds(0.3f);
            StartCoroutine(TeleportToSafety());
            lastTeleportTime = Time.time;
        }

        ChangeState(BossState.Idle);
        nextThinkTime = Time.time + restTimeAfterAction * 2f;
    }

    void SpawnProjectile(float speedMultiplier)
    {
        if (attackProjectile != null && attackSpawnPoint != null)
        {
            GameObject projectile = Instantiate(attackProjectile, attackSpawnPoint.position, Quaternion.identity);
            Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Vector2 direction = (player.position - transform.position).normalized;
                rb.linearVelocity = direction * 10f * speedMultiplier * aggressionLevel;
            }
        }
    }

    // === DASH ATTACK ===
    IEnumerator DashToPlayer()
    {
        if (player == null) yield break;
        
        animator.SetBool("Run", true);
        
        // Dash đến vị trí player
        Vector2 target = player.position;
        float dashSpeedCurrent = dashSpeed * aggressionLevel;

        while (Vector2.Distance(transform.position, target) > 0.5f && IsPlayerInArea())
        {
            FlipTowardsPlayer();
            transform.position = Vector2.MoveTowards(transform.position, target, dashSpeedCurrent * Time.deltaTime);
            yield return null;
        }

        animator.SetBool("Run", false);

        // Đánh sau dash
        if (Vector2.Distance(transform.position, player.position) <= meleeAttackMinDistance + 0.15f && IsPlayerInArea())
        {
            animator.SetBool("Attack", true);
            yield return new WaitForSeconds(0.2f);
            DealDamageToPlayer();
            yield return new WaitForSeconds(0.2f);
            animator.SetBool("Attack", false);
        }

        yield return StartCoroutine(JumpBack());

        isAttacking = false;
        ChangeState(BossState.Idle);
        nextThinkTime = Time.time + restTimeAfterAction;
    }

    // === AGGRESSIVE DASH - Khi player yếu ===
    IEnumerator AggressiveDashAttack()
    {
        if (player == null) yield break;
        
        // Preview animation
        animator.SetBool("Run", true);
        
        // Dash nhanh hơn
        Vector2 target = player.position;
        float dashSpeedCurrent = dashSpeed * 1.5f;

        while (Vector2.Distance(transform.position, target) > 0.3f && IsPlayerInArea())
        {
            FlipTowardsPlayer();
            transform.position = Vector2.MoveTowards(transform.position, target, dashSpeedCurrent * Time.deltaTime);
            yield return null;
        }

        animator.SetBool("Run", false);

        // Combo attack
        for (int i = 0; i < 2; i++)
        {
            animator.SetBool(i == 0 ? "Attack" : "Attack1", true);
            yield return new WaitForSeconds(0.15f);
            DealDamageToPlayer();
            yield return new WaitForSeconds(0.15f);
            ResetAnimations();
            yield return new WaitForSeconds(0.1f);
        }

        // Nhảy lui
        yield return StartCoroutine(JumpBack());

        isAttacking = false;
        ChangeState(BossState.Idle);
        nextThinkTime = Time.time + restTimeAfterAction;
    }

    // === JUMP BACK ===
    IEnumerator JumpBack()
    {
        if (player == null) yield break;
        
        animator.SetBool("Jump", true);
        float direction = transform.position.x > player.position.x ? 1f : -1f;
        Vector3 jumpTarget = new Vector3(transform.position.x + direction * jumpBackDistance, transform.position.y, transform.position.z);

        while (Vector3.Distance(transform.position, jumpTarget) > 0.1f && IsPlayerInArea())
        {
            transform.position = Vector3.MoveTowards(transform.position, jumpTarget, jumpSpeed * Time.deltaTime);
            yield return null;
        }

        animator.SetBool("Jump", false);
    }

    // === JUMP BACK TO SAFETY - Khi boss yếu ===
    IEnumerator JumpBackToSafety()
    {
        if (player == null) yield break;
        
        animator.SetBool("Jump", true);
        
        // Nhảy về phía ngược với player (ra xa hơn)
        float direction = transform.position.x > player.position.x ? 1f : -1f;
        
        // Tìm điểm an toàn
        Vector3 jumpTarget = transform.position + new Vector3(direction * jumpBackDistance * 1.5f, 0, 0);
        
        // Giới hạn trong khu vực
        if (pointA != null && pointB != null)
        {
            jumpTarget.x = Mathf.Clamp(jumpTarget.x, 
                Mathf.Min(pointA.position.x, pointB.position.x), 
                Mathf.Max(pointA.position.x, pointB.position.x));
        }

        while (Vector3.Distance(transform.position, jumpTarget) > 0.1f && IsPlayerInArea())
        {
            transform.position = Vector3.MoveTowards(transform.position, jumpTarget, jumpSpeed * 1.2f * Time.deltaTime);
            yield return null;
        }

        animator.SetBool("Jump", false);
    }

    // === TELEPORT ===
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
        yield return new WaitForSeconds(0.2f);
        animator.SetBool("Jump", false);
        
        isAttacking = false;
        ChangeState(BossState.Idle);
        nextThinkTime = Time.time + restTimeAfterAction;

        // Có thể bắn sau khi teleport
        if (Random.value < 0.4f && Time.time - lastRangedAttackTime >= rangedAttackCooldown)
        {
            yield return new WaitForSeconds(0.3f);
            StartCoroutine(FireMultipleProjectiles());
        }
    }

    // === TELEPORT TO SAFETY - Khi boss yếu ===
    IEnumerator TeleportToSafety()
    {
        if (showDebugLogs) Debug.Log("Boss teleporting to safety!");
        
        animator.SetBool("Jump", true);
        
        // Tìm điểm xa nhất từ player
        Transform safestPoint = null;
        float maxDistance = 0f;

        foreach (Transform point in teleportPoints)
        {
            float dist = Vector2.Distance(point.position, player.position);
            if (dist > maxDistance)
            {
                maxDistance = dist;
                safestPoint = point;
            }
        }

        yield return new WaitForSeconds(0.3f);

        if (safestPoint != null)
        {
            transform.position = safestPoint.position;
        }
        else if (teleportPoints.Length > 0)
        {
            // Teleport ngẫu nhiên nếu không tìm được điểm an toàn
            Transform targetPoint = teleportPoints[Random.Range(0, teleportPoints.Length)];
            transform.position = targetPoint.position;
        }

        FlipTowardsPlayer();
        yield return new WaitForSeconds(0.2f);
        animator.SetBool("Jump", false);
        
        isAttacking = false;
        ChangeState(BossState.Idle);
        nextThinkTime = Time.time + restTimeAfterAction * 1.5f;
    }

    // === TELEPORT BEHIND PLAYER ===
    IEnumerator TeleportBehindPlayer()
    {
        if (player == null) yield break;
        
        animator.SetBool("Jump", true);
        yield return new WaitForSeconds(0.2f);

        // Teleport ra sau player
        float direction = player.position.x > transform.position.x ? -1f : 1f;
        Vector3 targetPos = player.position + new Vector3(direction * 3f, 0, 0);
        
        // Giới hạn trong khu vực
        if (pointA != null && pointB != null)
        {
            targetPos.x = Mathf.Clamp(targetPos.x, 
                Mathf.Min(pointA.position.x, pointB.position.x), 
                Mathf.Max(pointA.position.x, pointB.position.x));
        }

        transform.position = targetPos;
        
        FlipTowardsPlayer();
        yield return new WaitForSeconds(0.15f);
        
        // Tấn công ngay
        animator.SetBool("Attack", true);
        yield return new WaitForSeconds(0.2f);
        DealDamageToPlayer();
        yield return new WaitForSeconds(0.2f);
        
        animator.SetBool("Attack", false);
        animator.SetBool("Jump", false);
        
        isAttacking = false;
        ChangeState(BossState.Idle);
        nextThinkTime = Time.time + restTimeAfterAction;
    }

    void DealDamageToPlayer()
    {
        if (player != null)
        {
            HealthSystem playerHealthSystem = player.GetComponent<HealthSystem>();
            if (playerHealthSystem != null)
            {
                playerHealthSystem.TakeDamage(attackDamage);
                if (showDebugLogs) Debug.Log($"Boss dealt {attackDamage} damage to player!");
            }
        }
    }

    void ResetAnimations()
    {
        animator.SetBool("Run", false);
        animator.SetBool("Attack", false);
        animator.SetBool("Attack1", false);
        animator.SetBool("Attack2", false);
        animator.SetBool("Jump", false);
    }

    // Được gọi bởi BossIntroManager
    public void StartBattle()
    {
        battleStarted = true;
        nextThinkTime = Time.time + 1f; // Đợi 1 giây trước khi bắt đầu
        if (showDebugLogs) Debug.Log("Boss Battle Started - AI Enhanced!");
    }
}
