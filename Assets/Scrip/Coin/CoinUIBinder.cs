using UnityEngine;
using UnityEngine.UI;

public class CoinUIBinder : MonoBehaviour
{
    void Start()
    {
        if (CoinManager.Instance != null)
        {
            CoinManager.Instance.coinText = GetComponent<Text>();
            CoinManager.Instance.UpdateCoinText();
        }
    }
}