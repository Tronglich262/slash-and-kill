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

    private void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();

        if (button != null)
            button.onClick.AddListener(OnClickSlot);
    }

    public void Setup(InventoryItem invItem, ItemDetailPanel panel)
    {
        if (invItem == null || invItem.itemData == null)
        {
            Debug.LogError("Inventory slot cannot display an item with missing data.");
            return;
        }

        item = invItem;
        detailPanel = panel;

        icon.sprite = item.itemData.itemIcon;
        quantityText.text = item.quantity.ToString();

    }

    private void OnClickSlot()
    {
        if (detailPanel != null && item != null)
        {
            detailPanel.ShowItemDetail(item);
        }
    }

    private void OnDestroy()
    {
        if (button != null)
            button.onClick.RemoveListener(OnClickSlot);
    }

}
