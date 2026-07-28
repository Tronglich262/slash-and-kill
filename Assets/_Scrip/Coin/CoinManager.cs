using UnityEngine;
using UnityEngine.UI;

public class CoinManager : MonoBehaviour
{
    public static CoinManager Instance;
    [SerializeField] public int coinCount = 0;
    [SerializeField] public Text coinText;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        LoadCoin(); 
        UpdateCoinText();
    }

    public void AddCoin(int amount)
    {
        coinCount = Mathf.Max(0, coinCount + amount);
        UpdateCoinText();
        SaveCoin(); 
    }

    public void UpdateCoinText()
    {
        if (coinText != null)
            coinText.text = "Coin: " + coinCount.ToString();
    }

    public void SaveCoin()
    {
        PlayerPrefs.SetInt("Coin", coinCount);
    }

    public void LoadCoin()
    {
        coinCount = PlayerPrefs.GetInt("Coin", 0); 
    }

}
