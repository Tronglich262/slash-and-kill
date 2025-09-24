using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static InventoryItem;

public class ShopManager : MonoBehaviour
{
    /// <summary>
    /// Quản lý ShopUI của NPC , trong đó có item và bảng hiển thị thông tin cũng như Buy 
    /// </summary>
    public static ShopManager Instance;

    [Header("Shop UI")]
    public Transform itemContainer;   // nơi chứa slot icon
    public GameObject itemSlotPrefab; // prefab slot icon

    [Header("Item Detail Panel")]
    public Image detailIcon;
    public TextMeshProUGUI detailName;
    public TextMeshProUGUI detailDescription;
    public TextMeshProUGUI detailPrice;

    private ItemData currentItem;
    public Inventory playerInventory;

    [Header("Dap do")]
    public Button dapdo;
    public TextMeshProUGUI Dapdo;
    public TextMeshProUGUI desdapdo;

    [Header("Chi Số đồ")]
    public TextMeshProUGUI Deschisodo;

    private void Awake()
    {
        Instance = this;
        if (playerInventory == null)
        {
            playerInventory = ScriptableObject.CreateInstance<Inventory>();
            Debug.LogWarning("playerInventory chưa gán, tạo mới runtime");
        }
    }

    // Load danh sách item của NPC vào slot icon
    public void LoadShop(NPC npc)
    {

        // Xóa slot cũ
        foreach (Transform child in itemContainer)
            Destroy(child.gameObject);

        // Khi load shop
        if (npc.sellType == ItemType.thoren)
        {
            foreach (var invItem in playerInventory.items)
            {
                if (invItem.itemData == null) continue;

                var slot = Instantiate(itemSlotPrefab, itemContainer);
                slot.GetComponent<ShopItemUI>().Setup(invItem.itemData);

                // Khi click vào slot → chọn item để forge
                slot.GetComponent<Button>().onClick.AddListener(() =>
                {
                    // lưu tạm item vừa chọn
                    InventoryItem selectedItem = invItem;

                    // cập nhật detail panel
                    ShowItemDetail(invItem.itemData);

                    // gán nút "Đập" luôn forge item vừa chọn
                    dapdo.onClick.RemoveAllListeners();
                    dapdo.onClick.AddListener(() =>
                    {
                        ForgeManager.Instance.OpenForge(selectedItem);
                    });
                });
            }
            Deschisodo.text = "";
            Dapdo.text = "Đập";
            desdapdo.text = "Nâng cấp item từ +0 → +10";
        }

        else
        {
            // Shop bình thường
            foreach (var item in npc.allItems)
            {
                if (item.itemType == npc.sellType)
                {
                    var slot = Instantiate(itemSlotPrefab, itemContainer);
                    slot.GetComponent<ShopItemUI>().Setup(item);
                }
            }
            dapdo.onClick.RemoveAllListeners();
          
            Dapdo.text = "Buy";
            desdapdo.text = "";

        }

        ClearItemDetail();
    }



    // Hiển thị chi tiết khi click slot
    public void ShowItemDetail(ItemData item)
    {
        currentItem = item;
        detailIcon.sprite = item.itemIcon;
        detailName.text = item.itemName;
        detailDescription.text = item.itemDescription;
        detailPrice.text = item.price + " gold";

        Deschisodo.text =
       $"HP: {item.hp}\n" +
       $"Tấn Công: {item.attack}\n" +
       $"Phòng Thủ: {item.phongthu}\n" +
       $"Né Tránh: {item.netranh}\n" +
       $"Tốc Độ: {item.tocdo}";
    }

    private void ClearItemDetail()
    {
        detailIcon.sprite = null;
        detailName.text = "Name";
        detailDescription.text = "Description";
        detailPrice.text = "Price";
        currentItem = null;
    }
    private string BuildItemStats(ItemData item)
    {
        string stats = "";

        if (item.hp != 0) stats += $"HP: {item.hp}\n";
        if (item.attack != 0) stats += $"Attack: {item.attack}\n";
        if (item.phongthu != 0) stats += $"Phòng thủ: {item.phongthu}\n";
        if (item.netranh != 0) stats += $"Né tránh: {item.netranh}\n";
        if (item.tocdo != 0) stats += $"Tốc độ: {item.tocdo}\n";

        return string.IsNullOrEmpty(stats) ? "Không có chỉ số" : stats.TrimEnd('\n');
    }

    // Nút mua
    public void BuyItem()
    {
        // Nếu tất cả ok, trừ coin và add item
        if (CoinManager.Instance.coinCount >= currentItem.price)
        {
            CoinManager.Instance.coinCount -= currentItem.price;
            CoinManager.Instance.UpdateCoinText();

            InventoryManager.Instance.playerInventory.AddItem(currentItem, 1);
            InventoryManager.Instance.RefreshInventory();
            SaveSystem.SaveInventory(InventoryManager.Instance.playerInventory);

            Debug.Log("Mua thành công: " + currentItem.itemName);
        }
        else
        {
            Debug.Log("Không đủ vàng!");
        }
    }

}
