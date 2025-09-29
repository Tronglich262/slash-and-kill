using UnityEngine;
using UnityEngine.UI;
using TMPro;
/// <summary>
/// Quản lý ShopUI của NPC , trong đó có item và bảng hiển thị thông tin cũng như dap do
/// </summary>
public class ForgeManager : MonoBehaviour
{
    public static ForgeManager Instance;

    [Header("UI")]
    public GameObject forgePanel;       // Panel hiện lên
    public Image itemIcon;
    public TextMeshProUGUI itemName;
    public TextMeshProUGUI itemLevelText;
    public Button confirmButton;
    public Button cancelButton;

    public InventoryItem currentItem;

    private void Awake()
    {
        Instance = this;
        forgePanel.SetActive(false);

        confirmButton.onClick.AddListener(OnConfirmForge);
        cancelButton.onClick.AddListener(() => forgePanel.SetActive(false));
    }

    // Mở bảng forge
    public void OpenForge(InventoryItem item)
    {
        if (item == null || item.itemData == null) return;

        currentItem = item;

        if (!forgePanel.activeSelf)
            forgePanel.SetActive(true);

        // luôn cập nhật lại UI
        itemIcon.sprite = item.itemData.itemIcon;
        itemName.text = item.itemData.itemName;
        itemLevelText.text = "Cấp: +" + GetItemLevel(currentItem);
    }



    public int GetItemLevel(InventoryItem item)
    {
        // Lấy level từ itemID hoặc metadata
        // Ví dụ: nếu itemID là "Sword+2" thì lấy +2
        // Để đơn giản, ta lưu level trong quantity
        return item.levelDo + 0; // giả sử +0 = quantity 1
    }

    private void OnConfirmForge()
    {
        if (currentItem == null) return;

        int level = GetItemLevel(currentItem);

        if (level >= 10)
        {
            Debug.Log("Item đã đạt +10!");
            return;
        }

        // Nâng cấp
        currentItem.levelDo += 1; // level +1
        itemLevelText.text = "Cấp: +" + GetItemLevel(currentItem);

        Debug.Log($"Item {currentItem.itemData.itemName} nâng cấp lên +{GetItemLevel(currentItem)}");

        // Cập nhật Inventory UI
        InventoryManager.Instance.RefreshInventory();
        SaveSystem.SaveInventory(InventoryManager.Instance.playerInventory);
    }
}
