using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine.UI;

public class BossIntroManager : MonoBehaviour
{
    [Header("Cài đặt Boss")]
    public BossController myBoss;
    public Animator myBossAnimator;

    [Header("Tin nhắn Intro")]
    [TextArea]
    public List<string> firstTimeMessages = new List<string>()
    {
        "Người dám xâm nhập lãnh thổ của ta...",
        "Hãy chuẩn bị tinh thần đi!",
        "Ta sẽ cho ngươi biết sự khác biệt về sức mạnh!",
        "Đây là trận chiến cuối cùng của ngươi!"
    };

    [TextArea]
    public List<string> returningMessages = new List<string>()
    {
        "Wow! Người có mặt tại đây cũng đồng nghĩa với việc ngươi đã tiêu diệt được đệ tử của ta",
        "Chúc mừng ngươi...",
        "Nhưng ta ở một đẳng cấp khác sẽ khiến ngươi tuyệt vọng",
        "ta sẽ cho ngươi thấy sự khác biệt về sức mạnh!",
    };

    [Header("Boss Last Words")]
    [TextArea]
    public List<string> defeatMessages = new List<string>()
    {
        "Không thể nào... ta lại thất bại trước ngươi...",
        "Nhưng đừng tưởng mọi chuyện đã kết thúc..."
    };
    [Range(0.01f, 0.5f)] public float defeatDialogueHealthThreshold = 0.2f;

    [Header("Cài đặt Camera")]
    public float cameraMoveSpeed = 2f;
    public float cameraOffsetX = 0f;
    public float cameraOffsetY = 1f;

    [Header("UI Panel")]
    public GameObject sharedPanel;
    public TextMeshProUGUI messageText;
    public float typingSpeed = 0.05f;
    [Tooltip("Optional Inspector button. A matching Next button is created automatically when empty.")]
    public Button nextButton;

    [Header("Cài đặt khác")]
    public bool showNotificationOnJoin = false; // Tick để hiện panel khi player vào map
    public float delayBetweenMessages = 1.5f;
    public float delayBeforeBattle = 2f;
    public float notificationDuration = 3f;
    public bool usePlayerPrefs = true;

    [Header("Map Boss Arrival")]
    [SerializeField, Min(0f)] private float arrivalShakeDuration = 1.35f;
    [SerializeField, Min(0f)] private float arrivalShakeIntensity = 0.17f;
    [SerializeField, Min(0f)] private float delayAfterArrivalShake = 0.18f;
    [SerializeField, Min(0.05f)] private float panelSlideDuration = 0.55f;
    [SerializeField, Min(0f)] private float panelSlideDistance = 360f;

    // Khóa PlayerPrefs riêng cho từng boss
    public string playerPrefsKey = "HasVisitedBossRoom";

    private bool introStarted = false;
    private bool battleStarted = false;
    private bool isFirstTime = true;
    private Transform player;
    private Camera mainCamera;
    private EnemyHealth bossHealth;
    private Coroutine activeDialogueRoutine;
    private bool lineIsTyping;
    private bool skipCurrentTyping;
    private bool advanceRequested;
    private bool defeatDialoguePlaying;
    private Image dialogueInputBlocker;

    void Start()
    {
        mainCamera = Camera.main;

        // Tìm player
        GameObject p = GameObject.FindWithTag("Player");
        if (p != null)
            player = p.transform;

        if (usePlayerPrefs)
        {
            isFirstTime = !PlayerPrefs.HasKey(playerPrefsKey);
        }
        if (sharedPanel != null)
            sharedPanel.SetActive(false);

        if (myBoss != null)
        {
            bossHealth = myBoss.GetComponent<EnemyHealth>();
            if (bossHealth != null)
            {
                BossFinalPhase finalPhase = myBoss.GetComponent<BossFinalPhase>();
                // Multi-bar final bosses reserve their last-words lock for the
                // final health bar. Earlier bars must be allowed to reset.
                if (finalPhase == null || finalPhase.IsOnFinalHealthBar)
                    bossHealth.nearDeathThreshold = defeatDialogueHealthThreshold;
                bossHealth.NearDeath += OnBossNearDeath;
            }
        }

        EnsureNextButton();
        EnsureDialogueInputBlocker();
        
        // Chỉ hiện notification khi join map nếu được tick
        if (showNotificationOnJoin)
            StartCoroutine(ShowNotificationThenWait());
    }

    private void OnDestroy()
    {
        if (bossHealth != null)
            bossHealth.NearDeath -= OnBossNearDeath;
    }

