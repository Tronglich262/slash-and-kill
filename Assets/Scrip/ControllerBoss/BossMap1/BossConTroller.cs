using System.Collections;
using UnityEngine;

public class BossController : MonoBehaviour
{
    public Transform player;
    public float speed = 2f;
    public float attackRange = 5f;  // Tầm tấn công cận chiến
    public float rangedAttackRange = 8f;  // Tầm tấn công từ xa (bắn)
    public float attackCooldown = 2f;
    public int attackDamage = 10;
    public float jumpBackDistance = 2f;
    public float jumpSpeed = 4f;

    public Transform[] teleportPoints;
    public float teleportCooldown = 5f;

    public GameObject attackProjectile;
    public Transform attackSpawnPoint;

    private bool isAttacking = false;
    private bool isFiring = false;  // Flag kiểm tra xem có đang bắn không
    private Animator animator;
    private float lastAttackTime;
    private float lastTeleportTime;

    // Tăng cooldown cho việc bắn để bắn ít hơn
    public float rangedAttackCooldown = 4f;  // Thời gian chờ giữa các lần bắn

    void Start()
    {
        animator = GetComponent<Animator>();
        animator.SetBool("Run", false);
    }

    void Update()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Ưu tiên tấn công cận chiến
        if (!isAttacking && distanceToPlayer <= attackRange && Time.time - lastAttackTime >= attackCooldown)
        {
            Flip(player.position.x);
            animator.SetBool("Run", true);
            StartCoroutine(ApproachAndAttack());
        }
        // Chỉ bắn khi Boss ở ngoài tầm tấn công cận chiến và cooldown bắn đã hết
        else if (!isAttacking && distanceToPlayer > attackRange && distanceToPlayer <= rangedAttackRange && !isFiring && Time.time - lastAttackTime >= rangedAttackCooldown)
        {
            animator.SetBool("Run", false);
            Flip(player.position.x);
            StartCoroutine(FireProjectile());  // Bắn quả cầu từ xa
        }

        // Kiểm tra teleport nếu cooldown hết
        if (Time.time - lastTeleportTime >= teleportCooldown)
        {
            StartCoroutine(TeleportRandomly());
            lastTeleportTime = Time.time;
        }
    }

    void Flip(float targetX)
    {
        if (targetX > transform.position.x)
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        else
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
    }

    IEnumerator ApproachAndAttack()
    {
        isAttacking = true;

        while (Vector2.Distance(transform.position, player.position) > 0.5f)
        {
            transform.position = Vector2.MoveTowards(transform.position, player.position, speed * Time.deltaTime);
            yield return null;
        }

        animator.SetBool("Run", false);
        animator.SetBool("Attack", true);

        yield return new WaitForSeconds(0.3f);

        if (Vector2.Distance(transform.position, player.position) <= attackRange)
        {
            HealthSystem playerHealth = player.GetComponent<HealthSystem>();
            if (playerHealth != null)
                playerHealth.TakeDamage(attackDamage);
        }

        yield return new WaitForSeconds(0.3f);
        animator.SetBool("Attack", false);

        yield return StartCoroutine(JumpBack());

        lastAttackTime = Time.time;
        isAttacking = false;
    }

    IEnumerator FireProjectile()
    {
        isFiring = true;  // Đánh dấu đang bắn

        animator.SetBool("Attack", true);  // Bật animation tấn công

        yield return new WaitForSeconds(0.2f);

        GameObject projectile = Instantiate(attackProjectile, attackSpawnPoint.position, Quaternion.identity);
        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();

        Vector2 direction = (player.position - transform.position).normalized;

        rb.linearVelocity = direction * 10f;  // Sử dụng velocity thay vì linearVelocity

        yield return new WaitForSeconds(0.3f);

        animator.SetBool("Attack", false);

        isFiring = false;  // Đánh dấu kết thúc bắn
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
        Transform targetPoint = teleportPoints[Random.Range(0, teleportPoints.Length)];
        
        yield return new WaitForSeconds(0.5f);

        transform.position = targetPoint.position;

        Flip(player.position.x);
        animator.SetBool("Jump", true);
        yield return new WaitForSeconds(0.2f);
        animator.SetBool("Jump", false);
    }
}
