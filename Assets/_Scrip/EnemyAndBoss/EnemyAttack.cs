using UnityEngine;
using System.Collections.Generic;

public class EnemyAttack : MonoBehaviour
{
    public int damage = 10;
    private readonly List<Collider2D> hitBuffer = new List<Collider2D>(8);
    private ContactFilter2D contactFilter;

    private void Awake()
    {
        contactFilter = new ContactFilter2D();
        contactFilter.NoFilter();
        contactFilter.useTriggers = true;
    }

    private void DoDamage()
    {
        // Gây sát thương cho Player nếu còn trong vùng đánh
        hitBuffer.Clear();
        Physics2D.OverlapCircle(transform.position, 1.5f, contactFilter, hitBuffer);

        for (int i = 0; i < hitBuffer.Count; i++)
        {
            if (hitBuffer[i].CompareTag("Player") &&
                hitBuffer[i].TryGetComponent(out HealthSystem healthSystem))
            {
                healthSystem.TakeDamage(damage);
                break;
            }
        }
    }

    // Vẽ vùng đánh để debug
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 1.5f);
    }
}
