using System;
using System.Collections;
using UnityEngine;

public enum EnemyType
{
    Flying,  
    Ground,
    Skeleton
}

public class EnemyFSM : MonoBehaviour
{
    public EnemyType enemyType = EnemyType.Ground; 
    public Transform pointA, pointB; 
    public Transform player; 
    public float speed = 2f; 
    public float attackRange = 0.8f; 
    public float retreatDistance = 1.5f; 
    public float attackCooldown = 2f; // Thời gian chờ giữa các lần tấn công
    public float smartAttackRange = 4f; // Khoảng cách để quái thông minh tấn công
    private bool isAttacking = false;
    private Transform target;
    private Animator animator;
    private static readonly int WalkParameter = Animator.StringToHash("Walk1");
    private static readonly int AttackParameter = Animator.StringToHash("Attack1");
    private bool walkAnimationState;
    private bool attackAnimationState;
    private bool hasWalkAnimationState;
    private bool hasAttackAnimationState;
    private HealthSystem playerHealth;
    private float nextPlayerSearchTime;
    private const float PlayerSearchInterval = 0.5f;
    [Header("Attack Hit Window")]
    [SerializeField, Min(0f)] private float attackHitDelay = 0.5f;
    [SerializeField, Min(0.1f)] private float attackAnimationDuration = 1f;
    [SerializeField, Min(0.1f)] private float attackHitRangeMultiplier = 1.2f;
    [SerializeField, Min(0.1f)] private float attackVerticalTolerance = 0.85f;
    private WaitForSeconds attackWindupDelay;
    private WaitForSeconds attackFollowThroughDelay;
    private WaitForSeconds skeletonFollowThroughDelay;
    private static readonly WaitForSeconds ChargeRepeatDelay = new WaitForSeconds(0.5f);
    private WaitForSeconds skeletonRecoveryDelay;
    public int attackDamage = 10; // Sát thương gây ra cho Player
    private Vector2 originalPosition; // Vị trí gốc để quái đất không bay lên
    private bool isUpgraded = false; // Đánh dấu quái bay đã nâng cấp
    private float lastAttackTime = -10f; // Thời gian tấn công cuối cùng
    
    // Thuộc tính riêng cho Skeleton
    public float skeletonAttackInterval = 1f; // Thời gian giữa các đòn đánh
    public float skeletonWaitAfterAttack = 0.5f; // Thời gian chờ sau mỗi đòn đánh

    // Knockback state - kiểm tra xem enemy có đang bị đẩy không
    private bool isKnockback = false;
    private bool chaseSummonerTarget;
    private bool attackDamageApplied;
    private bool isDead;
    public event Action<EnemyFSM> PlayerDamaged;

    void Start()
    {
        animator = GetComponent<Animator>(); 
        skeletonRecoveryDelay = new WaitForSeconds(skeletonWaitAfterAttack);
        attackWindupDelay = new WaitForSeconds(attackHitDelay);
        attackFollowThroughDelay = new WaitForSeconds(
            Mathf.Max(0.02f, attackAnimationDuration - attackHitDelay));
        skeletonFollowThroughDelay = new WaitForSeconds(
            Mathf.Max(0.02f, skeletonAttackInterval - attackHitDelay));
        target = pointB; 
        originalPosition = transform.position; 
        SetWalk(true);

        TryResolvePlayer();
    }

