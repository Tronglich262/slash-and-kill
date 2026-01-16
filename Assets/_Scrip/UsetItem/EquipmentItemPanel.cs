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
        currentSlot = null;
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
