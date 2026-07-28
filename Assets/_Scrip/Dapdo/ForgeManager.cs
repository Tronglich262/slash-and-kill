using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ForgeManager : MonoBehaviour
{
    public static ForgeManager Instance;

    [Header("UI")]
    public GameObject forgePanel;
    public Image itemIcon;
    public TextMeshProUGUI itemName;
    public TextMeshProUGUI itemLevelText;
    public TextMeshProUGUI statText;

    public Button confirmButton;
    public Button cancelButton;

    public InventoryItem currentItem;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            enabled = false;
            return;
        }

        Instance = this;
        forgePanel.SetActive(false);

        confirmButton.onClick.AddListener(OnConfirmForge);
        cancelButton.onClick.AddListener(CloseForge);
    }

    private void OnDestroy()
    {
        confirmButton.onClick.RemoveListener(OnConfirmForge);
        cancelButton.onClick.RemoveListener(CloseForge);
        if (Instance == this)
            Instance = null;
    }

    private void CloseForge()
    {
        forgePanel.SetActive(false);
    }

    public void OpenForge(InventoryItem item)
    {
        if (item == null) return;

        if (item.itemData == null)
            item.itemData = ItemDatabase.Instance.GetItemByID(item.itemID);

        if (item.itemData == null ||
            item.itemData.itemType == ItemType.vatpham ||
            item.itemData.itemType == ItemType.thoren)
            return;

        currentItem = item;

        if (!forgePanel.activeSelf)
            forgePanel.SetActive(true);

        RefreshUI();
    }

    private void RefreshUI()
    {
        if (currentItem == null || currentItem.itemData == null) return;

        itemIcon.sprite = currentItem.itemData.itemIcon;
        itemName.text = currentItem.itemData.itemName;
        itemLevelText.text = "Cấp: +" + currentItem.levelDo;

        if (statText != null)
        {
            statText.text =
                $"HP: {currentItem.GetHP()}\n" +
                $"ATK: {currentItem.GetAttack()}\n" +
                $"DEF: {currentItem.GetPhongThu()}\n" +
                $"Né: {currentItem.GetNeTranh()}\n" +
                $"Tốc: {currentItem.GetTocDo()}";
        }
    }

    private void OnConfirmForge()
    {
        if (currentItem == null || currentItem.itemData == null)
            return;

        Inventory inventory = ShopManager.Instance != null
            ? ShopManager.Instance.playerInventory
            : InventoryManager.Instance?.playerInventory;
        if (inventory == null || inventory.items == null ||
            !inventory.items.Contains(currentItem))
        {
            currentItem = null;
            CloseForge();
            return;
        }

        if (currentItem.levelDo >= 10)
        {
            Debug.Log("Item đã đạt +10!");
            return;
        }

        // Equipment stats must be recalculated when an equipped item is forged.
        bool isEquipped = false;
        if (EquipmentManager.Instance != null && EquipmentManager.Instance.slots != null)
        {
            foreach (var slot in EquipmentManager.Instance.slots)
            {
                if (slot != null && slot.currentItem == currentItem)
                {
                    LevelSystem.Instance?.RemoveItemStats(currentItem);
                    isEquipped = true;
                    break;
                }
            }
        }

        currentItem.levelDo++;

        if (isEquipped)
        {
            LevelSystem.Instance?.ApplyItemStats(currentItem);
            EquipmentManager.Instance.SaveEquipment();
        }

        Debug.Log($"Item {currentItem.itemData.itemName} nâng cấp lên +{currentItem.levelDo}");
        RefreshUI();
        ShopManager.Instance?.ShowForgeItemDetail(currentItem);
        InventoryManager.Instance?.RefreshInventory();

        SaveSystem.SaveInventory(inventory);
    }

}
