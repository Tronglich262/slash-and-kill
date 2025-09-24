using UnityEngine;

[System.Serializable]
public class InventoryItem
{
    public string itemID;
    public int levelDo;
    public int quantity;
    public int hp;
    public int attack;
    public int phongthu;
    public int netranh;
    public int tocdo;

    [System.NonSerialized]
    public ItemData itemData;
}
