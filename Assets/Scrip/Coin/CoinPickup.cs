using UnityEngine;

public class CoinPickup : MonoBehaviour
{
    public int coinValue = 10; 

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            CoinManager.Instance.AddCoin(coinValue);
            Destroy(gameObject);
        }
    }
}