    void Update()
    {
        if (isDead)
            return;

        if (!TryResolvePlayer())
            return;

        // Dừng tấn công nếu player đã chết
        if (playerHealth != null && playerHealth.isDead)
        {
            // Player đã chết, dừng mọi hành động
            SetWalk(false);
            SetAttack(false);
            return;
        }

        if (isAttacking || isKnockback) return; // Dừng lại nếu đang tấn công hoặc bị knockback

        float sqrDistanceToPlayer = SqrDistanceToPlayer();
        float attackRangeSqr = attackRange * attackRange;
        float currentTime = Time.time;

        // Quái bay
        if (enemyType == EnemyType.Flying && !isUpgraded && target != null)
        {
            float playerDistToTarget = Mathf.Abs(player.position.x - target.position.x);
            float enemyDistToTarget = Mathf.Abs(transform.position.x - target.position.x);
            
            if (playerDistToTarget > enemyDistToTarget + 2f)
            {
                isUpgraded = true;
                speed *= 1.5f;
                attackDamage *= 2;
                attackCooldown *= 1.5f;
                Debug.Log("Quái bay nâng cấp! Speed: " + speed + ", Damage: " + attackDamage);
            }
        }

        // Quái bay
        if (enemyType == EnemyType.Flying)
        {
           
            if (sqrDistanceToPlayer > attackRangeSqr)
            {
                MoveBetweenPoints();
                return;
            }

            // Kiểm tra cooldown
            if (currentTime - lastAttackTime < attackCooldown)
            {
                MoveBetweenPoints();
                return;
            }

            // Chỉ tấn công khi player TRONG vùng tấn công VÀ hết cooldown
            if (sqrDistanceToPlayer <= attackRangeSqr)
            {
                lastAttackTime = currentTime;
                StartCoroutine(ChargeAttack());
                return;
            }
            
            MoveBetweenPoints();
            return;
        }

        // Quái Skeleton
        if (enemyType == EnemyType.Skeleton)
        {
            if (sqrDistanceToPlayer <= attackRangeSqr)
            {
                if (!isAttacking)
                {
                    Flip(player.position.x);
                    StartCoroutine(SkeletonAttack());
                }
            }
            else
            {
                if (!isAttacking)
                {
                    if (chaseSummonerTarget)
                        ChaseAssignedPlayer();
                    else
                        MoveBetweenPoints();
                    SetWalk(true);
                }
            }
            return;
        }

        // Quái dưới đất: hành vi bình thường
        if (sqrDistanceToPlayer <= attackRangeSqr)
        {
            StartCoroutine(ChargeAttack());
        }
        else
        {
            MoveBetweenPoints();
        }
    }

    void MoveBetweenPoints()
    {
        // Quái dưới đất: giữ nguyên vị trí Y, không bay lên
        Vector3 moveTarget = target.position;
        if (enemyType == EnemyType.Ground)
        {
            moveTarget.y = originalPosition.y;
        }

        transform.position = Vector2.MoveTowards(transform.position, moveTarget, speed * Time.deltaTime);

        if ((transform.position - moveTarget).sqrMagnitude < 0.01f)
        {
            target = (target == pointA) ? pointB : pointA; // Đổi hướng
            Flip(target.position.x);
        }
    }

    void Flip(float targetX)
    {
        // Nếu mục tiêu bên phải, hướng sang phải; nếu mục tiêu bên trái, hướng sang trái
        if (targetX > transform.position.x)
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        else
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
    }

