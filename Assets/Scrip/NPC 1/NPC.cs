using UnityEngine;


/// <summary>
/// dữ liệu của NPC
/// </summary>
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
    public Transform arrowPoint; // Điểm trên đầu NPC để spawn mũi tên
    public GameObject Shope; // Cửa hàng của NPC

    [Header("Shop Items")]
    public ItemData[] allItems; // list item của NPC này
    public ItemType sellType;   // NPC này chỉ bán item thuộc type này

}
