using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EquipmentItemPanel : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject panel;
    public Image itemIcon;
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI itemDescriptionText;
    public TextMeshProUGUI Price;
    public TextMeshProUGUI itemLevelDo;

    [Header("Buttons")]
    public Button unequipButton;

    [Header("Empty Slot Message")]
    public GameObject emptySlotMessagePanel;
    public TextMeshProUGUI emptySlotText; 

    private EquipmentSlot currentSlot;
    public static EquipmentItemPanel instance;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            enabled = false;
            return;
        }

        instance = this;
        panel.SetActive(false);
        if (emptySlotMessagePanel != null)
            emptySlotMessagePanel.SetActive(false);
        unequipButton.onClick.AddListener(OnClickUnequip);
    }

    private void OnDestroy()
    {
        unequipButton.onClick.RemoveListener(OnClickUnequip);
    }

    public void ShowItem(EquipmentSlot slot)
    {
        if (slot == null || slot.currentItem == null || slot.currentItem.itemData == null)
        {
            return;
        }

        if (emptySlotMessagePanel != null)
            emptySlotMessagePanel.SetActive(false);

        currentSlot = slot;
        panel.SetActive(true);

        InventoryItem invItem = slot.currentItem;

        if (invItem.itemData == null)
            invItem.itemData = ItemDatabase.Instance.GetItemByID(invItem.itemID);

        ItemData data = invItem.itemData;

        itemIcon.sprite = data.itemIcon;
        itemNameText.text = data.itemName;

        itemDescriptionText.text =
            $"{data.itemDescription}\n" +
            $"HP: {invItem.GetHP()}\n" +
            $"Tấn Công: {invItem.GetAttack()}\n" +
            $"Phòng Thủ: {invItem.GetPhongThu()}\n" +
            $"Né Tránh: {invItem.GetNeTranh()}\n" +
            $"Tốc Độ: {invItem.GetTocDo()}";

        itemLevelDo.text = "Cấp: +" + invItem.levelDo;
        Price.text = "Price: " + data.price;
    }


    public void HidePanel()
    {
        panel.SetActive(false);
        if (emptySlotMessagePanel != null)
            emptySlotMessagePanel.SetActive(false);
        currentSlot = null;
    }

    private void ShowEmptySlotMessage(EquipmentSlot slot)
    {
        if (emptySlotMessagePanel == null) return;

        currentSlot = slot;
        panel.SetActive(false);
        emptySlotMessagePanel.SetActive(true);
        if (emptySlotText != null)
        {
            string slotName = slot != null ? GetSlotTypeName(slot.slotType) : "Ô trống";
            emptySlotText.text = slotName + "\n(Trống)";
        }
    }

    private string GetSlotTypeName(ItemType type)
    {
        switch (type)
        {
            case ItemType.Vukhi: return "Vũ Khí";
            case ItemType.mu: return "Mũ";
            case ItemType.ao: return "Áo";
            case ItemType.quan: return "Quần";
            case ItemType.gang: return "Găng";
            case ItemType.giay: return "Giày";
            case ItemType.vong: return "Vòng";
            case ItemType.nhan: return "Nhẫn";
            default: return type.ToString();
        }
    }

    private void OnClickUnequip()
    {
        if (currentSlot == null || currentSlot.currentItem == null) return;

        // Keep all equipment changes in EquipmentManager so item stats are removed too.
        if (EquipmentManager.Instance == null || InventoryManager.Instance == null)
            return;

        EquipmentManager.Instance.Unequip(currentSlot.slotType);
        SaveSystem.SaveInventory(InventoryManager.Instance.playerInventory);
        InventoryManager.Instance.RefreshInventory();
        HidePanel();
    }

}
