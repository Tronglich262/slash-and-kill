using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Notification UI")]
    public GameObject notificationPanel;
    public TextMeshProUGUI notificationText;
    public float notificationDuration = 2f;

    private float notificationTimer;

    private void Awake()
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

    private void Start()
    {
        ResetAllGameData();
    }

    private void Update()
    {
        if (notificationPanel != null && notificationPanel.activeSelf)
        {
            notificationTimer -= Time.deltaTime;
            if (notificationTimer <= 0)
            {
                notificationPanel.SetActive(false);
            }
        }
    }

    // Hiển thị thông báo
    public void ShowNotification(string message)
    {
        if (notificationPanel != null && notificationText != null)
        {
            notificationText.text = message;
            notificationPanel.SetActive(true);
            notificationTimer = notificationDuration;
        }
    }

    // Hiển thị thông báo không đủ mana
    public void ShowNotEnoughMana()
    {
        ShowNotification("Không đủ mana!");
    }

    // Hiển thị thông báo không đủ level để mặc đồ
    public void ShowNotEnoughLevel(int requiredLevel)
    {
        ShowNotification("Cần level " + requiredLevel + " để mặc đồ này!");
    }

    // Reset tất cả chỉ số nhân vật
    public void ResetAllStats()
    {
        if (LevelSystem.Instance != null)
        {
            LevelSystem.Instance.ResetAllStats();
        }
    }

    // Reset tất cả dữ liệu game (stats + inventory + equipment)
    public void ResetAllGameData()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        // Reset stats
        ResetAllStats();

        // Reset inventory
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.playerInventory.items.Clear();
            InventoryManager.Instance.RefreshInventory();
            SaveSystem.SaveInventory(InventoryManager.Instance.playerInventory);
        }

        // Reset equipment
        if (EquipmentManager.Instance != null)
        {
            foreach (var slot in EquipmentManager.Instance.slots)
            {
                if (slot.currentItem != null)
                {
                    slot.Unequip();
                }
            }
            EquipmentManager.Instance.SaveEquipment();
        }

        // Reset coin
        if (CoinManager.Instance != null)
        {
            CoinManager.Instance.coinCount = 0;
            CoinManager.Instance.UpdateCoinText();
        }

        Debug.Log("Đã reset tất cả dữ liệu game!");
    }
}
