using UnityEngine;
using static InventoryItem;

public static class SaveSystem
{
    public static void SaveInventory(Inventory inventory)
    {
        foreach (var item in inventory.items)
        {
            if (item.itemData != null)
                item.itemID = item.itemData.itemID; // map trước khi save
        }

        string json = JsonUtility.ToJson(inventory);
        PlayerPrefs.SetString("PlayerInventory", json);
        PlayerPrefs.Save();
    }

    public static void LoadInventory(Inventory inventory)
    {
        if (PlayerPrefs.HasKey("PlayerInventory"))
        {
            string json = PlayerPrefs.GetString("PlayerInventory");
            JsonUtility.FromJsonOverwrite(json, inventory);

            // map lại ID → ItemData
            foreach (var item in inventory.items)
            {
                item.itemData = ItemDatabase.Instance.GetItemByID(item.itemID);
            }
        }
    }

    //  Thêm hàm ResetInventory vào đây
    public static void ResetInventory(Inventory inventory)
    {
        inventory.items.Clear();
        PlayerPrefs.DeleteKey("PlayerInventory");
        PlayerPrefs.Save();
    }

}
