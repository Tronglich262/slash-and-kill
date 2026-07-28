using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EquipmentManager : MonoBehaviour
{
    public static EquipmentManager Instance;

    [Header("Equipment Slots")]
    public EquipmentSlot[] slots;
    public TextMeshProUGUI textchiso;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            enabled = false;
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        LoadEquipment();
        UpdateChiSoText();
    }

    public bool EquipItem(InventoryItem invItem)
    {
        if (invItem == null || slots == null || ItemDatabase.Instance == null)
            return false;

        if (invItem.itemData == null)
            invItem.itemData = ItemDatabase.Instance.GetItemByID(invItem.itemID);

        if (invItem.itemData == null)
            return false;

        ItemData item = invItem.itemData;

        if (LevelSystem.Instance != null && item.requiredLevel > 0)
        {
            if (LevelSystem.Instance.level < item.requiredLevel)
            {
                if (GameManager.Instance != null)
                    GameManager.Instance.ShowNotEnoughLevel(item.requiredLevel);
                else
                    Debug.LogWarning("Required level: " + item.requiredLevel);
                return false;
            }
        }

        foreach (var slot in slots)
        {
            if (slot != null && slot.slotType == item.itemType)
            {
                if (LevelSystem.Instance == null || InventoryManager.Instance == null ||
                    InventoryManager.Instance.playerInventory == null)
                    return false;

                if (slot.currentItem != null)
                {
                    if (slot.currentItem.itemData == null)
                        slot.currentItem.itemData = ItemDatabase.Instance.GetItemByID(slot.currentItem.itemID);

                    if (slot.currentItem.itemData == null)
                        return false;

                    InventoryManager.Instance.playerInventory.AddItem(
                        slot.currentItem.itemData, 1, slot.currentItem.levelDo);
                    LevelSystem.Instance.RemoveItemStats(slot.currentItem);
                }

                slot.Equip(invItem);
                LevelSystem.Instance.ApplyItemStats(invItem);
                SaveEquipment();
                UpdateChiSoText();
                return true;
            }
        }

        return false;
    }

    public void Unequip(ItemType slotType)
    {
        if (slots == null || InventoryManager.Instance == null ||
            InventoryManager.Instance.playerInventory == null || LevelSystem.Instance == null)
            return;

        foreach (var slot in slots)
        {
            if (slot != null && slot.slotType == slotType && slot.currentItem != null)
            {
                var removedItem = slot.currentItem;

                if (removedItem.itemData == null)
                    removedItem.itemData = ItemDatabase.Instance.GetItemByID(removedItem.itemID);

                if (removedItem.itemData == null)
                    return;

                InventoryManager.Instance.playerInventory.AddItem(
                    removedItem.itemData, 1, removedItem.levelDo);
                LevelSystem.Instance.RemoveItemStats(removedItem);

                slot.Unequip();
                SaveEquipment();
                UpdateChiSoText();

                return;
            }
        }
    }

    public void SaveEquipment()
    {
        if (slots == null)
        {
            return;
        }

        List<EquipmentSaveData> saveList = new List<EquipmentSaveData>();

        foreach (var slot in slots)
        {
            if (slot != null && slot.currentItem != null)
            {
                saveList.Add(new EquipmentSaveData
                {
                    itemID = slot.currentItem.itemID,
                    levelDo = slot.currentItem.levelDo,
                    slotType = slot.slotType
                });
            }
        }

        SaveSystem.SaveEquipment(saveList);
    }

    public void LoadEquipment()
    {
        List<EquipmentSaveData> saved = SaveSystem.LoadEquipment();
        if (saved == null || slots == null || ItemDatabase.Instance == null ||
            LevelSystem.Instance == null)
            return;

        HashSet<ItemType> loadedSlotTypes = new HashSet<ItemType>();
        foreach (var data in saved)
        {
            if (!loadedSlotTypes.Add(data.slotType))
                continue;

            ItemData item = ItemDatabase.Instance.GetItemByID(data.itemID);
            if (item == null) continue;

            InventoryItem invItem = new InventoryItem
            {
                itemID = data.itemID,
                levelDo = data.levelDo,
                quantity = 1,
                itemData = item
            };

            foreach (var slot in slots)
            {
                if (slot != null && slot.slotType == data.slotType)
                {
                    slot.Equip(invItem);
                    LevelSystem.Instance.ApplyItemStats(invItem);
                    break;
                }
            }
        }
    }

    private void UpdateChiSoText()
    {
        if (textchiso == null || LevelSystem.Instance == null) return;

        textchiso.text =
            $"Máu cơ bản         : {LevelSystem.Instance.maxHP}\n" +
            $"Năng lượng cơ bản   : {LevelSystem.Instance.maxMP}\n" +
            $"Tấn công cơ bản    : {LevelSystem.Instance.attack}\n" +
            $"Phòng thủ cơ bản   : {LevelSystem.Instance.Phongthu}\n" +
            $"Né Tránh cơ bản    : {LevelSystem.Instance.netranh}\n" +
            $"Tốc độ cơ bản      : {LevelSystem.Instance.tocdo}\n" +
            $"Hồi mau cơ bản     : {0}\n" +
            $"% Sát thương       : {0}\n" +
            $"% Sát thương kỹ năng    : {0}\n" +
            $"% Chí mạng         : {0}\n";

    }
}
