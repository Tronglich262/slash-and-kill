using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemDetailPanel : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject panel;
    public Image itemIcon;
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI itemDescriptionText;
    public TextMeshProUGUI itemPriceText;
    public TextMeshProUGUI itemLevelDo;

    [Header("Buttons")]
    public Button useButton;
    public Button dropButton;

    private InventoryItem currentItem; // item đang được chọn

    private void Awake()
    {
        panel.SetActive(false);
    }

    public void ShowItemDetail(InventoryItem invItem)
    {
        currentItem = invItem;
        panel.SetActive(true);

        if (invItem.itemData == null)
            invItem.itemData = ItemDatabase.Instance.GetItemByID(invItem.itemID);

        var data = invItem.itemData;

        itemIcon.sprite = data.itemIcon;
        itemNameText.text = data.itemName;

        // STAT LẤY TỪ InventoryItem (có levelDo)
        if (data.itemType == ItemType.vatpham)
        {
            itemDescriptionText.text = $"Hồi Máu: +{invItem.GetHP()}";
        }
        else
        {
            itemDescriptionText.text =
                $"{data.itemDescription}\n" +
                $"HP: {invItem.GetHP()}\n" +
                $"Tấn Công: {invItem.GetAttack()}\n" +
                $"Phòng Thủ: {invItem.GetPhongThu()}\n" +
                $"Né Tránh: {invItem.GetNeTranh()}\n" +
                $"Tốc Độ: {invItem.GetTocDo()}";
        }

        itemPriceText.text = "Price: " + data.price;
        itemLevelDo.text = "Cấp: +" + invItem.levelDo;

        useButton.onClick.RemoveAllListeners();
        dropButton.onClick.RemoveAllListeners();

        useButton.onClick.AddListener(OnClickUse);
        dropButton.onClick.AddListener(OnClickDrop);
    }


    public void HidePanel()
    {
        panel.SetActive(false);
        currentItem = null;
    }

    // ====== NÚT DÙNG ======
    private void OnClickUse()
    {
        if (currentItem == null || currentItem.itemData == null) return;

        var data = currentItem.itemData;

        if (data.itemType == ItemType.vatpham) // consumable
        {
            // Hồi máu cho player
            HealthSystem playerHealth = FindObjectOfType<HealthSystem>();
            if (playerHealth != null)
            {
                int healAmount = currentItem.GetHP(); // Lấy HP từ item (đã tính level)
                playerHealth.Heal(healAmount);
                Debug.Log($"Đã hồi {healAmount} máu!");
            }

            currentItem.quantity--;

            if (currentItem.quantity <= 0)
            {
                InventoryManager.Instance.playerInventory.RemoveItem(currentItem);
            }

            Debug.Log("Đã dùng 1 " + data.itemName + ". Còn: " + currentItem.quantity);
        }
        else // trang bị
        {
            // Tạo bản sao mới với quantity = 1
            InventoryItem invItemToEquip = new InventoryItem
            {
                itemData = currentItem.itemData,
                itemID = currentItem.itemID,
                quantity = 1
            };

            // Trang bị item vào slot
            EquipmentManager.Instance.EquipItem(invItemToEquip);

            // Giảm số lượng ở inventory
            currentItem.quantity--;
            if (currentItem.quantity <= 0)
            {
                InventoryManager.Instance.playerInventory.RemoveItem(currentItem);
            }
        }


        InventoryManager.Instance.RefreshInventory();
        SaveSystem.SaveInventory(InventoryManager.Instance.playerInventory);

        HidePanel();
    }

    // ====== NÚT VỨT ======
    private void OnClickDrop()
    {
        if (currentItem == null) return;
        if (currentItem.quantity > 1)
        {
            currentItem.quantity--;
        }
        else
        {
            // Xóa khỏi inventory
            InventoryManager.Instance.playerInventory.RemoveItem(currentItem);
        }
        InventoryManager.Instance.RefreshInventory();
        SaveSystem.SaveInventory(InventoryManager.Instance.playerInventory);

        HidePanel();
    }
}
