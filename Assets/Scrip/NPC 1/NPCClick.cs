using UnityEngine;


/// <summary>
/// Nhận ClICK chuột trái vào NPC để hiển thị UI
/// </summary>
public class NPCClick : MonoBehaviour
{
    void Update()
    {
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
                else
                {
                    // Click vào object khác, ẩn UI
                    NPCManager.Instance.HideNPC();
                }
            }
           /* else
            {
                NPCManager.Instance.HideNPC();
            }*/
        }
    }
}
