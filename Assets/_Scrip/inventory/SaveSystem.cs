using System.Collections.Generic;
using UnityEngine;

public static class SaveSystem
{
    // ---------------- INVENTORY ----------------
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

    public static void ResetInventory(Inventory inventory)
    {
        inventory.items.Clear();
        PlayerPrefs.DeleteKey("PlayerInventory");
        PlayerPrefs.Save();
    }

    // ---------------- EQUIPMENT ----------------
    public static void SaveEquipment(List<EquipmentSaveData> equipmentList)
    {
        string json = JsonUtility.ToJson(new EquipmentSaveWrapper { equipments = equipmentList });
        PlayerPrefs.SetString("PlayerEquipment", json);
        PlayerPrefs.Save();
    }

    public static List<EquipmentSaveData> LoadEquipment()
    {
        if (PlayerPrefs.HasKey("PlayerEquipment"))
        {
            string json = PlayerPrefs.GetString("PlayerEquipment");
            EquipmentSaveWrapper wrapper = JsonUtility.FromJson<EquipmentSaveWrapper>(json);
            return wrapper.equipments;
        }
        return null;
    }

    public static void ResetEquipment()
    {
        PlayerPrefs.DeleteKey("PlayerEquipment");
        PlayerPrefs.Save();
    }

    // Wrapper để JsonUtility serialize List
    [System.Serializable]
    private class EquipmentSaveWrapper
    {
        public List<EquipmentSaveData> equipments;
    }
}
