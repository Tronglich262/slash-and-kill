using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Gives a final boss several full health bars and creates its encounter HUD.
/// A reset occurs before EnemyHealth can enter its normal death routine.
/// </summary>
[DisallowMultipleComponent]
public sealed class BossFinalPhase : MonoBehaviour
{
    [Range(2, 8)] public int healthBarCount = 4;
    public string bossDisplayName = "HẮC ÁM — CHÚA TỂ HƯ KHÔNG";
    [Range(0.01f, 0.5f)] public float finalLastWordsThreshold = 0.2f;
    [Min(1f)] public float hudRevealDistance = 18f;

    public bool IsOnFinalHealthBar => currentHealthBar >= healthBarCount;

    private EnemyHealth health;
    private BossController controller;
    private int currentHealthBar = 1;
    private Image healthFill;
    private TextMeshProUGUI titleText;
    private TextMeshProUGUI phaseText;
    private Image[] phaseMarks;
    private GameObject hudRoot;
    private Transform player;

    private void Awake()
    {
        health = GetComponent<EnemyHealth>();
        controller = GetComponent<BossController>();
        if (health != null)
        {
            // Do not let early bars trigger the normal last-words damage lock.
            health.nearDeathThreshold = -1f;
            health.Damaged += OnBossDamaged;
            health.HealthChanged += OnHealthChanged;
        }
    }

    private void Start()
    {
        if (health != null && health.healthBar != null)
            health.healthBar.gameObject.SetActive(false);
        CreateHud();
        GameObject playerObject = GameObject.FindWithTag("Player");
        if (playerObject != null)
            player = playerObject.transform;
        RefreshHud();
    }

    private void Update()
    {
        if (player == null)
        {
            GameObject playerObject = GameObject.FindWithTag("Player");
            if (playerObject != null)
                player = playerObject.transform;
        }

        bool shouldShowHud = controller != null && controller.IsBattleActive && player != null && health != null && health.currentHealth > 0f &&
                             Mathf.Abs(player.position.x - transform.position.x) <= hudRevealDistance;
        if (hudRoot != null && hudRoot.activeSelf != shouldShowHud)
            hudRoot.SetActive(shouldShowHud);
        if (!shouldShowHud)
            return;

        RefreshHud();
    }

    private void OnDestroy()
    {
        if (health != null)
        {
            health.Damaged -= OnBossDamaged;
            health.HealthChanged -= OnHealthChanged;
        }
        if (hudRoot != null)
            Destroy(hudRoot);
    }

    private void OnBossDamaged(EnemyHealth damagedHealth, float damage)
    {
        // Event refresh keeps the central UI in lockstep with the damage frame.
        RefreshHud();
        if (damagedHealth == null || IsOnFinalHealthBar || damagedHealth.currentHealth > 0f)
            return;

        currentHealthBar++;
        damagedHealth.RestoreToFullHealth();
        if (IsOnFinalHealthBar)
            damagedHealth.nearDeathThreshold = finalLastWordsThreshold;
        else
            damagedHealth.nearDeathThreshold = -1f;

        controller?.OnHealthBarReset(currentHealthBar - 1);
        RefreshHud();
    }

    private void OnHealthChanged(EnemyHealth changedHealth)
    {
        RefreshHud();
    }

