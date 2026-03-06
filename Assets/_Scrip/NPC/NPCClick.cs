using UnityEngine;

/// <summary>
/// Nhận ClICK chuột trái vào NPC để hiển thị UI
/// </summary>
public class NPCClick : MonoBehaviour
{
    void Update()
    {
        // Kiểm tra Character UI có đang mở không (double check)
        if (ActiveUI.instance != null && ActiveUI.instance.SkilCharacterUI != null && ActiveUI.instance.SkilCharacterUI.activeSelf)
        {
            return;
        }

        // Kiểm tra NPC Panel có đang mở không
        if (NPCManager.Instance != null && NPCManager.Instance.npcInfoPanel != null && NPCManager.Instance.npcInfoPanel.activeSelf)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0)) // click chuột trái
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

            if (hit.collider != null)
            {
                NPC npc = hit.collider.GetComponent<NPC>();
                if (npc != null)
                {
                    NPCManager.Instance.ShowNPC(npc);
                }
            }
        }
    }
}
