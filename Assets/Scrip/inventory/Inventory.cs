using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerInventory", menuName = "Inventory/PlayerInventory")]
public class Inventory : ScriptableObject
{
    public List<InventoryItem> items = new List<InventoryItem>();

    // Thêm item
    public void AddItem(ItemData newItem, int amount = 1)
    {
        foreach (var invItem in items)
        {
            if (invItem.itemID == newItem.itemID)
            {
                invItem.quantity += amount;
                return;
            }
        }

        var temp = new InventoryItem
        {
            itemID = newItem.itemID,
            quantity = amount,
            itemData = newItem
        };
        items.Add(temp);
    }

    // Khi load từ PlayerPrefs, link lại itemData
    public void LinkItemData()
    {
        foreach (var invItem in items)
        {
            invItem.itemData = ItemDatabase.Instance.GetItemByID(invItem.itemID);
        }
    }
}
