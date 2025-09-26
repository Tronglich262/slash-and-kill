using UnityEngine;

public enum ItemType
{
    quan,
    ao,
    giay,
    gang,
    mu,
    vong,
    nhan,
    Vukhi,
    vatpham,
    thoren,
}

[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/ItemData")]
public class ItemData : ScriptableObject
{
    public string itemID; // duy nhất
    public string itemName;
    public string itemDescription;
    public int hp;
    public int attack;
    public int phongthu;
    public int netranh;
    public int tocdo;
    public Sprite itemIcon;
    public int price;
    public int leveledo;
    public ItemType itemType; 
}