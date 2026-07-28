using UnityEngine;
using UnityEngine.UI;

public class ShopItemUI : MonoBehaviour
{
    public Image icon;
    private ItemData itemData;
    private InventoryItem inventoryItem; 

    public void Setup(ItemData data)
    {
        if (data == null)
            return;

        itemData = data;
        inventoryItem = null; 
        icon.sprite = data.itemIcon;
    }

    public void Setup(InventoryItem invItem)
    {
        if (invItem == null || invItem.itemData == null)
            return;

        itemData = invItem.itemData;
        inventoryItem = invItem;
        icon.sprite = invItem.itemData.itemIcon;
    }

    public void OnClick()
    {
        if (ShopManager.Instance == null)
            return;

        if (inventoryItem != null)
        {
            ShopManager.Instance.SelectForgeItem(inventoryItem);
        }
        else
        {
            ShopManager.Instance.ShowItemDetail(itemData);
        }
    }
}
