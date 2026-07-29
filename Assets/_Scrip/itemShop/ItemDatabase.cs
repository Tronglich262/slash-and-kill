using System.Collections.Generic;
using UnityEngine;

public class ItemDatabase : MonoBehaviour
{
    public static ItemDatabase Instance;

    public ItemData[] allItems;
    private Dictionary<string, ItemData> dict;
    private HashSet<ItemData> registeredAssets;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        dict = new Dictionary<string, ItemData>();
        registeredAssets = new HashSet<ItemData>();

        // Keep Inspector ordering for existing content, then automatically add
        // every ItemData asset that lives anywhere under a Resources folder.
        RegisterItems(allItems);
        RegisterItems(Resources.LoadAll<ItemData>(string.Empty));
    }

    private void RegisterItems(IEnumerable<ItemData> items)
    {
        if (items == null)
            return;

        foreach (ItemData item in items)
        {
            // The same asset may exist in allItems and Resources. It is not a duplicate ID.
            if (item == null || !registeredAssets.Add(item))
                continue;

            if (string.IsNullOrWhiteSpace(item.itemID))
            {
                Debug.LogError($"ItemData '{item.name}' chưa có itemID.", item);
                continue;
            }

            if (dict.TryGetValue(item.itemID, out ItemData existing))
            {
                Debug.LogError(
                    $"Trùng itemID {item.itemID}: '{existing.name}' và '{item.name}'.",
                    item);
                continue;
            }

            dict.Add(item.itemID, item);
        }
    }

    public ItemData GetItemByID(string id)
    {
        if (string.IsNullOrEmpty(id))
            return null;

        dict.TryGetValue(id, out ItemData item);
        if (item == null)
            Debug.LogWarning("Item ID không tồn tại: " + id);
        return item;
    }
}