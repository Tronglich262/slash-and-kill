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
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        LoadEquipment();
        UpdateChiSoText();
    }

    public void EquipItem(InventoryItem invItem)
    {
        if (invItem == null) return;

        if (invItem.itemData == null)
            invItem.itemData = ItemDatabase.Instance.GetItemByID(invItem.itemID);

        if (invItem.itemData == null) return;

        ItemData item = invItem.itemData;

        if (LevelSystem.Instance != null && item.requiredLevel > 0)
        {
            if (LevelSystem.Instance.level < item.requiredLevel)
            {
                GameManager.Instance.ShowNotEnoughLevel(item.requiredLevel);
                return;
            }
        }

        foreach (var slot in slots)
        {
            if (slot.slotType == item.itemType)
            {
                if (slot.currentItem != null)
                {
                    if (slot.currentItem.itemData == null)
                        slot.currentItem.itemData = ItemDatabase.Instance.GetItemByID(slot.currentItem.itemID);

                    InventoryManager.Instance.playerInventory.AddItem(slot.currentItem.itemData, 1);
                    LevelSystem.Instance.RemoveItemStats(slot.currentItem);
                }

                slot.Equip(invItem);
                LevelSystem.Instance.ApplyItemStats(invItem);
                SaveEquipment();
                UpdateChiSoText();
                return;
            }
        }
    }

    public void Unequip(ItemType slotType)
    {
        foreach (var slot in slots)
        {
            if (slot.slotType == slotType && slot.currentItem != null)
            {
                var removedItem = slot.currentItem;

                if (removedItem.itemData == null)
                    removedItem.itemData = ItemDatabase.Instance.GetItemByID(removedItem.itemID);

                InventoryManager.Instance.playerInventory.AddItem(removedItem.itemData, 1);
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
        if (saved == null) return;

        foreach (var data in saved)
        {
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
                if (slot.slotType == data.slotType)
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
