using UnityEngine;

public class CayAI : MonoBehaviour
{
    public Transform player;
    public float attackRange = 1f;
    public float attackCooldown = 1.5f;

    private Animator animator;
    private bool isFacingRight = true;
    private bool canAttack = true;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= attackRange && canAttack)
        {
            StartCoroutine(Attack());
        }
        else if (distanceToPlayer > attackRange)
        {
            animator.SetBool("Attack1", false);
        }
    }

    System.Collections.IEnumerator Attack()
    {
        canAttack = false;
        animator.SetBool("Attack1", true);
        FlipToFacePlayer();

        yield return new WaitForSeconds(0.5f); // animation attack đang diễn ra

        animator.SetBool("Attack1", false); // tắt attack sau animation (tùy vào style animation bạn set)

        yield return new WaitForSeconds(attackCooldown); // cooldown giữa các lần tấn công

        canAttack = true;
    }

    void FlipToFacePlayer()
    {
        float direction = player.position.x - transform.position.x;

        if ((direction < 0 && !isFacingRight) || (direction > 0 && isFacingRight))
        {
            isFacingRight = !isFacingRight;
            Vector3 scale = transform.localScale;
            scale.x *= -1;
            transform.localScale = scale;
        }
    }
}