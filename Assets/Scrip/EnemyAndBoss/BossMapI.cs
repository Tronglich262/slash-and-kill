using System.Collections;
using UnityEngine;

public class BosMapI : MonoBehaviour
{
    public Transform pointA, pointB; // Điểm A & B
    public Transform player; // Player
    public float speed = 2f;
    public float attackRange = 2f; // Khoảng cách để attack
    private Transform target;
    private bool isAttacking = false;
    
    private Animator animator; // Animator của Enemy

    void Start()
    {
        animator = GetComponent<Animator>(); // Lấy Animator có sẵn
        target = pointB; // Enemy bắt đầu di chuyển về B
        animator.SetBool("Walk1", true); // Bắt đầu đi
    }

    void Update()
    {
        if (isAttacking) return; // Nếu đang attack thì không di chuyển

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= attackRange) // Nếu Player vào phạm vi tấn công
        {
            StartCoroutine(AttackPlayer());
        }
        else
        {
            MoveBetweenPoints(); // Nếu không có Player, tiếp tục di chuyển giữa A và B
        }
    }

    void MoveBetweenPoints()
    {
        transform.position = Vector2.MoveTowards(transform.position, target.position, speed * Time.deltaTime);

        if (Vector2.Distance(transform.position, target.position) < 0.1f)
        {
            target = (target == pointA) ? pointB : pointA; // Đổi hướng
            Flip();
        }
    }

    void Flip()
    {
        Vector3 scale = transform.localScale;
        scale.x = (target == pointB) ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x); // Đổi hướng theo vị trí
        transform.localScale = scale;
    }

    IEnumerator AttackPlayer()
    {
        isAttacking = true;
        animator.SetBool("Walk1", false);
        animator.SetBool("Attackbos", true);

        yield return new WaitForSeconds(1f); // Thời gian attack

        animator.SetBool("Attackbos", false);
        animator.SetBool("Walk1", true);

        isAttacking = false;
    }
}