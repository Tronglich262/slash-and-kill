using UnityEngine;

public class CoinPickup : MonoBehaviour
{
    public int coinValue = 10; 

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Hiển thị floating text Gold
            if (FloatingTextManager.Instance != null)
            {
                FloatingTextManager.Instance.ShowGold(coinValue);
            }

            CoinManager.Instance.AddCoin(coinValue);
            Destroy(gameObject);
        }
    }
}