    private void CreateHud()
    {
        GameObject mainCanvasObject = GameObject.Find("Canvas");
        Canvas canvas = mainCanvasObject != null
            ? mainCanvasObject.GetComponent<Canvas>()
            : FindFirstObjectByType<Canvas>();
        if (canvas == null)
            return;

        GameObject root = FindSceneHud();
        if (root == null)
            root = new GameObject("HacAmBossHud", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        hudRoot = root;
        root.transform.SetParent(canvas.transform, false);
        RectTransform rootRect = root.GetComponent<RectTransform>();
        rootRect.anchorMin = new Vector2(0.5f, 1f);
        rootRect.anchorMax = new Vector2(0.5f, 1f);
        rootRect.pivot = new Vector2(0.5f, 1f);
        rootRect.anchoredPosition = new Vector2(0f, -18f);
        rootRect.sizeDelta = new Vector2(620f, 94f);
        Image rootImage = root.GetComponent<Image>();
        rootImage.color = new Color(0.035f, 0.01f, 0.06f, 0.9f);

        titleText = CreateText(root.transform, "Boss Name", new Vector2(0f, -8f), new Vector2(600f, 28f), 19f);
        titleText.fontStyle = FontStyles.Bold;
        phaseText = CreateText(root.transform, "Boss Phase", new Vector2(0f, -34f), new Vector2(600f, 22f), 13f);

        Image barBack = CreateImage(root.transform, "Health Back", new Vector2(0f, -62f), new Vector2(540f, 19f), new Color(0.12f, 0.04f, 0.16f, 1f));
        healthFill = CreateImage(barBack.transform, "Health Fill", Vector2.zero, Vector2.zero, new Color(0.65f, 0.08f, 0.85f, 1f));
        RectTransform fillRect = healthFill.rectTransform;
        fillRect.anchorMin = new Vector2(0f, 0f);
        fillRect.anchorMax = new Vector2(1f, 1f);
        fillRect.offsetMin = new Vector2(3f, 3f);
        fillRect.offsetMax = new Vector2(-3f, -3f);

        phaseMarks = new Image[healthBarCount];
        for (int i = 0; i < phaseMarks.Length; i++)
        {
            float x = -42f + i * 28f;
            phaseMarks[i] = CreateImage(root.transform, "Health Bar Mark " + (i + 1), new Vector2(x, -34f), new Vector2(15f, 15f), Color.gray);
        }
    }

    private static GameObject FindSceneHud()
    {
        GameObject[] objects = Resources.FindObjectsOfTypeAll<GameObject>();
        for (int i = 0; i < objects.Length; i++)
            if (objects[i] != null && objects[i].scene.IsValid() && objects[i].name == "HacAmBossHud") return objects[i];
        return null;
    }

    private void RefreshHud()
    {
        if (health == null)
            return;
        if (titleText != null)
            titleText.text = bossDisplayName;
        if (phaseText != null)
            phaseText.text = "THỂ MÁU " + currentHealthBar + " / " + healthBarCount;
        if (healthFill != null)
        {
            float healthPercent = health.maxHealth > 0f
                ? Mathf.Clamp01(health.currentHealth / health.maxHealth)
                : 0f;
            RectTransform fillRect = healthFill.rectTransform;
            fillRect.anchorMax = new Vector2(healthPercent, 1f);
            fillRect.offsetMax = new Vector2(healthPercent <= 0f ? 0f : -3f, -3f);
        }
        if (phaseMarks == null)
            return;

        for (int i = 0; i < phaseMarks.Length; i++)
        {
            if (phaseMarks[i] != null)
                phaseMarks[i].color = i < currentHealthBar
                    ? new Color(0.84f, 0.22f, 1f, 1f)
                    : new Color(0.2f, 0.12f, 0.25f, 1f);
        }
    }

    private static Image CreateImage(Transform parent, string objectName, Vector2 position, Vector2 size, Color color)
    {
        GameObject child = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        child.transform.SetParent(parent, false);
        RectTransform rect = child.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        Image image = child.GetComponent<Image>();
        // Unity 6 no longer exposes UI/Skin/UISprite.psd. Image renders its
        // built-in white graphic when no sprite is assigned, which is enough
        // for these coloured HUD panels and avoids a missing-resource error.
        image.color = color;
        return image;
    }

    private static TextMeshProUGUI CreateText(Transform parent, string objectName, Vector2 position, Vector2 size, float fontSize)
    {
        GameObject child = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        child.transform.SetParent(parent, false);
        RectTransform rect = child.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        TextMeshProUGUI text = child.GetComponent<TextMeshProUGUI>();
        text.alignment = TextAlignmentOptions.Center;
        text.fontSize = fontSize;
        text.color = Color.white;
        return text;
    }
}
