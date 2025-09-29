using UnityEngine;

[System.Serializable] // Cho phép lưu trữ dữ liệu
public class Item
{
    public string itemName;
    public Sprite icon;
    public string itemType; // "Weapon", "Armor", "Consumable", v.v.
    public int itemId; // ID duy nhất cho mỗi item
    // Các thuộc tính khác của item (damage, defense, effect, v.v.)
}