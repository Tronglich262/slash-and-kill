using System;
using System.Collections;
using UnityEngine;

public class EnemyFSMVII : MonoBehaviour
{
    public Transform pointA, pointB; 
    public Transform player; 
    public float speed = 2f; 
    public float attackRange = 2f; 
    public float retreatDistance = 1.5f; 
    private bool isAttacking = false;
    private Transform target;
    private Animator animator;
    public int attackDamage = 10; // Sát thương gây ra cho Player

    void Start()
    {
        animator = GetComponent<Animator>(); 
        target = pointB; // Bắt đầu tuần tra về điểm B
        animator.SetBool("Walk1", true); 
    }

    void Update()
    {
        if (isAttacking) return; 

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

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
        transform.position = Vector2.MoveTowards(transform.position, target.position, speed * Time.deltaTime);

        if (Vector2.Distance(transform.position, target.position) < 0.1f)
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

        // Bước 1: Lao vào Player
        animator.SetBool("Walk1", true);
        while (Vector2.Distance(transform.position, player.position) > 0.5f)
        {
            transform.position = Vector2.MoveTowards(transform.position, player.position, speed * Time.deltaTime * 2);
            yield return null;
        }

        // Bước 2: Tấn công
        animator.SetBool("Walk1", false);
        animator.SetTrigger("Attack1");
        yield return new WaitForSeconds(1f);

        // Bước 3: Lùi lại
        float direction = (transform.position.x > player.position.x) ? 1f : -1f;
        Vector3 retreatTarget = new Vector3(transform.position.x + (direction * retreatDistance), transform.position.y, transform.position.z);

        float retreatTime = 0.5f;
        float elapsedTime = 0f;
        while (elapsedTime < retreatTime)
        {
            transform.position = Vector3.MoveTowards(transform.position, retreatTarget, speed * Time.deltaTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Bước 4: Kiểm tra Player có còn trong phạm vi không
        if (Vector2.Distance(transform.position, player.position) <= attackRange)
        {
            yield return new WaitForSeconds(0.5f); // Delay giữa các lần tấn công
            StartCoroutine(ChargeAttack()); // Tấn công tiếp
        }
        else
        {
            animator.SetBool("Walk1", true);
            isAttacking = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            Debug.Log("Đã va chạm với Player");
            HealthSystem playerHealth = other.gameObject.GetComponent<HealthSystem>(); // Lấy HealthSystem từ Player
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(attackDamage); // Gọi TakeDamage trên HealthSystem của Player
            }
        }
    }
}