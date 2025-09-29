using UnityEngine;
using static InventoryItem;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    public Inventory playerInventory;
    public Transform itemContainer;
    public GameObject itemSlotPrefab;
    public ItemDetailPanel detailPanel;

    private void Awake()
    {
        Instance = this;
        if (playerInventory == null)
        {
            playerInventory = ScriptableObject.CreateInstance<Inventory>();
            Debug.LogWarning("playerInventory chưa gán, tạo mới runtime");
        }
    }

    public void Start()
    {
        if (playerInventory == null)
        {
            Debug.LogError("playerInventory chưa gán!");
            return;
        }

        // Load dữ liệu cũ
        SaveSystem.LoadInventory(playerInventory);

        // Liên kết lại itemData từ ItemDatabase
        playerInventory.LinkItemData();

        // Hiển thị UI
        RefreshInventory();
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            SaveSystem.ResetInventory(playerInventory);
        }
    }


    public void RefreshInventory()
    {
        foreach (Transform child in itemContainer)
            Destroy(child.gameObject);

        foreach (var invItem in playerInventory.items)
        {
            if (invItem.itemData == null)
            {
                Debug.LogWarning("ItemData null, skip: " + (invItem != null ? invItem.itemID : "null item"));
                continue;
            }

            var slotGO = Instantiate(itemSlotPrefab, itemContainer);
            var slotUI = slotGO.GetComponent<InventorySlotUI>();
            slotUI.Setup(invItem, detailPanel);
        }
    }
}
