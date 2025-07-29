using System.Collections;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public Transform pointA, pointB;
    public Transform player;
    public float speed = 2f;
    public float attackRange = 2f;

    private Transform target;
    private bool isAttacking = false;
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
        target = pointB;
        SetAnimationState(isWalking: true, isAttacking: false);
    }

    void Update()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= attackRange)
        {
            if (!isAttacking)
            {
                StartCoroutine(AttackPlayer());
            }

            FlipToPlayer();
        }
        else
        {
            if (!isAttacking)
            {
                MoveBetweenPoints();
                SetAnimationState(isWalking: true, isAttacking: false);
            }
        }
    }

    void MoveBetweenPoints()
    {
        transform.position = Vector2.MoveTowards(transform.position, target.position, speed * Time.deltaTime);

        if (Vector2.Distance(transform.position, target.position) < 0.1f)
        {
            target = (target == pointA) ? pointB : pointA;
            Flip();
        }
    }

    void Flip()
    {
        Vector3 scale = transform.localScale;
        scale.x = (target == pointB) ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
        transform.localScale = scale;
    }

    void FlipToPlayer()
    {
        Vector3 scale = transform.localScale;
        scale.x = (player.position.x > transform.position.x) ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
        transform.localScale = scale;
    }

    void SetAnimationState(bool isWalking, bool isAttacking)
    {
        animator.SetBool("Walk1", isWalking);
        animator.SetBool("Attack1", isAttacking);
    }

    IEnumerator AttackPlayer()
    {
        isAttacking = true;

        while (Vector2.Distance(transform.position, player.position) <= attackRange)
        {
            SetAnimationState(isWalking: false, isAttacking: true);

            // Gây sát thương tại đây nếu cần

            yield return new WaitForSeconds(1f); // thời gian giữa các đòn đánh

            SetAnimationState(isWalking: false, isAttacking: false); // chuyển về idle nhẹ

            yield return new WaitForSeconds(0.5f); // chờ thêm 1 lúc trước khi đánh tiếp
        }

        SetAnimationState(isWalking: true, isAttacking: false);
        isAttacking = false;
    }
}
