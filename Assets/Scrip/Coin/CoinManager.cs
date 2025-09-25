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
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        LoadCoin(); // Load coin khi game bắt đầu
        UpdateCoinText();
    }

    public void AddCoin(int amount)
    {
        coinCount += amount;
        UpdateCoinText();
        SaveCoin(); // Lưu coin sau khi thay đổi
    }

    public void UpdateCoinText()
    {
        if (coinText != null)
            coinText.text = "Coin: " + coinCount.ToString();
    }

    public void SaveCoin()
    {
        PlayerPrefs.SetInt("Coin", coinCount);
        PlayerPrefs.Save();
    }

    public void LoadCoin()
    {
        coinCount = PlayerPrefs.GetInt("Coin", 0); // Nếu chưa có thì mặc định 0
    }

    private void OnApplicationQuit()
    {
        SaveCoin(); // Lưu khi thoát game
    }
}
