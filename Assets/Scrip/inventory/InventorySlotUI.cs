using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour
{
    public Image icon;
    public TextMeshProUGUI quantityText;
    public Button button;
    private InventoryItem item;
    private ItemDetailPanel detailPanel;

    public void Setup(InventoryItem invItem, ItemDetailPanel panel)
    {
        if (invItem.itemData == null)
        {
            Debug.LogError("itemData null! ItemID: " + invItem.itemID);
            return;
        }

        item = invItem;
        detailPanel = panel;

        icon.sprite = item.itemData.itemIcon;
        quantityText.text = item.quantity.ToString();

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnClickSlot);
    }

    private void OnClickSlot()
    {
        if (detailPanel != null && item != null)
        {
            detailPanel.ShowItemDetail(item);
        }
    }
  


}
