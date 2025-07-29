using UnityEngine;

public class kiem : MonoBehaviour
{
    public int attackDamage = 10;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter2D(Collider2D other)
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
