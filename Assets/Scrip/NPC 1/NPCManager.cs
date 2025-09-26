using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Quản lý NPC hiển thị UI và cửa hàng NPC click vào
/// </summary>
public class NPCManager : MonoBehaviour
{
    public static NPCManager Instance;

    [Header("UI Elements")]
    public GameObject npcInfoPanel;
    public TextMeshProUGUI npcNameText;
    public TextMeshProUGUI npcDescriptionText;
    public Image npcAvatarImage;

    private NPC currentNPC;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        npcInfoPanel.SetActive(false);
    }


    public void ShowNPC(NPC npc)
    {
        currentNPC = npc;

        npcInfoPanel.SetActive(true);
        npcNameText.text = npc.npcData.npcName;
        npcDescriptionText.text = npc.npcData.npcDescription;
        npcAvatarImage.sprite = npc.npcData.npcAvatar;

        if (npc.shopUI != null)
        {
            npc.shopUI.SetActive(true);
        }
        ShopManager.Instance.LoadShop(npc);

    }

    public void HideNPC()
    {
        npcInfoPanel.SetActive(false);

        if (currentNPC != null && currentNPC.shopUI != null)
        {
            currentNPC.shopUI.SetActive(false);
        }

        currentNPC = null; // reset NPC hiện tại
    }

    public void Close()
    {
        HideNPC();
    }
}
