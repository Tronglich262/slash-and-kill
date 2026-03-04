using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerInventory", menuName = "Inventory/PlayerInventory")]
public class Inventory : ScriptableObject
{
    public List<InventoryItem> items = new List<InventoryItem>();

    /// <summary>
    /// KHÔNG GỘP – LUÔN TẠO ITEM MỚI
    /// </summary>
    public void AddItem(ItemData newItem, int amount = 1, int levelDo = 0)
    {
        if (newItem == null)
        {
            Debug.LogWarning("AddItem: newItem null");
            return;
        }

        // LUÔN TẠO ITEM MỚI – KHÔNG CHECK TRÙNG
        InventoryItem temp = new InventoryItem
        {
            itemID = newItem.itemID,
            itemData = newItem,
            levelDo = levelDo,
            quantity = 1 // LUÔN = 1
        };

        items.Add(temp);

        Debug.Log($"[Inventory] Add item: {newItem.itemName} +{levelDo}");
    }

    /// <summary>
    /// Khi load từ PlayerPrefs, link lại itemData
    /// </summary>
    public void LinkItemData()
    {
        foreach (var invItem in items)
        {
            if (invItem.itemData == null)
                invItem.itemData = ItemDatabase.Instance.GetItemByID(invItem.itemID);
        }
    }

    /// <summary>
    /// Remove đúng instance item
    /// </summary>
    public void RemoveItem(InventoryItem item)
    {
        if (item == null) return;
        items.Remove(item);
    }
}
