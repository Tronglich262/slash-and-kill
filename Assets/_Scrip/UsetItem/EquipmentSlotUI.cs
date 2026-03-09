using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EquipmentSlot : MonoBehaviour, IPointerClickHandler
{
    public ItemType slotType;          
    public Image icon;                 
    public InventoryItem currentItem;    
    public EquipmentSlot slot; 

    // --- EQUIP ---
    public void Equip(InventoryItem newItem)
    {
        if (newItem.itemData.itemType == slotType)
        {
            currentItem = newItem;
            icon.sprite = newItem.itemData.itemIcon;
            icon.enabled = true;

            Debug.Log("Đã trang bị: " + newItem.itemData.itemName + " vào " + slotType);
        }
        else
        {
            Debug.LogWarning("Không thể trang bị " + newItem.itemData.itemName + " vào ô " + slotType);
        }
    }

    // --- UNEQUIP ---
    public void Unequip()
    {
        currentItem = null;
        icon.sprite = null;
        icon.enabled = false;
    }

    // --- GET ITEM ---
    public ItemData GetEquippedItem()
    {
        return currentItem != null ? currentItem.itemData : null;
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        // Chỉ click trái
        if (eventData.button != PointerEventData.InputButton.Left) return;
        
        // Debug
        Debug.Log($"Click slot {slotType} - currentItem: {currentItem}");

        // Chỉ hiện panel nếu có item trang bị
        if (currentItem != null)
            EquipmentItemPanel.instance.ShowItem(this);
    }
}
