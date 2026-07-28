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
    private InventoryItem currentForgeItem;
    public Inventory playerInventory;
    private sealed class ShopSlotCache
    {
        public readonly GameObject GameObject;
        public readonly ShopItemUI ItemUI;

        public ShopSlotCache(GameObject gameObject, ShopItemUI itemUI)
        {
            GameObject = gameObject;
            ItemUI = itemUI;
        }
    }

    private readonly System.Collections.Generic.List<ShopSlotCache> itemSlotPool =
        new System.Collections.Generic.List<ShopSlotCache>();
    private bool isSlotPoolInitialized;
    private int usedSlotCount;
    private static readonly WaitForSeconds PurchaseMessageDuration = new WaitForSeconds(2f);
    private Coroutine purchaseMessageRoutine;

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
        if (Instance != null && Instance != this)
        {
            enabled = false;
            return;
        }

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
        BeginSlotRefresh();
        dapdo.onClick.RemoveListener(ForgeSelectedItem);

        if (!SaveSystem.EnsureInventoryLoaded(playerInventory))
        {
            Debug.LogWarning("Shop đang chờ inventory và ItemDatabase khởi tạo.");
            HideUnusedSlots();
            return;
        }

        // Khi load shop
        if (npc.sellTypes != null && npc.sellTypes.Length == 1 && npc.sellTypes[0] == ItemType.thoren)
        {
            currentMode = ShopMode.Forge;

            InventoryItem firstItem = null;

            foreach (var invItem in playerInventory.items)
            {
                if (invItem == null || !CanForge(invItem.itemData))
                    continue;

                if (firstItem == null)
                    firstItem = invItem;

                ShopSlotCache slot = GetNextSlot();
                slot.ItemUI.Setup(invItem);
            }

            Dapdo.text = "Đập";
            desdapdo.text = "Nâng cấp item từ +0 → +10";
            textthongbaobuy.text = "";
            dapdo.onClick.AddListener(ForgeSelectedItem);
            dapdo.interactable = firstItem != null;

            // AUTO TARGET ITEM ĐẦU TIÊN
            if (firstItem != null)
                SelectForgeItem(firstItem);
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

                ShopSlotCache slot = GetNextSlot();
                slot.ItemUI.Setup(item);

            }

            Dapdo.text = "Buy";
            desdapdo.text = "";
            dapdo.interactable = firstItem != null;

            // AUTO TARGET ITEM ĐẦU TIÊN
            if (firstItem != null)
                ShowItemDetail(firstItem);
        }

        HideUnusedSlots();
    }

    private void BeginSlotRefresh()
    {
        if (!isSlotPoolInitialized)
        {
            foreach (Transform child in itemContainer)
            {
                if (TryCreateSlotCache(child.gameObject, out ShopSlotCache slot))
                    itemSlotPool.Add(slot);
                else
                    child.gameObject.SetActive(false);
            }

            isSlotPoolInitialized = true;
        }

        usedSlotCount = 0;
    }

    private ShopSlotCache GetNextSlot()
    {
        ShopSlotCache slot;
        if (usedSlotCount < itemSlotPool.Count)
        {
            slot = itemSlotPool[usedSlotCount];
            slot.GameObject.SetActive(true);
        }
        else
        {
            GameObject slotObject = Instantiate(itemSlotPrefab, itemContainer);
            if (!TryCreateSlotCache(slotObject, out slot))
            {
                Destroy(slotObject);
                throw new MissingComponentException(
                    $"Shop slot prefab '{itemSlotPrefab.name}' requires ShopItemUI.");
            }

            itemSlotPool.Add(slot);
        }

        usedSlotCount++;
        return slot;
    }

    private static bool TryCreateSlotCache(GameObject slotObject, out ShopSlotCache slot)
    {
        if (slotObject.TryGetComponent(out ShopItemUI itemUI))
        {
            slot = new ShopSlotCache(slotObject, itemUI);
            return true;
        }

        slot = null;
        return false;
    }

    private void HideUnusedSlots()
    {
        for (int i = usedSlotCount; i < itemSlotPool.Count; i++)
        {
            ShopSlotCache slot = itemSlotPool[i];
            slot.GameObject.SetActive(false);
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

    public void SelectForgeItem(InventoryItem invItem)
    {
        currentForgeItem = invItem;
        ShowForgeItemDetail(invItem);
    }

    private void ForgeSelectedItem()
    {
        if (currentMode == ShopMode.Forge &&
            currentForgeItem != null &&
            playerInventory != null &&
            playerInventory.items != null &&
            playerInventory.items.Contains(currentForgeItem) &&
            ForgeManager.Instance != null)
        {
            ForgeManager.Instance.OpenForge(currentForgeItem);
        }
    }

    private static bool CanForge(ItemData item)
    {
        return item != null &&
               item.itemType != ItemType.vatpham &&
               item.itemType != ItemType.thoren;
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
        currentForgeItem = null;
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

            playerInventory.AddItem(currentItem, 1);
            InventoryManager.Instance?.RefreshInventory();
            SaveSystem.SaveInventory(playerInventory);

            ShowPurchaseMessage("Đã mua thành công");
        }
        else
        {
            ShowPurchaseMessage("Không đủ vàng, mua thất bại!");
        }
    }

    private void ShowPurchaseMessage(string message)
    {
        if (purchaseMessageRoutine != null)
            StopCoroutine(purchaseMessageRoutine);
        purchaseMessageRoutine = StartCoroutine(ShowPurchaseMessageRoutine(message));
    }

    private IEnumerator ShowPurchaseMessageRoutine(string message)
    {
        textthongbaobuy.text = message;
        yield return PurchaseMessageDuration;
        textthongbaobuy.text = "";
        purchaseMessageRoutine = null;
    }

}
