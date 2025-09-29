using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static Main;

public class EquipmentManager : MonoBehaviour
{
    public static EquipmentManager Instance;

    [Header("Equipment Slots")]
    public EquipmentSlot[] slots; // Kéo thả slot trong Inspector
    public TextMeshProUGUI textchiso;
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    private void Start()
    {
        LoadEquipment();
    }


    // --- EQUIP ---
    public void EquipItem(InventoryItem invItem)
    {
        if (invItem == null || invItem.itemData == null) return;

        ItemData item = invItem.itemData;
        foreach (var slot in slots)
        {
            if (slot.slotType == item.itemType)
            {
                // Nếu slot đã có đồ → trả về inventory và trừ stats
                if (slot.currentItem != null && slot.currentItem.itemData != null)
                {
                    InventoryManager.Instance.playerInventory.AddItem(slot.currentItem.itemData, 1);
                    LevelSystem.Instance.RemoveItemStats(slot.currentItem.itemData);
                }

                slot.Equip(invItem);

                // Cộng stats từ item mới
                LevelSystem.Instance.ApplyItemStats(invItem.itemData);

                // Lưu trạng thái
                SaveEquipment();

                Debug.Log($"[EquipmentManager] Đã trang bị: {item.itemName}");
                return;
            }
        }

        Debug.LogWarning("Không có slot phù hợp cho item " + item.itemName);
    }

    public void Unequip(ItemType slotType)
    {
        foreach (var slot in slots)
        {
            if (slot.slotType == slotType && slot.currentItem != null)
            {
                var removedItem = slot.currentItem;

                // Trả item về inventory
                InventoryManager.Instance.playerInventory.AddItem(removedItem.itemData, 1);

                // Trừ stats của item vừa tháo
                LevelSystem.Instance.RemoveItemStats(removedItem.itemData);

                slot.Unequip();
                SaveEquipment();

                Debug.Log($"[EquipmentManager] Đã tháo trang bị khỏi slot {slotType}");
                return;
            }
        }
    }

    // --- SAVE ---
    public void SaveEquipment()
    {
        if (slots == null)
        {
            Debug.LogError("❌ EquipmentManager: slots chưa gán trong Inspector!");
            return;
        }

        List<EquipmentSaveData> saveList = new List<EquipmentSaveData>();

        foreach (var slot in slots)
        {
            if (slot != null && slot.currentItem != null && slot.currentItem.itemData != null)
            {
                EquipmentSaveData data = new EquipmentSaveData
                {
                    itemID = slot.currentItem.itemData.itemID,
                    slotType = slot.slotType
                };
                saveList.Add(data);
            }
        }

        SaveSystem.SaveEquipment(saveList);
    }


    // --- LOAD ---
    public void LoadEquipment()
    {
        List<EquipmentSaveData> saved = SaveSystem.LoadEquipment();
        if (saved == null) return;

        foreach (var data in saved)
        {
            ItemData item = ItemDatabase.Instance.GetItemByID(data.itemID);
            if (item == null) continue;

            InventoryItem invItem = new InventoryItem { itemData = item, quantity = 1 };

            foreach (var slot in slots)
            {
                if (slot.slotType == data.slotType)
                {
                    slot.Equip(invItem);

                    LevelSystem.Instance?.ApplyItemStats(item);
                    textchiso.text =
                        $"Máu cơ bản        :          {LevelSystem.Instance.maxHP}\n" +
                        $"Tấn công cơ bản   :          {LevelSystem.Instance.attack}\n" +
                        $"Phòng thủ cơ bản  :          {LevelSystem.Instance.Phongthu}\n" +
                        $"Né Tránh cơ bản   :          {LevelSystem.Instance.netranh}\n" +
                        $"Tốc độ cơ bản     :          {LevelSystem.Instance.tocdo}\n" +
                        $"% Máu hồi phục    :          0\n" +
                        $"% Tấn công        :          0\n" +
                        $"% Phòng Thủ       :          0\n" +
                        $"% Né tránh        :          0\n" +
                        $"% Tốc độ          :          0\n";

                    break;
                }
            }
        }
    }



}
