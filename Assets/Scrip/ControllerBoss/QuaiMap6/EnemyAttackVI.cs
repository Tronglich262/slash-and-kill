using UnityEngine;

public class EnemyAttackVI : MonoBehaviour
{
    public int damage = 10;

    private void DoDamage()
    {
        // Gây sát thương cho Player nếu còn trong vùng đánh
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 1.5f); // tùy theo game bạn chỉnh radius
        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                // Gọi hàm giảm máu của Player
                hit.GetComponent<HealthSystem>().TakeDamage(damage);
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