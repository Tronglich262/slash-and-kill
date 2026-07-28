using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Intro encounter dedicated to Hac Am.</summary>
[DisallowMultipleComponent]
public sealed class HacAmEncounterGate : MonoBehaviour
{
    [Header("Encounter")]
    [SerializeField, Min(2f)] private float triggerDistance = 16f;
    [Header("Combat Zones (scene objects)")]
    [Tooltip("Kéo BoxCheckBossHacAmTrai trong scene vào đây. Không tạo collider bằng code.")]
    [SerializeField] private Collider2D boxCheckBossHacAmTrai;
    [Tooltip("Kéo BoxCheckBossHacAmPhai trong scene vào đây. Không tạo collider bằng code.")]
    [SerializeField] private Collider2D boxCheckBossHacAmPhai;
    [Header("Dialogue Reposition")]
    [SerializeField, Min(1f)] private float dialogueSafeDistance = 3.4f;
    [SerializeField, Min(1f)] private float dialogueTeleportDistance = 4.6f;
    [SerializeField, Min(0f)] private float dialogueTeleportDelay = 0.22f;
    [SerializeField, Min(0.5f)] private float gateOffsetBehindPlayer = 1.15f;
    [SerializeField, Min(2f)] private float gateHeight = 7f;
    [TextArea(2, 4)] [SerializeField] private string[] lines =
    {
        "Một kẻ phàm nhân lại dám bước vào hư không của ta?",
        "Ta là Hắc Ám — Chúa Tể Hư Không. Hãy để bóng tối nuốt chửng ngươi.",
        "Ngươi đã sẵn sàng để bước vào hư không chưa?"
    };

    private BossController boss;
    private EnemyHealth health;
    private Transform player;
    private bool introStarted;
    private bool rainDialogueShown;
    private bool summonDialogueShown;
    private bool dialogueActive;
    private float nextCombatZoneLookupTime;
    private GameObject gate;
    private GameObject panel;
    private TextMeshProUGUI speakerText;
    private TextMeshProUGUI bodyText;
    private TextMeshProUGUI hintText;

    private void Awake()
    {
        boss = GetComponent<BossController>();
        health = GetComponent<EnemyHealth>();
        ResolveCombatZones();
        if (boss != null)
        {
            boss.autoStartBattle = false;
            boss.ShadowRainStarted += OnShadowRainStarted;
            boss.SummonStarted += OnSummonStarted;
        }
    }

    private void Update()
    {
        ResolvePlayer();
        if (!HasCombatZoneReferences() && Time.unscaledTime >= nextCombatZoneLookupTime)
        {
            nextCombatZoneLookupTime = Time.unscaledTime + 0.5f;
            ResolveCombatZones();
        }

        bool insideCombatZone = IsPlayerInsideCombatZone();
        if (!introStarted && player != null &&
            (HasCombatZoneReferences() ? insideCombatZone : Mathf.Abs(player.position.x - transform.position.x) <= triggerDistance))
            StartCoroutine(IntroRoutine());

        if (introStarted && health != null && health.currentHealth <= 0f)
        {
            RemoveGate();
            if (panel != null) panel.SetActive(false);
            SetPlayerLocked(false);
        }
    }

    private void ResolveCombatZones()
    {
        Collider2D[] allColliders = Resources.FindObjectsOfTypeAll<Collider2D>();
        for (int i = 0; i < allColliders.Length; i++)
        {
            Collider2D candidate = allColliders[i];
            if (candidate == null || !candidate.gameObject.scene.IsValid())
                continue;

            if (boxCheckBossHacAmTrai == null &&
                string.Equals(candidate.name, "boxcheckbosshacamtrai", System.StringComparison.OrdinalIgnoreCase))
            {
                boxCheckBossHacAmTrai = candidate;
                continue;
            }

            if (boxCheckBossHacAmPhai == null &&
                string.Equals(candidate.name, "boxcheckbosshacamphai", System.StringComparison.OrdinalIgnoreCase))
            {
                boxCheckBossHacAmPhai = candidate;
            }
        }

        ConfigureCombatZone(boxCheckBossHacAmTrai);
        ConfigureCombatZone(boxCheckBossHacAmPhai);
    }

