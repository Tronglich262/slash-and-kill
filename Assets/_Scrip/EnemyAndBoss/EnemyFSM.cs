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
    public float attackRange = 2f; 
    public float retreatDistance = 1.5f; 
    public float attackCooldown = 2f; // Thời gian chờ giữa các lần tấn công
    public float smartAttackRange = 4f; // Khoảng cách để quái thông minh tấn công
    private bool isAttacking = false;
    private Transform target;
    private Animator animator;
    public int attackDamage = 10; // Sát thương gây ra cho Player
    private Vector2 originalPosition; // Vị trí gốc để quái đất không bay lên
    private bool isUpgraded = false; // Đánh dấu quái bay đã nâng cấp
    private float lastAttackTime = -10f; // Thời gian tấn công cuối cùng
    
    // Thuộc tính riêng cho Skeleton
    public float skeletonAttackInterval = 1f; // Thời gian giữa các đòn đánh
    public float skeletonWaitAfterAttack = 0.5f; // Thời gian chờ sau mỗi đòn đánh

    void Start()
    {
        animator = GetComponent<Animator>(); 
        target = pointB; 
        originalPosition = transform.position; 
        animator.SetBool("Walk1", true);

        if (player == null)
        {
            GameObject foundPlayer = GameObject.FindWithTag("Player"); 
            if (foundPlayer != null)
                player = foundPlayer.transform;
        }

    }

    void Update()
    {
        if (isAttacking) return; 

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        float currentTime = Time.time;

        // Quái bay: nâng cấp khi player đi qua khỏi vùng mục tiêu
        if (enemyType == EnemyType.Flying && !isUpgraded && target != null)
        {
            float playerDistToTarget = Vector2.Distance(new Vector2(player.position.x, 0), new Vector2(target.position.x, 0));
            float enemyDistToTarget = Vector2.Distance(new Vector2(transform.position.x, 0), new Vector2(target.position.x, 0));
            
            if (playerDistToTarget > enemyDistToTarget + 2f)
            {
                isUpgraded = true;
                speed *= 1.5f;
                attackDamage *= 2;
                attackCooldown *= 1.5f;
                Debug.Log("Quái bay nâng cấp! Speed: " + speed + ", Damage: " + attackDamage);
            }
        }

        // Quái bay: Tấn công thông minh - có cooldown và chỉ tấn công khi player gần
        if (enemyType == EnemyType.Flying)
        {
           
            if (distanceToPlayer > attackRange)
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
            if (distanceToPlayer <= attackRange)
            {
                lastAttackTime = currentTime;
                StartCoroutine(ChargeAttack());
                return;
            }
            
            MoveBetweenPoints();
            return;
        }

        // Quái Skeleton: hành vi đơn giản như EnemyAI
        if (enemyType == EnemyType.Skeleton)
        {
            if (distanceToPlayer <= attackRange)
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
                    MoveBetweenPoints();
                    animator.SetBool("Walk1", true);
                }
            }
            return;
        }

        // Quái dưới đất: hành vi bình thường
        if (distanceToPlayer <= attackRange)
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

        if (Vector2.Distance(transform.position, moveTarget) < 0.1f)
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

    IEnumerator ChargeAttack()
    {
        isAttacking = true;

        // Xoay mặt về phía Player trước khi lao vào
        Flip(player.position.x);

       
        animator.SetBool("Walk1", true);
        while (Vector2.Distance(transform.position, player.position) > 0.5f)
        {
            
            if (Vector2.Distance(transform.position, player.position) > attackRange)
            {
                isAttacking = false;
                animator.SetBool("Walk1", true);
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

        animator.SetBool("Walk1", false);
        animator.SetBool("Attack1", true);
        
        DealDamageToPlayer();
        
        yield return new WaitForSeconds(1f);
        
        // Reset attack animation
        animator.SetBool("Attack1", false);

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
            if (Vector2.Distance(transform.position, player.position) > attackRange)
            {
                isAttacking = false;
                animator.SetBool("Walk1", true);
                yield break;
            }

            transform.position = Vector3.MoveTowards(transform.position, retreatTarget, speed * Time.deltaTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Quái bay: không tấn công liên tục, chờ cooldown
        if (enemyType == EnemyType.Flying)
        {
            animator.SetBool("Walk1", true);
            isAttacking = false;
        }
        else
        {
            // Quái đất: tấn công liên tục nếu player còn trong vùng
            if (Vector2.Distance(transform.position, player.position) <= attackRange)
            {
                yield return new WaitForSeconds(0.5f); 
                StartCoroutine(ChargeAttack()); 
            }
            else
            {
                animator.SetBool("Walk1", true);
                isAttacking = false;
            }
        }
    }

    private void DealDamageToPlayer()
    {
        if (player != null)
        {
            HealthSystem playerHealth = player.GetComponent<HealthSystem>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(attackDamage);
                Debug.Log("Enemy gây " + attackDamage + " damage cho Player!");
            }
        }
    }

    // Skeleton Attack - tương tự EnemyAI
    IEnumerator SkeletonAttack()
    {
        isAttacking = true;

        while (Vector2.Distance(transform.position, player.position) <= attackRange)
        {
            animator.SetBool("Walk1", false);
            animator.SetBool("Attack1", true);

            // Gây sát thương cho Player
            DealDamageToPlayer();

            yield return new WaitForSeconds(skeletonAttackInterval); // thời gian giữa các đòn đánh

            animator.SetBool("Attack1", false); // chuyển về idle nhẹ

            yield return new WaitForSeconds(skeletonWaitAfterAttack); // chờ thêm 1 lúc trước khi đánh tiếp
        }

        animator.SetBool("Walk1", true);
        animator.SetBool("Attack1", false);
        isAttacking = false;
    }
}