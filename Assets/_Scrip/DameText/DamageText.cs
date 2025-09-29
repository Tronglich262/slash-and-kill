using UnityEngine;
using TMPro;

public class DamageText : MonoBehaviour
{
    public TextMeshProUGUI damageText;
    private Transform enemyTransform;
    private Vector3 offset = new Vector3(0, 0f, 0); // Hiển thị trên đầu quái
    private float moveSpeed = 1f; // Tốc độ bay lên
    private float fadeSpeed = 2f; // Tốc độ mờ dần
    private CanvasGroup canvasGroup; // Dùng để làm mờ text

    public void Setup(int damage, Transform enemy)
    {
        damageText.text = "-" + damage.ToString();
        damageText.color = Color.red; // Màu đỏ
        damageText.fontSize = 50; // Kích thước chữ
        enemyTransform = enemy; // Theo dõi quái

        canvasGroup = gameObject.AddComponent<CanvasGroup>(); // Thêm CanvasGroup để làm mờ
        Destroy(gameObject, 1f); // Xóa sau 1 giây
    }

    void Update()
    {
        if (enemyTransform != null)
        {
            // Cập nhật vị trí DamageText theo quái
            Vector3 screenPosition = Camera.main.WorldToScreenPoint(enemyTransform.position + offset);
            transform.position = screenPosition;
        }

        // Bay dần lên
        offset.y += moveSpeed * Time.deltaTime;

        // Mờ dần
        if (canvasGroup != null)
        {
            canvasGroup.alpha -= fadeSpeed * Time.deltaTime;
        }
    }
}