    private void Update()
    {
        if (sharedPanel != null && sharedPanel.activeInHierarchy &&
            (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)))
        {
            NextDialogue();
        }
    }

    IEnumerator ShowNotificationThenWait()
    {
        SetPlayerInput(false);
        ForcePlayerIdle();

        // Let the persistent Cinemachine camera settle after the scene switch,
        // then play several decaying impacts so the dungeon tremor is visible.
        yield return null;
        yield return PlayArrivalTremor();
        yield return new WaitForSecondsRealtime(delayAfterArrivalShake);

        if (sharedPanel != null && messageText != null)
        {
            sharedPanel.SetActive(true);
            messageText.text = isFirstTime ? "CẢNH BÁO: Xâm nhập vào Lãnh thổ BOSS" : "Wel Wel ai đây";
            yield return AnimatePanelBands(true);
            yield return new WaitForSecondsRealtime(notificationDuration);
            yield return AnimatePanelBands(false);
            sharedPanel.SetActive(false);
        }

        SetPlayerInput(true);
    }

    private void ForcePlayerIdle()
    {
        if (player == null)
            return;

        PlayerJump jump = player.GetComponent<PlayerJump>();
        if (jump != null)
        {
            jump.ForceIdleAnimation();
            Rigidbody2D body = player.GetComponent<Rigidbody2D>();
            if (body != null)
                body.linearVelocity = new Vector2(body.linearVelocity.x, 0f);
            return;
        }

        Animator playerAnimator = player.GetComponent<Animator>();
        if (playerAnimator == null)
            return;
        playerAnimator.ResetTrigger("Jump");
        playerAnimator.SetBool("IsJumping", false);
        playerAnimator.SetBool("Fall", false);
        playerAnimator.SetFloat("VerticalSpeed", 0f);
    }
    private IEnumerator PlayArrivalTremor()
    {
        if (arrivalShakeDuration <= 0f || arrivalShakeIntensity <= 0f)
            yield break;

        float elapsed = 0f;
        float nextImpact = 0f;
        const float impactInterval = 0.24f;

        while (elapsed < arrivalShakeDuration)
        {
            if (elapsed >= nextImpact)
            {
                float progress = Mathf.Clamp01(elapsed / arrivalShakeDuration);
                float strength = arrivalShakeIntensity * Mathf.Lerp(1f, 0.4f, progress);
                float remaining = Mathf.Max(0.12f, arrivalShakeDuration - elapsed);
                BossCameraShake2D.Shake(Mathf.Min(0.36f, remaining), strength);
                nextImpact += impactInterval;
            }

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
    }
    private IEnumerator AnimatePanelBands(bool showing)
    {
        if (sharedPanel == null)
            yield break;

        RectTransform panelRect = sharedPanel.transform as RectTransform;
        if (panelRect == null)
            yield break;

        RectTransform topBand = null;
        RectTransform bottomBand = null;
        for (int i = 0; i < panelRect.childCount; i++)
        {
            RectTransform child = panelRect.GetChild(i) as RectTransform;
            if (child == null || child.GetComponent<Image>() == null)
                continue;

            if (child.anchorMin.y >= 0.9f && child.anchorMax.y >= 0.9f)
                topBand = child;
            else if (child.anchorMin.y <= 0.1f && child.anchorMax.y <= 0.1f &&
                     child.anchorMax.x - child.anchorMin.x > 0.8f)
                bottomBand = child;
        }

        if (topBand == null && bottomBand == null)
            yield break;

        Vector2 topTarget = topBand != null ? topBand.anchoredPosition : Vector2.zero;
        Vector2 bottomTarget = bottomBand != null ? bottomBand.anchoredPosition : Vector2.zero;
        Sequence sequence = DOTween.Sequence().SetUpdate(true);

        if (showing)
        {
            if (topBand != null)
            {
                topBand.DOKill();
                topBand.anchoredPosition = topTarget + Vector2.up * panelSlideDistance;
                sequence.Join(topBand.DOAnchorPos(topTarget, panelSlideDuration).SetEase(Ease.OutCubic));
            }
            if (bottomBand != null)
            {
                bottomBand.DOKill();
                bottomBand.anchoredPosition = bottomTarget + Vector2.down * panelSlideDistance;
                sequence.Join(bottomBand.DOAnchorPos(bottomTarget, panelSlideDuration).SetEase(Ease.OutCubic));
            }
        }
        else
        {
            if (topBand != null)
            {
                topBand.DOKill();
                sequence.Join(topBand.DOAnchorPos(topTarget + Vector2.up * panelSlideDistance, panelSlideDuration * 0.75f)
                    .SetEase(Ease.InCubic));
            }
            if (bottomBand != null)
            {
                bottomBand.DOKill();
                sequence.Join(bottomBand.DOAnchorPos(bottomTarget + Vector2.down * panelSlideDistance, panelSlideDuration * 0.75f)
                    .SetEase(Ease.InCubic));
            }
        }

        yield return sequence.WaitForCompletion();

        if (!showing)
        {
            if (topBand != null) topBand.anchoredPosition = topTarget;
            if (bottomBand != null) bottomBand.anchoredPosition = bottomTarget;
        }
    }
    // Gọi phương thức này khi player va chạm với trigger
    public void OnPlayerEnterTrigger()
    {
        if (introStarted || battleStarted) return;

        // Lưu trạng thái đã đến
        if (usePlayerPrefs)
        {
            PlayerPrefs.SetInt(playerPrefsKey, 1);
            PlayerPrefs.Save();
        }

        StartCoroutine(BossIntroSequence());
    }

    IEnumerator BossIntroSequence()
    {
        if (introStarted) yield break;
        introStarted = true;

        // Tắt di chuyển player
        SetPlayerInput(false);

        // Camera di chuyển đến boss
        yield return StartCoroutine(MoveCameraToBoss());

        // Boss nói chuyện
        yield return StartCoroutine(BossChatSequence());

        // Bắt đầu chiến đấu
        yield return StartCoroutine(StartBattle());
    }

    IEnumerator MoveCameraToBoss()
    {
        if (mainCamera == null || myBoss == null) yield break;

        Vector3 targetPos = new Vector3(
            myBoss.transform.position.x + cameraOffsetX,
            myBoss.transform.position.y + cameraOffsetY,
            mainCamera.transform.position.z
        );

        float t = 0f;
        Vector3 startPos = mainCamera.transform.position;

        while (t < 1f)
        {
            t += Time.deltaTime * cameraMoveSpeed * 0.5f;
            mainCamera.transform.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        yield return new WaitForSeconds(0.5f);
    }

    IEnumerator BossChatSequence()
    {
        List<string> messages = isFirstTime ? firstTimeMessages : returningMessages;
        yield return ShowDialogueMessages(messages, true);
    }

    IEnumerator StartBattle()
    {
        // Camera trở về player
        if (mainCamera != null && player != null)
        {
            Vector3 targetPos = new Vector3(
                player.position.x + cameraOffsetX,
                player.position.y + cameraOffsetY,
                mainCamera.transform.position.z
            );

            float t = 0f;
            Vector3 startPos = mainCamera.transform.position;

            while (t < 1f)
            {
                t += Time.deltaTime * cameraMoveSpeed * 0.5f;
                mainCamera.transform.position = Vector3.Lerp(startPos, targetPos, t);
                yield return null;
            }
        }

        yield return new WaitForSeconds(delayBeforeBattle);

        battleStarted = true;

        // Bật di chuyển player
        SetPlayerInput(true);

        // Khởi động boss
        if (myBossAnimator != null && HasAnimatorParameter(myBossAnimator, "IntroComplete"))
            myBossAnimator.SetBool("IntroComplete", true);

        if (myBoss != null)
            myBoss.StartBattle();

        Debug.Log("Battle Started: " + myBoss.name);
    }

    public void NextDialogue()
    {
        if (activeDialogueRoutine == null)
            return;

        if (lineIsTyping)
            skipCurrentTyping = true;
        else
            advanceRequested = true;
    }

    private IEnumerator ShowDialogueMessages(List<string> messages, bool hidePanelWhenFinished)
    {
        if (sharedPanel == null || messageText == null || messages == null)
            yield break;

        activeDialogueRoutine = StartCoroutine(TypeDialogueMessages(messages));
        yield return activeDialogueRoutine;
        activeDialogueRoutine = null;

        if (hidePanelWhenFinished && sharedPanel != null)
            sharedPanel.SetActive(false);
    }

    private IEnumerator TypeDialogueMessages(List<string> messages)
    {
        sharedPanel.SetActive(true);

        foreach (string message in messages)
        {
            string line = message ?? string.Empty;
            messageText.text = string.Empty;
            lineIsTyping = true;
            skipCurrentTyping = false;
            advanceRequested = false;

            for (int i = 0; i < line.Length; i++)
            {
                if (skipCurrentTyping)
                {
                    messageText.text = line;
                    break;
                }

                messageText.text += line[i];
                yield return new WaitForSeconds(typingSpeed);
            }

            lineIsTyping = false;

            float elapsed = 0f;
            while (!advanceRequested && elapsed < delayBetweenMessages)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }
        }

        lineIsTyping = false;
    }

    private void OnBossNearDeath(EnemyHealth health)
    {
        if (defeatDialoguePlaying || health == null)
            return;

        StartCoroutine(BossDefeatDialogueSequence(health));
    }

    private IEnumerator BossDefeatDialogueSequence(EnemyHealth health)
    {
        defeatDialoguePlaying = true;
        SetPlayerInput(false);
        if (myBoss != null)
            myBoss.PauseForDialogue();

        yield return ShowDialogueMessages(defeatMessages, true);

        health.ReleaseNearDeathDamageLock();
        if (myBoss != null)
            myBoss.ResumeAfterDialogue();
        SetPlayerInput(true);
        defeatDialoguePlaying = false;
    }

    private void SetPlayerInput(bool enabled)
    {
        if (player == null)
        {
            GameObject foundPlayer = GameObject.FindWithTag("Player");
            if (foundPlayer != null)
                player = foundPlayer.transform;
        }

        if (player == null)
            return;

        PlayerController controller = player.GetComponent<PlayerController>();
        PlayerAttack attack = player.GetComponent<PlayerAttack>();
        PlayerJump jump = player.GetComponent<PlayerJump>();
        PlayerWallSlide wallSlide = player.GetComponent<PlayerWallSlide>();
        LadderClimb ladder = player.GetComponent<LadderClimb>();
        PlayerEdgeIdle edgeIdle = player.GetComponent<PlayerEdgeIdle>();
        if (controller != null)
            controller.SetCanMove(enabled);
        if (attack != null)
            attack.SetCanAct(enabled);
        if (jump != null)
        {
            jump.SetInputLocked(!enabled);
            jump.enabled = enabled;
        }
        if (wallSlide != null)
            wallSlide.enabled = enabled;
        if (ladder != null)
            ladder.enabled = enabled;
        if (edgeIdle != null)
            edgeIdle.enabled = enabled;
    }

    private void EnsureNextButton()
    {
        if (nextButton != null)
        {
            nextButton.onClick.RemoveListener(NextDialogue);
            nextButton.onClick.AddListener(NextDialogue);
            return;
        }

        if (sharedPanel == null || messageText == null)
            return;

        GameObject buttonObject = new GameObject(
            "Next Dialogue Button",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image),
            typeof(Button));
        buttonObject.transform.SetParent(sharedPanel.transform, false);

        RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(1f, 0f);
        buttonRect.anchorMax = new Vector2(1f, 0f);
        buttonRect.pivot = new Vector2(1f, 0f);
        buttonRect.anchoredPosition = new Vector2(-42f, 32f);
        buttonRect.sizeDelta = new Vector2(210f, 52f);

        Image background = buttonObject.GetComponent<Image>();
        background.color = new Color(0f, 0f, 0f, 0.62f);
        nextButton = buttonObject.GetComponent<Button>();
        nextButton.targetGraphic = background;
        nextButton.onClick.AddListener(NextDialogue);

        GameObject labelObject = new GameObject(
            "Label",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(TextMeshProUGUI));
        labelObject.transform.SetParent(buttonObject.transform, false);

        RectTransform labelRect = labelObject.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;

        TextMeshProUGUI label = labelObject.GetComponent<TextMeshProUGUI>();
        label.font = messageText.font;
        label.fontSharedMaterial = messageText.fontSharedMaterial;
        label.fontSize = Mathf.Max(18f, messageText.fontSize * 0.55f);
        label.fontStyle = FontStyles.Bold;
        label.alignment = TextAlignmentOptions.Center;
        label.raycastTarget = false;
        label.text = "NEXT  [SPACE]";
    }

    private void EnsureDialogueInputBlocker()
    {
        if (sharedPanel == null)
            return;

        RectTransform panelRect = sharedPanel.GetComponent<RectTransform>();
        if (panelRect == null)
            return;

        Transform existing = sharedPanel.transform.Find("Dialogue Input Blocker");
        if (existing != null)
        {
            dialogueInputBlocker = existing.GetComponent<Image>();
            return;
        }

        GameObject blockerObject = new GameObject(
            "Dialogue Input Blocker",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image));
        blockerObject.transform.SetParent(sharedPanel.transform, false);
        blockerObject.transform.SetAsFirstSibling();

        RectTransform blockerRect = blockerObject.GetComponent<RectTransform>();
        blockerRect.anchorMin = Vector2.zero;
        blockerRect.anchorMax = Vector2.one;
        blockerRect.offsetMin = Vector2.zero;
        blockerRect.offsetMax = Vector2.zero;

        dialogueInputBlocker = blockerObject.GetComponent<Image>();
        dialogueInputBlocker.color = Color.clear;
        dialogueInputBlocker.raycastTarget = true;
    }

    private static bool HasAnimatorParameter(Animator targetAnimator, string parameterName)
    {
        int parameterHash = Animator.StringToHash(parameterName);
        foreach (AnimatorControllerParameter parameter in targetAnimator.parameters)
        {
            if (parameter.nameHash == parameterHash)
                return true;
        }

        return false;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Va chạm trigger: " + gameObject.name);
            OnPlayerEnterTrigger();
        }
    }
}
