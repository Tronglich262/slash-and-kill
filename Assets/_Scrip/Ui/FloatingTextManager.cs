using UnityEngine;
using TMPro;

/// <summary>
/// Loại floating text hiển thị
/// </summary>
public enum FloatingTextType
{
    EXP,
    HP,
    Mana,
    Dodge,
    Damage,
    CriticalDamage,
    Gold,
    LevelUp
}

/// <summary>
/// Quản lý floating text - gắn vào một GameObject duy nhất trong scene
/// </summary>
public class FloatingTextManager : MonoBehaviour
{
    public static FloatingTextManager Instance;

    [Header("Prefab")]
    public GameObject floatingTextPrefab;

    [Header("Cài đặt màu")]
    public Color expColor = Color.yellow;
    public Color hpColor = Color.green;
    public Color manaColor = Color.cyan;
    public Color dodgeColor = Color.white;
    public Color damageColor = Color.red;
    public Color criticalDamageColor = new Color(1f, 0.5f, 0f); // Màu cam - chí mạng
    public Color goldColor = new Color(1f, 0.84f, 0f); // Gold
    public Color levelUpColor = new Color(1f, 0.5f, 0f); // Orange

    [Header("Cài đặt hiệu ứng")]
    public float floatSpeed = 2f;
    public float lifetime = 1.5f;

    private int textQueueIndex = 0;
    private const int maxQueueSlots = 5;
    private float[] queueOffsets = { 0f, 0.3f, 0.6f, 0.9f, 1.2f };

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    /// <summary>
    /// Hiển thị floating text tại vị trí của player
    /// </summary>
    public void ShowFloatingText(string text, FloatingTextType type)
    {
        ShowFloatingText(text, type, GetPlayerPosition());
    }

    /// <summary>
    /// Hiển thị floating text tại vị trí chỉ định (world space)
    /// </summary>
    public void ShowFloatingText(string text, FloatingTextType type, Vector3 worldPosition)
    {
        if (floatingTextPrefab == null)
        {
            Debug.LogWarning("FloatingTextPrefab chưa được gán!");
            return;
        }
        float yOffset = queueOffsets[textQueueIndex];
        Vector3 spawnPos = worldPosition + new Vector3(0, yOffset, 0);
        textQueueIndex = (textQueueIndex + 1) % maxQueueSlots;
        GameObject obj = Instantiate(floatingTextPrefab, spawnPos, Quaternion.identity);
        FloatingText ft = obj.GetComponent<FloatingText>();
        if (ft != null)
        {
            ft.Setup(text, GetColor(type), floatSpeed, lifetime);
        }
    }

    /// <summary>
    /// Hiển thị số EXP
    /// </summary>
    public void ShowEXP(int amount)
    {
        ShowFloatingText("+" + amount, FloatingTextType.EXP);
    }

    /// <summary>
    /// Hiển thị HP hồi
    /// </summary>
    public void ShowHP(int amount)
    {
        ShowFloatingText("+" + amount, FloatingTextType.HP);
    }

    /// <summary>
    /// Hiển thị Mana hồi
    /// </summary>
    public void ShowMana(int amount)
    {
        if (amount > 0)
            ShowFloatingText("+" + amount, FloatingTextType.Mana);
        else
            ShowFloatingText(amount.ToString(), FloatingTextType.Mana);
    }

    /// <summary>
    /// Hiển thị né tránh
    /// </summary>
    public void ShowDodge()
    {
        ShowFloatingText("DODGE!", FloatingTextType.Dodge);
    }

    /// <summary>
    /// Hiển thị damage
    /// </summary>
    public void ShowDamage(int amount, Vector3 position)
    {
        ShowFloatingText("-" + amount, FloatingTextType.Damage, position);
    }

    /// <summary>
    /// Hiển thị damage chí mạng
    /// </summary>
    public void ShowCriticalDamage(int amount, Vector3 position)
    {
        ShowFloatingText("-" + amount + "!", FloatingTextType.CriticalDamage, position);
    }

    /// <summary>
    /// Hiển thị gold
    /// </summary>
    public void ShowGold(int amount)
    {
        ShowFloatingText("+" + amount + "G", FloatingTextType.Gold);
    }

    /// <summary>
    /// Hiển thị level up
    /// </summary>
    public void ShowLevelUp()
    {
        ShowFloatingText("LEVEL UP!", FloatingTextType.LevelUp);
    }

    Vector3 GetPlayerPosition()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
            return player.transform.position + Vector3.down * 0.5f; // Dưới player
        return Vector3.zero;
    }

    Color GetColor(FloatingTextType type)
    {
        switch (type)
        {
            case FloatingTextType.EXP: return expColor;
            case FloatingTextType.HP: return hpColor;
            case FloatingTextType.Mana: return manaColor;
            case FloatingTextType.Dodge: return dodgeColor;
            case FloatingTextType.Damage: return damageColor;
            case FloatingTextType.CriticalDamage: return criticalDamageColor;
            case FloatingTextType.Gold: return goldColor;
            case FloatingTextType.LevelUp: return levelUpColor;
            default: return Color.white;
        }
    }
}
