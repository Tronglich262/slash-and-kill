using UnityEngine;

/// <summary>
/// Nhận ClICK chuột trái vào NPC để hiển thị UI
/// </summary>
public class NPCClick : MonoBehaviour
{
    void Update()
    {
        if (ActiveUI.instance != null && ActiveUI.instance.SkilCharacterUI != null && ActiveUI.instance.SkilCharacterUI.activeSelf)
        {
            return;
        }

        if (NPCManager.Instance != null && NPCManager.Instance.npcInfoPanel != null && NPCManager.Instance.npcInfoPanel.activeSelf)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
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
