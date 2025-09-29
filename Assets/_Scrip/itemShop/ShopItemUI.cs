using UnityEngine;
using UnityEngine.UI;

public class ShopItemUI : MonoBehaviour
{
    public Image icon;
    private ItemData itemData;

    public void Setup(ItemData data)
    {
        itemData = data;
        icon.sprite = data.itemIcon;
    }

    public void OnClick()
    {
        ShopManager.Instance.ShowItemDetail(itemData);
    }
}
