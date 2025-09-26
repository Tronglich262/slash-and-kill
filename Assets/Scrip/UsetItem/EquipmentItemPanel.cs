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

    private EquipmentSlot currentSlot;
    public static EquipmentItemPanel instance;

    private void Awake()
    {
        instance = this;
        panel.SetActive(false);
    }

    public void ShowItem(EquipmentSlot slot)
    {
        if (slot == null || slot.currentItem == null) return;

        currentSlot = slot;
        panel.SetActive(true);

        var data = slot.currentItem.itemData;
        itemIcon.sprite = data.itemIcon;
        itemNameText.text = data.itemName;
        itemDescriptionText.text =
            $"{data.itemDescription}\n" +
            $"HP: {data.hp}\n" +
            $"Tấn Công: {data.attack}\n" +
            $"Phòng Thủ: {data.phongthu}\n" +
            $"Né Tránh: {data.netranh}\n" +
            $"Tốc Độ: {data.tocdo}";
        itemLevelDo.text = "Cấp: " + data.leveledo;
        Price.text = "Price: " + data.price;

        // Hiển thị quantity trong inventory nếu có
        var invItem = InventoryManager.Instance.playerInventory.items
            .Find(x => x.itemID == data.itemID);
        int qty = invItem != null ? invItem.quantity : 1;

        // Nút Tháo
        unequipButton.onClick.RemoveAllListeners();
        unequipButton.onClick.AddListener(OnClickUnequip);
    }

    public void HidePanel()
    {
        panel.SetActive(false);
        currentSlot = null;
    }

    private void OnClickUnequip()
    {
        if (currentSlot == null || currentSlot.currentItem == null) return;

        var invItem = currentSlot.currentItem;

        // Tháo khỏi slot
        currentSlot.Unequip();

        // Nếu inventory đã có item → cộng quantity
        InventoryItem existing = InventoryManager.Instance.playerInventory.items
            .Find(x => x.itemID == invItem.itemID);

        if (existing != null)
        {
            existing.quantity += invItem.quantity;
        }
        else
        {
            InventoryManager.Instance.playerInventory.AddItem(invItem.itemData, invItem.quantity);
        }

        // Lưu trạng thái
        EquipmentManager.Instance.SaveEquipment();
        SaveSystem.SaveInventory(InventoryManager.Instance.playerInventory);

        InventoryManager.Instance.RefreshInventory();
        HidePanel();
    }
}
