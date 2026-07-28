using UnityEngine;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Component cho prefab floating text - tự động bay lên và mờ dần (world space)
/// </summary>
public class FloatingText : MonoBehaviour
{
    private static readonly Stack<FloatingText> Pool = new Stack<FloatingText>();

    [Header("Components")]
    public TextMeshProUGUI textMesh;

    [Header("Cài đặt")]
    public float floatSpeed = 2f;
    public float lifetime = 1.5f;

    private Color textColor;
    private float timer;
    private bool isInitialized = false;

    public static FloatingText Spawn(GameObject prefab, Vector3 position)
    {
        FloatingText instance = null;

        while (Pool.Count > 0 && instance == null)
            instance = Pool.Pop();

        if (instance == null)
        {
            GameObject obj = Instantiate(prefab, position, Quaternion.identity);
            instance = obj.GetComponent<FloatingText>();
        }
        else
        {
            instance.transform.SetPositionAndRotation(position, Quaternion.identity);
            instance.gameObject.SetActive(true);
        }

        return instance;
    }

    void Update()
    {
        if (!isInitialized) return;

        transform.position += Vector3.up * floatSpeed * Time.deltaTime;
        timer += Time.deltaTime;
        if (timer > lifetime * 0.5f)
        {
            float alpha = 1f - ((timer - lifetime * 0.5f) / (lifetime * 0.5f));
            alpha = Mathf.Clamp01(alpha);
            SetAlpha(alpha);
        }
        if (timer >= lifetime)
        {
            Release();
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

    private void Release()
    {
        isInitialized = false;
        gameObject.SetActive(false);
        Pool.Push(this);
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
