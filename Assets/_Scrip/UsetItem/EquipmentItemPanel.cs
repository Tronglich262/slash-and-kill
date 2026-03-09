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
    public GameObject emptySlotMessagePanel; // Panel thông báo ô trống
    public TextMeshProUGUI emptySlotText; // Text thông báo

    private EquipmentSlot currentSlot;
    public static EquipmentItemPanel instance;

    private void Awake()
    {
        instance = this;
        panel.SetActive(false);
        if (emptySlotMessagePanel != null)
            emptySlotMessagePanel.SetActive(false);
    }

    public void ShowItem(EquipmentSlot slot)
    {
        // Kiểm tra null đầy đủ - không làm gì nếu không có item
        if (slot == null || slot.currentItem == null || slot.currentItem.itemData == null)
        {
            return;
        }

        // Ẩn thông báo ô trống nếu đang hiển thị
        if (emptySlotMessagePanel != null)
            emptySlotMessagePanel.SetActive(false);

        currentSlot = slot;
        panel.SetActive(true);

        InventoryItem invItem = slot.currentItem;

        // đảm bảo itemData không null
        if (invItem.itemData == null)
            invItem.itemData = ItemDatabase.Instance.GetItemByID(invItem.itemID);

        ItemData data = invItem.itemData;

        itemIcon.sprite = data.itemIcon;
        itemNameText.text = data.itemName;

        //  STAT PHẢI LẤY TỪ InventoryItem (có levelDo)
        itemDescriptionText.text =
            $"{data.itemDescription}\n" +
            $"HP: {invItem.GetHP()}\n" +
            $"Tấn Công: {invItem.GetAttack()}\n" +
            $"Phòng Thủ: {invItem.GetPhongThu()}\n" +
            $"Né Tránh: {invItem.GetNeTranh()}\n" +
            $"Tốc Độ: {invItem.GetTocDo()}";

        //  LEVEL PHẢI LẤY TỪ InventoryItem
        itemLevelDo.text = "Cấp: +" + invItem.levelDo;

        Price.text = "Price: " + data.price;

        // Hiển thị quantity trong inventory nếu có
        var invInBag = InventoryManager.Instance.playerInventory.items
            .Find(x => x.itemID == invItem.itemID);

        int qty = invInBag != null ? invInBag.quantity : 1;

        // Nút Tháo
        unequipButton.onClick.RemoveAllListeners();
        unequipButton.onClick.AddListener(OnClickUnequip);
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
        panel.SetActive(false); // Ẩn panel item detail
        emptySlotMessagePanel.SetActive(true);

        // Hiển thị tên ô trống
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

        InventoryItem invItem = currentSlot.currentItem;

        // tháo khỏi slot
        currentSlot.Unequip();

        //  THÊM THẲNG 1 ITEM MỚI – KHÔNG GỘP
        InventoryManager.Instance.playerInventory.items.Add(new InventoryItem
        {
            itemID = invItem.itemID,
            itemData = invItem.itemData,
            levelDo = invItem.levelDo,
            quantity = 1
        });

        // lưu
        EquipmentManager.Instance.SaveEquipment();
        SaveSystem.SaveInventory(InventoryManager.Instance.playerInventory);

        InventoryManager.Instance.RefreshInventory();
        HidePanel();
    }

}
