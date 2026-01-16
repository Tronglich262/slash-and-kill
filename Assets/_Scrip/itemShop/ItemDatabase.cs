using System.Collections.Generic;
using UnityEngine;

public class ItemDatabase : MonoBehaviour
{
    public static ItemDatabase Instance;

    public ItemData[] allItems;
    private Dictionary<string, ItemData> dict;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;

        dict = new Dictionary<string, ItemData>();
        foreach (var item in allItems)
        {
            if (item == null || string.IsNullOrEmpty(item.itemID)) continue;

            if (!dict.ContainsKey(item.itemID))
                dict.Add(item.itemID, item);
            else
                Debug.LogWarning("Trùng itemID: " + item.itemID);
        }
    }

    public ItemData GetItemByID(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        dict.TryGetValue(id, out var item);
        if (item == null) Debug.LogWarning("Item ID không tồn tại: " + id);
        return item;
    }
}
