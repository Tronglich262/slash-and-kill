using System.Collections;
using UnityEngine;

public class BossController : MonoBehaviour
{
    public Transform player;
    public float speed = 2f;
    public float attackRange = 1f;  // Tầm tấn công cận chiến
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

    // Trạng thái battle - không tấn công cho đến khi intro kết thúc
    private bool battleStarted = false;

    // Chức năng nhảy lên trời (Sky Jump)
    public float skyJumpCooldown = 8f;  // Thời gian giữa các lần nhảy lên trời
    public float skyJumpHeight = 5f;    // Độ cao khi nhảy lên
    public float skyJumpDuration = 1.5f; // Thời gian giữ trạng thái nhảy
    public float skyJumpSpeed = 6f;
    public float skyJumpChance = 0.3f;  // 30% cơ hội nhảy mỗi khi cooldown đủ
    private float lastSkyJumpTime = -100f;  // Bắt đầu với giá trị âm để có thể nhảy ngay lần đầu
    private bool isSkyJumping = false;
    private bool skyJumpCooldownReady = false;  // Flag để kiểm tra cooldown đã sẵn sàng chưa

    void Start()
    {
        animator = GetComponent<Animator>();
        animator.SetBool("Run", false);

        if (player == null)
        {
            GameObject foundPlayer = GameObject.FindWithTag("Player"); // Tìm theo Tag
            if (foundPlayer != null)
                player = foundPlayer.transform;
        }

    }

    void Update()
    {
        // Không làm gì cho đến khi battle bắt đầu (sau intro)
        if (!battleStarted) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Kiểm tra nhảy lên trời (Sky Jump) - chỉ khi không đang tấn công và không đang nhảy
        if (!isAttacking && !isSkyJumping && !isFiring)
        {
            // Kiểm tra nếu cooldown vừa mới sẵn sàng (chưa được kiểm tra lần nào)
            if (!skyJumpCooldownReady && Time.time - lastSkyJumpTime >= skyJumpCooldown)
            {
                skyJumpCooldownReady = true;  // Đánh dấu là đã sẵn sàng để kiểm tra
            }

            // Chỉ kiểm tra chance MỘT LẦN khi cooldown vừa sẵn sàng
            if (skyJumpCooldownReady)
            {
                if (Random.Range(0f, 1f) < skyJumpChance)
                {
                    StartCoroutine(SkyJump());
                    lastSkyJumpTime = Time.time;
                    skyJumpCooldownReady = false;  // Reset flag sau khi đã nhảy
                }
                else
                {
                    // Nếu không nhảy lần này, reset cooldown để không kiểm tra nữa cho đến khi hết cooldown
                    skyJumpCooldownReady = false;
                    lastSkyJumpTime = Time.time;  // Bắt đầu đếm cooldown mới
                }
                return;  // Không làm gì khác trong frame này
            }
        }

        // Ưu tiên tấn công cận chiến (không tấn công khi đang sky jump)
        if (!isAttacking && !isSkyJumping && distanceToPlayer <= attackRange && Time.time - lastAttackTime >= attackCooldown)
        {
            Flip(player.position.x);
            animator.SetBool("Run", true);
            StartCoroutine(ApproachAndAttack());
        }
        // Chỉ bắn khi Boss ở ngoài tầm tấn công cận chiến và cooldown bắn đã hết
        else if (!isAttacking && !isSkyJumping && distanceToPlayer > attackRange && distanceToPlayer <= rangedAttackRange && !isFiring && Time.time - lastAttackTime >= rangedAttackCooldown)
        {
            animator.SetBool("Run", false);
            Flip(player.position.x);
            StartCoroutine(FireProjectile());  // Bắn quả cầu từ xa
        }

        // Kiểm tra teleport nếu cooldown hết (không teleport khi đang sky jump)
        if (!isSkyJumping && Time.time - lastTeleportTime >= teleportCooldown)
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

        if (Vector2.Distance(transform.position, player.position) <= attackRange * 0.5f)
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

    // Coroutine nhảy lên trời - thể hiện sức mạnh boss
    IEnumerator SkyJump()
    {
        isSkyJumping = true;
        
        // Tính vị trí cao hơn
        Vector3 skyPosition = new Vector3(transform.position.x, transform.position.y + skyJumpHeight, transform.position.z);

        // Nhảy lên cao
        animator.SetBool("Jump", true);
        
        while (Vector3.Distance(transform.position, skyPosition) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, skyPosition, skyJumpSpeed * Time.deltaTime);
            yield return null;
        }

        // Bắn projectile xuống dưới khi đang lơ lửng trên trời
        int projectileCount = Random.Range(1, 3);  // 1 hoặc 2 cái
        for (int i = 0; i < projectileCount; i++)
        {
            yield return new WaitForSeconds(0.2f);
            FireProjectileDown();
        }

        // Giữ trạng thái ở trên cao - boss đang "lơ lửng" thể hiện sức mạnh
        yield return new WaitForSeconds(skyJumpDuration);

        // Hạ xuống
        Vector3 groundPosition = new Vector3(transform.position.x, transform.position.y - skyJumpHeight, transform.position.z);
        
        while (Vector3.Distance(transform.position, groundPosition) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, groundPosition, skyJumpSpeed * Time.deltaTime);
            yield return null;
        }

        animator.SetBool("Jump", false);
        isSkyJumping = false;
    }

    // Hàm bắn projectile hướng về phía player
    void FireProjectileDown()
    {
        if (attackProjectile != null && attackSpawnPoint != null && player != null)
        {
            GameObject projectile = Instantiate(attackProjectile, attackSpawnPoint.position, Quaternion.identity);
            Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();

            // Tính hướng từ vị trí boss (đang ở trên trời) đến player
            Vector2 direction = (player.position - transform.position).normalized;

            rb.linearVelocity = direction * 10f;
        }
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

    // Phương thức được gọi bởi BossIntroManager sau khi intro kết thúc
    public void StartBattle()
    {
        battleStarted = true;
        Debug.Log("Boss Battle Started!");
    }
}
