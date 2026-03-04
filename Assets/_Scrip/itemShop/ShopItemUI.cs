using UnityEngine;
using UnityEngine.UI;

public class ShopItemUI : MonoBehaviour
{
    public Image icon;
    private ItemData itemData;
    private InventoryItem inventoryItem; // Thêm cho forge mode

    public void Setup(ItemData data)
    {
        itemData = data;
        inventoryItem = null; // Reset
        icon.sprite = data.itemIcon;
    }

    public void Setup(InventoryItem invItem)
    {
        itemData = invItem.itemData;
        inventoryItem = invItem;
        icon.sprite = invItem.itemData.itemIcon;
    }

    public void OnClick()
    {
        // Nếu là inventory item (forge mode) thì gọi ShowForgeItemDetail
        if (inventoryItem != null)
        {
            ShopManager.Instance.ShowForgeItemDetail(inventoryItem);
        }
        else
        {
            ShopManager.Instance.ShowItemDetail(itemData);
        }
    }
}
