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
    public ItemData[] allItems;   // Toàn bộ item trong database (hoặc kéo vào đây)
    public ItemType[] sellTypes;  // Loại item mà NPC này bán

    // Lấy danh sách item mà NPC này sẽ bán
    public ItemData[] GetSellItems()
    {
        return System.Array.FindAll(allItems, item =>
            System.Array.Exists(sellTypes, t => t == item.itemType));
    }
}
