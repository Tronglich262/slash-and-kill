using UnityEngine;

public class kiem : MonoBehaviour
{
    public int attackDamage = 10;
    void OnTriggerEnter2D(Collider2D other)
    {

        if (other.gameObject.CompareTag("Player"))
        {
            Debug.Log("Đã va chạm với Player");
            HealthSystem playerHealth = other.gameObject.GetComponent<HealthSystem>(); 
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(attackDamage); 
            }
        }
    }
}
