using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static InventoryItem;
public enum ShopMode
{
    Buy,
    Forge
}


public class ShopManager : MonoBehaviour
{
    /// <summary>
    /// Quản lý ShopUI của NPC , trong đó có item và bảng hiển thị thông tin cũng như Buy 
    /// </summary>
    public static ShopManager Instance;
    public ShopMode currentMode;

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

    [Header("Text Level Yêu Cầu")]
    public TextMeshProUGUI textLevelReq;

    [Header("Text Thông báo mua đô ")]
    public TextMeshProUGUI textthongbaobuy;

    private void Awake()
    {
        Instance = this;
        if (playerInventory == null)
        {
            playerInventory = ScriptableObject.CreateInstance<Inventory>();
            Debug.LogWarning("playerInventory chưa gán, tạo mới runtime");
        }
        textthongbaobuy.text = "";
    }

    // Load danh sách item của NPC vào slot icon
    public void LoadShop(NPC npc)
    {
        ClearItemDetail();
        // Xóa slot cũ
        foreach (Transform child in itemContainer)
            Destroy(child.gameObject);

        // Khi load shop
        if (npc.sellTypes != null && npc.sellTypes.Length == 1 && npc.sellTypes[0] == ItemType.thoren)
        {
            currentMode = ShopMode.Forge;

            InventoryItem firstItem = null;

            foreach (var invItem in playerInventory.items)
            {
                if (invItem.itemData == null) continue;

                if (firstItem == null)
                    firstItem = invItem;

                var slot = Instantiate(itemSlotPrefab, itemContainer);
                var shopItemUI = slot.GetComponent<ShopItemUI>();
                shopItemUI.Setup(invItem);

                InventoryItem cachedItem = invItem;
                slot.GetComponent<Button>().onClick.AddListener(() =>
                {
                    shopItemUI.OnClick();
                    dapdo.onClick.RemoveAllListeners();
                    dapdo.onClick.AddListener(() =>
                    {
                        ForgeManager.Instance.OpenForge(cachedItem);
                    });
                });
            }

            Dapdo.text = "Đập";
            desdapdo.text = "Nâng cấp item từ +0 → +10";
            textthongbaobuy.text = "";

            // AUTO TARGET ITEM ĐẦU TIÊN
            if (firstItem != null)
            {
                ShowForgeItemDetail(firstItem);

                dapdo.onClick.RemoveAllListeners();
                dapdo.onClick.AddListener(() =>
                {
                    ForgeManager.Instance.OpenForge(firstItem);
                });
            }
        }


        else
        {
            currentMode = ShopMode.Buy;

            ItemData firstItem = null;

            foreach (var item in npc.allItems)
            {
                if (!System.Array.Exists(npc.sellTypes, t => t == item.itemType))
                    continue;

                if (firstItem == null)
                    firstItem = item;

                var slot = Instantiate(itemSlotPrefab, itemContainer);
                slot.GetComponent<ShopItemUI>().Setup(item);

                ItemData cachedItem = item; //  tránh closure bug
                slot.GetComponent<Button>().onClick.AddListener(() =>
                {
                    ShowItemDetail(cachedItem);

                    dapdo.onClick.RemoveAllListeners();
                    dapdo.onClick.AddListener(BuyItem);
                });
            }

            Dapdo.text = "Buy";
            desdapdo.text = "";

            // AUTO TARGET ITEM ĐẦU TIÊN
            if (firstItem != null)
            {
                ShowItemDetail(firstItem);

                dapdo.onClick.RemoveAllListeners();
                dapdo.onClick.AddListener(BuyItem);
            }
        }
    }

