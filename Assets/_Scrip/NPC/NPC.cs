using UnityEngine;

[System.Serializable]
public class NPCData
{
    public string npcName;
    public Sprite npcAvatar;
    [TextArea] public string npcDescription;
}

public class NPC : MonoBehaviour
{
    public NPCData npcData;
    public Transform arrowPoint;
    public GameObject shopUI;

    [Header("Shop Items")]
    public ItemData[] allItems;   
    public ItemType[] sellTypes;  

    public ItemData[] GetSellItems()
    {
        return System.Array.FindAll(allItems, item =>
            System.Array.Exists(sellTypes, t => t == item.itemType));
    }
}
