using System.Collections;
using UnityEngine;

public class Dan : MonoBehaviour
{
    private Animator animator;
    private bool isDestroying = false;
    public float speed = 5f;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        transform.Translate(Vector2.down * speed * Time.deltaTime);

    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isDestroying) return;

        if (other.CompareTag("Player"))
        {
            HealthSystem healthSystem = other.gameObject.GetComponent<HealthSystem>();
            if (healthSystem != null)
                healthSystem.TakeDamage(10);

            Destroy(gameObject); // Đạn biến mất ngay khi trúng Player
        }

        /*else if (other.CompareTag("Ground"))
        {
            StartCoroutine(DelayDestroy());
        }
    }

    IEnumerator DelayDestroy()
    {
        isDestroying = true;

        if (animator != null)
            animator.SetBool("Destroy", true);

        yield return new WaitForSeconds(0.1f); // chờ animation chạy

        Destroy(gameObject); // destroy sau khi animation kết thúc
    }*/
    }
}