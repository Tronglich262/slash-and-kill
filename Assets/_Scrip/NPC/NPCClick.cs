using UnityEngine;

/// <summary>
/// Nhận ClICK chuột trái vào NPC để hiển thị UI
/// </summary>
public class NPCClick : MonoBehaviour
{
    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

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
            if (mainCamera == null)
                mainCamera = Camera.main;
            if (mainCamera == null)
                return;

            Vector2 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            Collider2D hit = Physics2D.OverlapPoint(mousePos);

            if (hit != null && hit.TryGetComponent(out NPC npc))
            {
                if (NPCManager.Instance != null)
                    NPCManager.Instance.ShowNPC(npc);
            }
        }
    }
}
