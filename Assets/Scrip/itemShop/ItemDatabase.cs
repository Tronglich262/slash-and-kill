using UnityEngine;

public class ItemDatabase : MonoBehaviour
{
    public static ItemDatabase Instance;

    public ItemData[] allItems;

    private void Awake()
    {
        Instance = this;
    }

    public ItemData GetItemByID(string id)
    {
        foreach (var item in allItems)
        {
            if (item.itemID == id) return item;
        }
        Debug.LogWarning("Item ID không tồn tại: " + id);
        return null;
    }
}
