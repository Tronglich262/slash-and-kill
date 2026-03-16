using UnityEngine;
using UnityEngine.UI;

public class ShopItemUI : MonoBehaviour
{
    public Image icon;
    private ItemData itemData;
    private InventoryItem inventoryItem; 

    public void Setup(ItemData data)
    {
        itemData = data;
        inventoryItem = null; 
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
