using NUnit.Framework.Interfaces;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemDetailPanel : MonoBehaviour
{
    public GameObject panel;
    public Image itemIcon;
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI itemDescriptionText;
    public TextMeshProUGUI itemPriceText;
    public TextMeshProUGUI itemleveldo;

    private void Awake()
    {
        panel.SetActive(false);
    }

    public void ShowItemDetail(InventoryItem invItem)
    {
        panel.SetActive(true);
        itemIcon.sprite = invItem.itemData.itemIcon;
        itemNameText.text = invItem.itemData.itemName;
        itemDescriptionText.text = $"{invItem.itemData.itemDescription}\n Hp: {invItem.itemData.hp}\n Tấn Công: {invItem.itemData.attack}\n Phòng Thủ: {invItem.itemData.phongthu}\n Né Tránh: {invItem.itemData.netranh}\n Tốc Độ: {invItem.itemData.tocdo}";
        itemPriceText.text = "Price: " + invItem.itemData.price;
        itemleveldo.text = "Cấp: " + invItem.levelDo;
    }
    public void HidePanel()
    {
        panel.SetActive(false);
    }
}