    //thợ rèn
    // DÙNG RIÊNG CHO THỢ RÈN
    public void ShowForgeItemDetail(InventoryItem invItem)
    {
        if (invItem == null) return;

        if (invItem.itemData == null)
            invItem.itemData = ItemDatabase.Instance.GetItemByID(invItem.itemID);

        ItemData data = invItem.itemData;
        if (data == null) return;

        detailIcon.sprite = data.itemIcon;
        detailName.text = data.itemName;
        detailDescription.text = data.itemDescription;
        detailPrice.text = ""; // thợ rèn không bán

        // Item hồi máu/mana
        if (data.itemType == ItemType.vatpham)
        {
            string stats = "";
            if (invItem.GetHP() > 0) stats += $"Hồi Máu: +{invItem.GetHP()}\n";
            if (invItem.GetMP() > 0) stats += $"Hồi Mana: +{invItem.GetMP()}";
            Deschisodo.text = string.IsNullOrEmpty(stats) ? "Không có tác dụng" : stats.TrimEnd('\n');
        }
        else
        {
            // STAT THEO LEVEL ĐẬP
            Deschisodo.text =
                $"HP: {invItem.GetHP()}\n" +
                $"Tấn Công: {invItem.GetAttack()}\n" +
                $"Phòng Thủ: {invItem.GetPhongThu()}\n" +
                $"Né Tránh: {invItem.GetNeTranh()}\n" +
                $"Tốc Độ: {invItem.GetTocDo()}";
        }
    }


    // Hiển thị chi tiết khi click slot
    public void ShowItemDetail(ItemData item)
    {
        if (item == null) return;
        
        currentItem = item;
        detailIcon.sprite = item.itemIcon;
        detailName.text = item.itemName;
        detailDescription.text = item.itemDescription;
        detailPrice.text = item.price + " gold";

        // Hiển thị level yêu cầu riêng
        if (textLevelReq != null)
        {
            textLevelReq.text = item.requiredLevel > 0 ? $"<color=yellow>Cần Level: {item.requiredLevel}</color>" : "";
        }

        // Item hồi máu/mana
        if (item.itemType == ItemType.vatpham)
        {
            string stats = "";
            if (item.baseHP > 0) stats += $"Hồi Máu: +{item.baseHP}\n";
            if (item.baseMP > 0) stats += $"Hồi Mana: +{item.baseMP}";
            Deschisodo.text = string.IsNullOrEmpty(stats) ? "Không có tác dụng" : stats.TrimEnd('\n');
        }
        else
        {
            Deschisodo.text =
 $"HP: {item.baseHP}\n" +
 $"Tấn Công: {item.baseAttack}\n" +
 $"Phòng Thủ: {item.basePhongThu}\n" +
 $"Né Tránh: {item.baseNeTranh}\n" +
 $"Tốc Độ: {item.baseTocDo}";
        }

    }

    private void ClearItemDetail()
    {
        detailIcon.sprite = null;
        detailName.text = "Name";
        detailDescription.text = "Description";
        detailPrice.text = "Price";
        currentItem = null;
    }
    private string BuildItemStatsBase(ItemData item)
    {
        if (item == null) return "Không có chỉ số";

        string stats = "";

        if (item.baseHP != 0) stats += $"HP: {item.baseHP}\n";
        if (item.baseAttack != 0) stats += $"Tấn công: {item.baseAttack}\n";
        if (item.basePhongThu != 0) stats += $"Phòng thủ: {item.basePhongThu}\n";
        if (item.baseNeTranh != 0) stats += $"Né tránh: {item.baseNeTranh}\n";
        if (item.baseTocDo != 0) stats += $"Tốc độ: {item.baseTocDo}\n";

        return string.IsNullOrEmpty(stats)
            ? "Không có chỉ số"
            : stats.TrimEnd('\n');
    }



    // Nút mua
    public void BuyItem()
    {
        //  CHẶN LUÔN KHI KHÔNG PHẢI MODE BUY
        if (currentMode != ShopMode.Buy)
            return;

        if (currentItem == null) return;

        if (CoinManager.Instance.coinCount >= currentItem.price)
        {
            CoinManager.Instance.AddCoin(-currentItem.price);

            InventoryManager.Instance.playerInventory.AddItem(currentItem, 1);
            InventoryManager.Instance.RefreshInventory();
            SaveSystem.SaveInventory(InventoryManager.Instance.playerInventory);

            StartCoroutine(Thongbaotc());
        }
        else
        {
            StartCoroutine(Thongbaotctb());
        }
    }

    IEnumerator Thongbaotc()
    {
        
        textthongbaobuy.text = "Đã mua thành công";
        yield return new WaitForSeconds(2f);
        textthongbaobuy.text = "";
    }
    IEnumerator Thongbaotctb()
    {

        textthongbaobuy.text = "Không đủ vàng, mua thất bại!";
        yield return new WaitForSeconds(2f);
        textthongbaobuy.text = "";
    }

}
