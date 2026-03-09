using UnityEngine;
using TMPro;

/// <summary>
/// Component cho prefab floating text - tự động bay lên và mờ dần (world space)
/// </summary>
public class FloatingText : MonoBehaviour
{
    [Header("Components")]
    public TextMeshProUGUI textMesh;

    [Header("Cài đặt")]
    public float floatSpeed = 2f;
    public float lifetime = 1.5f;

    private Color textColor;
    private float timer;
    private bool isInitialized = false;

    void Update()
    {
        if (!isInitialized) return;

        // Bay lên trong world space
        transform.position += Vector3.up * floatSpeed * Time.deltaTime;

        // Đếm thời gian
        timer += Time.deltaTime;

        // Mờ dần sau nửa thời gian lifetime
        if (timer > lifetime * 0.5f)
        {
            float alpha = 1f - ((timer - lifetime * 0.5f) / (lifetime * 0.5f));
            alpha = Mathf.Clamp01(alpha);
            SetAlpha(alpha);
        }

        // Hủy khi hết thời gian
        if (timer >= lifetime)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Thiết lập floating text
    /// </summary>
    public void Setup(string text, Color color, float speed, float life)
    {
        if (textMesh == null)
            textMesh = GetComponent<TextMeshProUGUI>();

        if (textMesh != null)
        {
            textMesh.text = text;
            textMesh.color = color;
            textColor = color;
        }

        floatSpeed = speed;
        lifetime = life;
        timer = 0f;
        isInitialized = true;
    }

    void SetAlpha(float alpha)
    {
        if (textMesh != null)
        {
            Color c = textColor;
            c.a = alpha;
            textMesh.color = c;
        }
    }
}