    private void ConfigureCombatZone(Collider2D zone)
    {
        if (zone == null)
            return;

        // These are scene-authored detection zones, never physical walls.
        zone.isTrigger = true;

        HacAmCombatZoneRelay relay = zone.GetComponent<HacAmCombatZoneRelay>();
        if (relay == null)
            relay = zone.gameObject.AddComponent<HacAmCombatZoneRelay>();
        relay.Initialize(this);
    }

    public void NotifyPlayerEnteredCombatZone(Collider2D other)
    {
        if (introStarted || other == null || !other.CompareTag("Player"))
            return;

        player = other.transform;
        StartCoroutine(IntroRoutine());
    }

    private bool HasCombatZoneReferences()
    {
        return boxCheckBossHacAmTrai != null || boxCheckBossHacAmPhai != null;
    }

    private bool IsPlayerInsideCombatZone()
    {
        if (player == null)
            return false;

        Collider2D playerCollider = player.GetComponent<Collider2D>();
        return IsInsideZone(boxCheckBossHacAmTrai, player, playerCollider) ||
               IsInsideZone(boxCheckBossHacAmPhai, player, playerCollider);
    }

    private static bool IsInsideZone(Collider2D zone, Transform target, Collider2D targetCollider)
    {
        if (zone == null || !zone.enabled || !zone.gameObject.activeInHierarchy)
            return false;

        if (targetCollider != null)
            return zone.bounds.Intersects(targetCollider.bounds);

        return zone.OverlapPoint(target.position);
    }
    private void ResolvePlayer()
    {
        if (player != null) return;
        GameObject found = GameObject.FindGameObjectWithTag("Player");
        if (found != null) player = found.transform;
    }

    private IEnumerator IntroRoutine()
    {
        introStarted = true;
        if (boss != null) boss.SetEncounterDialogueLock(true);
        SetPlayerLocked(true);
        yield return RepositionBossForDialogue();
        CreateArenaGate();
        CreatePanel();
        if (panel != null) panel.SetActive(true);

        for (int i = 0; i < lines.Length; i++)
        {
            speakerText.text = "— HẮC ÁM —";
            yield return TypeLine(lines[i]);
            while (!Input.GetKeyDown(KeyCode.Space) && !Input.GetKeyDown(KeyCode.Return))
                yield return null;
        }

        if (panel != null) panel.SetActive(false);
        SetPlayerLocked(false);
        if (boss != null)
        {
            boss.SetEncounterDialogueLock(false);
            boss.StartBattle();
        }
    }

    private void OnShadowRainStarted()
    {
        if (!rainDialogueShown && introStarted && !dialogueActive)
        {
            rainDialogueShown = true;
            BeginCombatDialogue("Hãy né đi, phàm nhân. Bóng tối sẽ rơi xuống khắp chiến trường!");
        }
    }

    private void OnSummonStarted()
    {
        if (!summonDialogueShown && introStarted && !dialogueActive && boss != null && boss.HealthResetCount >= 2)
        {
            summonDialogueShown = true;
            BeginCombatDialogue("Ngươi sẽ phải trả giá. Hãy xem đội quân hư không của ta nghiền nát ngươi!");
        }
    }

    private void BeginCombatDialogue(string message)
    {
        if (boss != null) boss.SetEncounterDialogueLock(true);
        SetPlayerLocked(true);
        CreatePanel();
        if (panel != null) panel.SetActive(true);
        StartCoroutine(CombatDialogue(message));
    }

    private IEnumerator CombatDialogue(string message)
    {
        dialogueActive = true;
        yield return RepositionBossForDialogue();
        if (speakerText != null) speakerText.text = "— HẮC ÁM —";
        yield return TypeLine(message);
        while (!Input.GetKeyDown(KeyCode.Space) && !Input.GetKeyDown(KeyCode.Return))
            yield return null;
        if (panel != null) panel.SetActive(false);
        SetPlayerLocked(false);
        dialogueActive = false;
        if (boss != null)
        {
            boss.SetEncounterDialogueLock(false);
            boss.StartBattle();
        }
    }

