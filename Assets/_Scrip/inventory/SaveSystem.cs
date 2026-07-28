using System.Collections.Generic;
using UnityEngine;

public static class SaveSystem
{
    private const string InventoryKey = "PlayerInventory";
    private const string EquipmentKey = "PlayerEquipment";
    private static readonly HashSet<int> LoadedInventoryIds = new HashSet<int>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetRuntimeState()
    {
        LoadedInventoryIds.Clear();
    }

    public static bool EnsureInventoryLoaded(Inventory inventory)
    {
        if (inventory == null)
            return false;

        int inventoryId = inventory.GetInstanceID();
        if (LoadedInventoryIds.Contains(inventoryId))
            return true;

        // Item references can only be reconstructed after the database Awake.
        // Leave it unmarked so the next caller can retry.
        if (ItemDatabase.Instance == null)
            return false;

        LoadInventory(inventory);
        inventory.LinkItemData();
        LoadedInventoryIds.Add(inventoryId);
        return true;
    }

    // ---------------- INVENTORY ----------------
    public static void SaveInventory(Inventory inventory)
    {
        if (inventory == null)
            return;

        LoadedInventoryIds.Add(inventory.GetInstanceID());

        if (inventory.items == null)
            inventory.items = new List<InventoryItem>();

        foreach (var item in inventory.items)
        {
            if (item != null && item.itemData != null)
                item.itemID = item.itemData.itemID;
        }

        string json = JsonUtility.ToJson(inventory);
        PlayerPrefs.SetString(InventoryKey, json);
    }

    public static void LoadInventory(Inventory inventory)
    {
        if (inventory == null || !PlayerPrefs.HasKey(InventoryKey))
            return;

        try
        {
            string json = PlayerPrefs.GetString(InventoryKey);
            if (string.IsNullOrWhiteSpace(json))
                throw new System.FormatException("Inventory save is empty.");

            JsonUtility.FromJsonOverwrite(json, inventory);

            if (inventory.items == null)
                inventory.items = new List<InventoryItem>();

            ItemDatabase database = ItemDatabase.Instance;
            if (database == null)
            {
                Debug.LogWarning("Inventory loaded, but ItemDatabase is not ready yet.");
                return;
            }

            // Remove entries that cannot be reconstructed. Keeping them would
            // create invisible slots and repeat warnings on every refresh.
            bool repaired = false;
            for (int i = inventory.items.Count - 1; i >= 0; i--)
            {
                InventoryItem item = inventory.items[i];
                if (item == null || string.IsNullOrEmpty(item.itemID))
                {
                    inventory.items.RemoveAt(i);
                    repaired = true;
                    continue;
                }

                item.itemData = database.GetItemByID(item.itemID);
                if (item.itemData == null)
                {
                    inventory.items.RemoveAt(i);
                    repaired = true;
                    continue;
                }

                if (item.quantity <= 0)
                {
                    inventory.items.RemoveAt(i);
                    repaired = true;
                    continue;
                }

                int validForgeLevel = Mathf.Clamp(item.levelDo, 0, 10);
                if (item.levelDo != validForgeLevel)
                {
                    item.levelDo = validForgeLevel;
                    repaired = true;
                }
            }

            if (repaired)
                SaveInventory(inventory);
        }
        catch (System.Exception exception)
        {
            inventory.items = new List<InventoryItem>();
            PlayerPrefs.DeleteKey(InventoryKey);
            Debug.LogWarning($"Inventory save was invalid and has been reset: {exception.Message}");
        }
    }

    public static void ResetInventory(Inventory inventory)
    {
        if (inventory != null)
        {
            LoadedInventoryIds.Add(inventory.GetInstanceID());
            if (inventory.items == null)
                inventory.items = new List<InventoryItem>();
            else
                inventory.items.Clear();
        }

        PlayerPrefs.DeleteKey(InventoryKey);
        PlayerPrefs.Save();
    }

    // ---------------- EQUIPMENT ----------------
    public static void SaveEquipment(List<EquipmentSaveData> equipmentList)
    {
        string json = JsonUtility.ToJson(new EquipmentSaveWrapper
        {
            equipments = equipmentList ?? new List<EquipmentSaveData>()
        });
        PlayerPrefs.SetString(EquipmentKey, json);
    }

    public static List<EquipmentSaveData> LoadEquipment()
    {
        if (!PlayerPrefs.HasKey(EquipmentKey))
            return null;

        try
        {
            string json = PlayerPrefs.GetString(EquipmentKey);
            if (string.IsNullOrWhiteSpace(json))
                throw new System.FormatException("Equipment save is empty.");

            EquipmentSaveWrapper wrapper = JsonUtility.FromJson<EquipmentSaveWrapper>(json);
            return wrapper != null && wrapper.equipments != null
                ? wrapper.equipments
                : new List<EquipmentSaveData>();
        }
        catch (System.Exception exception)
        {
            PlayerPrefs.DeleteKey(EquipmentKey);
            Debug.LogWarning($"Equipment save was invalid and has been reset: {exception.Message}");
            return new List<EquipmentSaveData>();
        }
    }

    public static void ResetEquipment()
    {
        PlayerPrefs.DeleteKey(EquipmentKey);
        PlayerPrefs.Save();
    }

    [System.Serializable]
    private class EquipmentSaveWrapper
    {
        public List<EquipmentSaveData> equipments;
    }
}
