using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

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