    private IEnumerator RepositionBossForDialogue()
    {
        if (boss == null || player == null)
            yield break;

        Vector3 bossPosition = boss.transform.position;
        float horizontalDistance = Mathf.Abs(bossPosition.x - player.position.x);
        if (horizontalDistance >= dialogueSafeDistance)
            yield break;

        Animator bossAnimator = boss.GetComponent<Animator>();
        SpriteRenderer bossRenderer = boss.GetComponent<SpriteRenderer>();
        if (bossAnimator != null)
            bossAnimator.CrossFade("Spell-NoEffect", 0.06f, 0, 0f);

        yield return new WaitForSecondsRealtime(dialogueTeleportDelay);
        if (bossRenderer != null) bossRenderer.enabled = false;
        yield return new WaitForSecondsRealtime(0.06f);

        float direction = Mathf.Sign(bossPosition.x - player.position.x);
        if (Mathf.Approximately(direction, 0f))
            direction = Random.value < 0.5f ? -1f : 1f;
        bossPosition.x = player.position.x + direction * dialogueTeleportDistance;
        boss.transform.position = bossPosition;

        if (bossRenderer != null) bossRenderer.enabled = true;
        if (bossAnimator != null)
            bossAnimator.CrossFade("Idle", 0.08f, 0, 0f);
        yield return new WaitForSecondsRealtime(0.08f);
    }
    private IEnumerator TypeLine(string value)
    {
        bodyText.text = string.Empty;
        if (hintText != null && hintText != bodyText) hintText.gameObject.SetActive(false);
        foreach (char character in value)
        {
            bodyText.text += character;
            yield return new WaitForSecondsRealtime(0.018f);
        }
        if (hintText != null && hintText != bodyText) hintText.gameObject.SetActive(true);
    }

    private void SetPlayerLocked(bool locked)
    {
        if (player == null) return;
        PlayerController controller = player.GetComponent<PlayerController>();
        if (controller != null) controller.SetCanMove(!locked);
        PlayerAttack attack = player.GetComponent<PlayerAttack>();
        if (attack != null) attack.SetCanAct(!locked);
        PlayerJump jump = player.GetComponent<PlayerJump>();
        if (jump != null) jump.SetInputLocked(locked);
    }

    private void CreateArenaGate()
    {
        if (gate != null || player == null) return;
        float away = Mathf.Sign(player.position.x - transform.position.x);
        if (Mathf.Approximately(away, 0f)) away = 1f;
        gate = new GameObject("HacAm_ArenaGate");
        gate.layer = LayerMask.NameToLayer("Ground");
        gate.transform.position = new Vector3(player.position.x + away * gateOffsetBehindPlayer, player.position.y + gateHeight * 0.5f - 0.1f, 0f);
        BoxCollider2D collider = gate.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(0.45f, gateHeight);
    }

    private void RemoveGate()
    {
        if (gate == null) return;
        Destroy(gate);
        gate = null;
    }

    private static GameObject FindExistingBossChatPanel()
    {
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        for (int i = 0; i < allObjects.Length; i++)
        {
            GameObject candidate = allObjects[i];
            if (candidate != null && candidate.scene.IsValid() && candidate.name == "BossChatPanel_HacAm")
                return candidate;
        }
        return null;
    }

