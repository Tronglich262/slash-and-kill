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

            Destroy(gameObject); 
        }

    }
}