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
        Instance = this;
        forgePanel.SetActive(false);

        confirmButton.onClick.AddListener(OnConfirmForge);
        cancelButton.onClick.AddListener(() => forgePanel.SetActive(false));
    }

    public void OpenForge(InventoryItem item)
    {
        if (item == null) return;

        if (item.itemData == null)
            item.itemData = ItemDatabase.Instance.GetItemByID(item.itemID);

        if (item.itemData == null) return;

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
        if (currentItem == null) return;

        if (currentItem.levelDo >= 10)
        {
            Debug.Log("Item đã đạt +10!");
            return;
        }

        currentItem.levelDo++;

        Debug.Log($"Item {currentItem.itemData.itemName} nâng cấp lên +{currentItem.levelDo}");
        RefreshUI();
        ShopManager.Instance?.ShowForgeItemDetail(currentItem);
        InventoryManager.Instance.RefreshInventory();
        SaveSystem.SaveInventory(InventoryManager.Instance.playerInventory);
    }

}
