using UnityEngine;
using static InventoryItem;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    public Inventory playerInventory;
    public Transform itemContainer;
    public GameObject itemSlotPrefab;
    public ItemDetailPanel detailPanel;
    private readonly System.Collections.Generic.List<InventorySlotUI> slotPool =
        new System.Collections.Generic.List<InventorySlotUI>();
    private bool isSlotPoolInitialized;

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
    }

    public void Start()
    {
        if (playerInventory == null)
        {
            Debug.LogError("playerInventory chưa gán!");
            return;
        }

        if (!SaveSystem.EnsureInventoryLoaded(playerInventory))
        {
            Debug.LogWarning("Inventory đang chờ ItemDatabase khởi tạo.");
            return;
        }

        // Hiển thị UI
        RefreshInventory();
    }
#if UNITY_EDITOR
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            SaveSystem.ResetInventory(playerInventory);
        }
    }
#endif


    public void RefreshInventory()
    {
        if (playerInventory == null || playerInventory.items == null)
            return;

        InitializeSlotPool();
        int usedSlotCount = 0;

        foreach (var invItem in playerInventory.items)
        {
            if (invItem.itemData == null)
            {
                Debug.LogWarning("ItemData null, skip: " + (invItem != null ? invItem.itemID : "null item"));
                continue;
            }

            InventorySlotUI slotUI;
            if (usedSlotCount < slotPool.Count)
            {
                slotUI = slotPool[usedSlotCount];
                slotUI.gameObject.SetActive(true);
            }
            else
            {
                GameObject slotGO = Instantiate(itemSlotPrefab, itemContainer);
                slotUI = slotGO.GetComponent<InventorySlotUI>();
                slotPool.Add(slotUI);
            }

            slotUI.Setup(invItem, detailPanel);
            usedSlotCount++;
        }

        for (int i = usedSlotCount; i < slotPool.Count; i++)
            slotPool[i].gameObject.SetActive(false);
    }

    private void InitializeSlotPool()
    {
        if (isSlotPoolInitialized)
            return;

        foreach (Transform child in itemContainer)
        {
            InventorySlotUI slot = child.GetComponent<InventorySlotUI>();
            if (slot != null)
                slotPool.Add(slot);
            else
                child.gameObject.SetActive(false);
        }

        isSlotPoolInitialized = true;
    }
}
