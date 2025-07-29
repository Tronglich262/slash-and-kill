using UnityEngine;
using UnityEngine.UI;

public class CoinManager : MonoBehaviour
{
    public static CoinManager Instance;
    public int coinCount = 0;
    public Text coinText; 

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); 
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddCoin(int amount)
    {
        coinCount += amount;
        UpdateCoinText();
    }

    public void UpdateCoinText()
    {
        if (coinText != null)
            coinText.text = "Coin: " + coinCount.ToString();
    }
    
}