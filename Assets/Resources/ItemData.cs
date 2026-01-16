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
    public string itemID;
    public string itemName;
    public string itemDescription;

    [Header("Base Stats")]
    public int baseHP;
    public int baseAttack;
    public int basePhongThu;
    public int baseNeTranh;
    public int baseTocDo;

    public Sprite itemIcon;
    public int price;
    public ItemType itemType;
}