    private void ChaseAssignedPlayer()
    {
        if (player == null)
            return;

        Vector3 targetPosition = player.position;
        // Summoned skeletons are ground units: chase horizontally and keep
        // their feet at their own spawn height.
        targetPosition.y = originalPosition.y;
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            speed * Time.deltaTime);
        Flip(player.position.x);
    }

    public void SetSummonedTarget(Transform targetPlayer)
    {
        player = targetPlayer;
        playerHealth = player != null ? player.GetComponent<HealthSystem>() : null;
        chaseSummonerTarget = player != null;
    }

    private void SetWalk(bool value)
    {
        if (hasWalkAnimationState && walkAnimationState == value)
            return;

        walkAnimationState = value;
        hasWalkAnimationState = true;
        animator.SetBool(WalkParameter, value);
    }

    private void SetAttack(bool value)
    {
        if (hasAttackAnimationState && attackAnimationState == value)
            return;

        if (value)
            attackDamageApplied = false;

        attackAnimationState = value;
        hasAttackAnimationState = true;
        animator.SetBool(AttackParameter, value);
    }

    IEnumerator ChargeAttack()
    {
        isAttacking = true;

        // Xoay mặt về phía Player trước khi lao vào
        Flip(player.position.x);

       
        SetWalk(true);
        float attackRangeSqr = attackRange * attackRange;
        while (SqrDistanceToPlayer() > 0.25f)
        {
            
            if (SqrDistanceToPlayer() > attackRangeSqr)
            {
                isAttacking = false;
                SetWalk(true);
                yield break;
            }

            Vector3 movePos = Vector2.MoveTowards(transform.position, player.position, speed * Time.deltaTime * 2);
           
            if (enemyType == EnemyType.Ground)
            {
                movePos.y = originalPosition.y;
            }
            transform.position = movePos;
            yield return null;
        }

        SetWalk(false);
        SetAttack(true);

        // Body contact is harmless. Damage is evaluated only at the weapon hit frame.
        yield return attackWindupDelay;
        TryDealAttackDamage();
        yield return attackFollowThroughDelay;
        
        // Reset attack animation
        SetAttack(false);

        // Bước 3: Lùi lại
        float direction = (transform.position.x > player.position.x) ? 1f : -1f;
        Vector3 retreatTarget = new Vector3(transform.position.x + (direction * retreatDistance), transform.position.y, transform.position.z);
        
        if (enemyType == EnemyType.Ground)
        {
            retreatTarget.y = originalPosition.y;
        }

        float retreatTime = 0.5f;
        float elapsedTime = 0f;
        while (elapsedTime < retreatTime)
        {
            if (SqrDistanceToPlayer() > attackRangeSqr)
            {
                isAttacking = false;
                SetWalk(true);
                yield break;
            }

            transform.position = Vector3.MoveTowards(transform.position, retreatTarget, speed * Time.deltaTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Quái bay: không tấn công liên tục, chờ cooldown
        if (enemyType == EnemyType.Flying)
        {
            SetWalk(true);
            isAttacking = false;
        }
        else
        {
            // Quái đất: tấn công liên tục nếu player còn trong vùng
            if (SqrDistanceToPlayer() <= attackRangeSqr)
            {
                yield return ChargeRepeatDelay;
                StartCoroutine(ChargeAttack()); 
            }
            else
            {
                SetWalk(true);
                isAttacking = false;
            }
        }
    }

    private void TryDealAttackDamage()
    {
        if (playerHealth == null)
            TryResolvePlayer();
        if (playerHealth == null)
            playerHealth = HealthSystem.Instance;

        if (!isAttacking || !attackAnimationState || attackDamageApplied ||
            playerHealth == null || playerHealth.isDead ||
            !IsPlayerInsideAttackHitbox())
            return;

        attackDamageApplied = true;
        int healthBefore = playerHealth.currentHP;
        playerHealth.TakeDamage(attackDamage);
        if (playerHealth.currentHP < healthBefore)
            PlayerDamaged?.Invoke(this);
    }

    private bool IsPlayerInsideAttackHitbox()
    {
        if (player == null)
            return false;

        Collider2D enemyCollider = GetComponent<Collider2D>();
        Collider2D targetCollider = player.GetComponent<Collider2D>();
        if (targetCollider == null)
            targetCollider = player.GetComponentInChildren<Collider2D>();

        float hitRange = Mathf.Max(0.1f, attackRange * attackHitRangeMultiplier);
        if (enemyCollider != null && targetCollider != null &&
            enemyCollider.enabled && targetCollider.enabled)
        {
            ColliderDistance2D distance = enemyCollider.Distance(targetCollider);
            return distance.isOverlapped || distance.distance <= hitRange;
        }

        Vector2 delta = player.position - transform.position;
        return Mathf.Abs(delta.x) <= hitRange &&
               Mathf.Abs(delta.y) <= Mathf.Max(attackVerticalTolerance, hitRange);
    }

    // Animation Event on skill_1 calls this at the sword contact frame.
    public void ApplyAttackHit()
    {
        TryDealAttackDamage();
    }

    private float SqrDistanceToPlayer()
    {
        return ((Vector2)(transform.position - player.position)).sqrMagnitude;
    }

    private bool TryResolvePlayer()
    {
        if (player != null)
        {
            if (playerHealth == null)
                player.TryGetComponent(out playerHealth);

            return true;
        }

        playerHealth = null;

        if (Time.unscaledTime < nextPlayerSearchTime)
            return false;

        nextPlayerSearchTime = Time.unscaledTime + PlayerSearchInterval;
        GameObject foundPlayer = GameObject.FindWithTag("Player");
        if (foundPlayer == null)
            return false;

        player = foundPlayer.transform;
        foundPlayer.TryGetComponent(out playerHealth);
        return true;
    }

    // Skeleton Attack
    IEnumerator SkeletonAttack()
    {
        isAttacking = true;

        float attackRangeSqr = attackRange * attackRange;
        while (SqrDistanceToPlayer() <= attackRangeSqr)
        {
            SetWalk(false);
            SetAttack(true);

            // The slash can miss if the player leaves the hitbox during wind-up.
            yield return attackWindupDelay;
            TryDealAttackDamage();
            yield return skeletonFollowThroughDelay;
            SetAttack(false); // chuyển về idle nhẹ

            yield return skeletonRecoveryDelay; // chờ thêm 1 lúc trước khi đánh tiếp
        }

        SetWalk(true);
        SetAttack(false);
        isAttacking = false;
    }

    // Called by EnemyHealth at the exact lethal-damage frame.
    public void SetKnockbackState(bool knockback)
    {
        if (!isDead)
            isKnockback = knockback;
    }

    public void EnterDeathState()
    {
        if (isDead)
            return;

        isDead = true;
        StopAllCoroutines();
        isAttacking = false;
        isKnockback = false;
        SetWalk(false);
        SetAttack(false);
        enabled = false;
    }
}