    private void ConfigureHacAmPanel(GameObject hacPanel)
    {
        TextMeshProUGUI[] texts = hacPanel.GetComponentsInChildren<TextMeshProUGUI>(true);
        float largestTextArea = -1f;
        float smallestTextArea = float.MaxValue;
        bodyText = null;
        speakerText = null;
        for (int i = 0; i < texts.Length; i++)
        {
            TextMeshProUGUI text = texts[i];
            text.text = string.Empty;
            float area = Mathf.Abs(text.rectTransform.rect.width * text.rectTransform.rect.height);
            if (area > largestTextArea)
            {
                largestTextArea = area;
                bodyText = text;
            }
            if (area < smallestTextArea)
            {
                smallestTextArea = area;
                speakerText = text;
            }
        }
        if (speakerText == null) speakerText = bodyText;
        hintText = EnsureNextHint(hacPanel, bodyText != null ? bodyText.font : null);

        SpriteRenderer bossSprite = GetComponent<SpriteRenderer>();
        Sprite hacSprite = bossSprite != null ? bossSprite.sprite : null;
        if (hacSprite == null) return;

        Image[] images = hacPanel.GetComponentsInChildren<Image>(true);
        Image portrait = null;
        float largestPortraitArea = -1f;
        for (int i = 0; i < images.Length; i++)
        {
            Image image = images[i];
            if (image.sprite == null) continue;
            float area = Mathf.Abs(image.rectTransform.rect.width * image.rectTransform.rect.height);
            if (area > largestPortraitArea)
            {
                largestPortraitArea = area;
                portrait = image;
            }
        }
        if (portrait != null)
        {
            portrait.sprite = hacSprite;
            portrait.preserveAspect = true;
        }
    }

    private static TextMeshProUGUI EnsureNextHint(GameObject panelObject, TMP_FontAsset font)
    {
        Transform existing = panelObject.transform.Find("HacAmNextHint");
        if (existing != null) return existing.GetComponent<TextMeshProUGUI>();
        GameObject nextObject = new GameObject("HacAmNextHint", typeof(RectTransform), typeof(TextMeshProUGUI));
        nextObject.transform.SetParent(panelObject.transform, false);
        TextMeshProUGUI next = nextObject.GetComponent<TextMeshProUGUI>();
        next.font = font != null ? font : TMP_Settings.defaultFontAsset;
        next.text = "NEXT  [SPACE]";
        next.fontSize = 18f;
        next.alignment = TextAlignmentOptions.BottomRight;
        next.color = Color.white;
        RectTransform rect = next.rectTransform;
        rect.anchorMin = new Vector2(0.76f, 0.035f);
        rect.anchorMax = new Vector2(0.96f, 0.16f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        return next;
    }

    private void CreatePanel()
    {
        if (panel != null) return;

        GameObject sharedPanel = FindExistingBossChatPanel();
        if (sharedPanel != null)
        {
            panel = sharedPanel;
            ConfigureHacAmPanel(panel);
            return;
        }

        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        TextMeshProUGUI existing = FindFirstObjectByType<TextMeshProUGUI>();
        TMP_FontAsset font = existing != null ? existing.font : TMP_Settings.defaultFontAsset;

        panel = new GameObject("HacAm_IntroPanel", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(canvas.transform, false);
        panel.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.88f);
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0f, 0f);
        panelRect.anchorMax = new Vector2(1f, 0f);
        panelRect.pivot = new Vector2(0.5f, 0f);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = new Vector2(0f, 215f);

        speakerText = MakeText("Speaker", panel.transform, font, 28, TextAlignmentOptions.Left);
        SetRect(speakerText.rectTransform, new Vector2(0.27f, 0.58f), new Vector2(0.92f, 0.87f));
        bodyText = MakeText("Dialogue", panel.transform, font, 24, TextAlignmentOptions.Left);
        SetRect(bodyText.rectTransform, new Vector2(0.27f, 0.20f), new Vector2(0.92f, 0.62f));
        hintText = MakeText("NextHint", panel.transform, font, 18, TextAlignmentOptions.Right);
        hintText.text = "NEXT  [SPACE]";
        SetRect(hintText.rectTransform, new Vector2(0.70f, 0.04f), new Vector2(0.94f, 0.20f));
    }

    private static void SetRect(RectTransform rect, Vector2 min, Vector2 max)
    {
        rect.anchorMin = min;
        rect.anchorMax = max;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static TextMeshProUGUI MakeText(string objectName, Transform parent, TMP_FontAsset font, float size, TextAlignmentOptions alignment)
    {
        GameObject textObject = new GameObject(objectName, typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);
        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        text.font = font;
        text.fontSize = size;
        text.alignment = alignment;
        text.color = Color.white;
        text.enableWordWrapping = true;
        return text;
    }

    private void OnDestroy()
    {
        RemoveGate();
        SetPlayerLocked(false);
    }
}
