using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

public class BossIntroManager : MonoBehaviour
{
    [Header("Boss References")]
    public Transform boss;
    public Transform player;
    public Animator bossAnimator;

    [Header("Camera Settings")]
    public float cameraMoveSpeed = 2f;
    public float cameraOffsetX = 0f;
    public float cameraOffsetY = 1f;

    [Header("Shared Panel - Dùng chung cho cả notification và chat")]
    public GameObject sharedPanel; 
    public TextMeshProUGUI messageText; 
    public float typingSpeed = 0.05f; 

    [Header("Boss Trigger Zone")]
    public Collider2D bossTriggerZone; 

    [Header("Boss Intro Messages - First Time")]
    [TextArea]
    public List<string> firstTimeMessages = new List<string>()
    {
        "Người dám xâm nhập lãnh thổ của ta...",
        "Hãy chuẩn bị tinh thần đi!",
        "Ta sẽ cho ngươi biết sự khác biệt về sức mạnh!",
        "Đây là trận chiến cuối cùng của ngươi!"
    };

    [Header("Boss Intro Messages - Returning")]
    [TextArea]
    public List<string> returningMessages = new List<string>()
    {
        "Chúc mừng đã tới được đây...",
        "Ngươi dám quay lại sao?",
        "Hãy chiến đấu đi!"
    };

    [Header("Settings")]
    public float delayBetweenMessages = 1.5f;
    public float delayBeforeBattle = 2f;
    public float notificationDuration = 3f;
    public bool usePlayerPrefs = true; 

    private bool introStarted = false;
    private bool battleStarted = false;
    private bool playerInBossZone = false;
    private Vector3 originalCameraPosition;
    private Camera mainCamera;
    private bool isFirstTime = true;

    void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera != null)
        {
            originalCameraPosition = mainCamera.transform.position;
        }

        // Kiểm tra xem đã từng vào boss room chưa
        if (usePlayerPrefs)
        {
            isFirstTime = !PlayerPrefs.HasKey("HasVisitedBossRoom");
        }

        // Ẩn panel ban đầu
        if (sharedPanel != null)
        {
            sharedPanel.SetActive(false);
        }

        // Tìm player nếu chưa có
        if (player == null)
        {
            GameObject foundPlayer = GameObject.FindWithTag("Player");
            if (foundPlayer != null)
                player = foundPlayer.transform;
        }

        // Bắt đầu với thông báo đã vào map boss
        StartCoroutine(ShowNotificationThenWait());
    }



    IEnumerator ShowNotificationThenWait()
    {
        // Disable player movement khi hiện panel cảnh báo
        if (player != null)
        {
            PlayerController pc = player.GetComponent<PlayerController>();
            PlayerAttack pa = player.GetComponent<PlayerAttack>();
            if (pc != null) pc.SetCanMove(false);
            if (pa != null) pa.SetCanAct(false);
        }

        // Hiện thông báo đã vào map boss (không có hiệu ứng gõ chữ)
        // Player vẫn có thể di chuyển để vào boss zone
        if (sharedPanel != null && messageText != null)
        {
            sharedPanel.SetActive(true);

            if (isFirstTime)
            {
                messageText.text = "CẢNH BÁO: Xâm nhập vào Lãnh thổ BOSS";
            }
            else
            {
                messageText.text = "QUAY LẠI CHIẾN ĐẤU";
            }

            // Đợi một khoảng thời gian
            yield return new WaitForSeconds(notificationDuration);

            // Ẩn thông báo
            sharedPanel.SetActive(false);
        }

        // Enable player để di chuyển vào boss zone
        if (player != null)
        {
            PlayerController pc = player.GetComponent<PlayerController>();
            PlayerAttack pa = player.GetComponent<PlayerAttack>();
            if (pc != null) pc.SetCanMove(true);
            if (pa != null) pa.SetCanAct(true);
        }

        // Đợi cho đến khi player vào vùng trigger của boss
        while (!playerInBossZone)
        {
            yield return null;
        }
    }

    public void OnPlayerEnterBossZone()
    {
        if (introStarted || battleStarted) return;

        // Lưu trạng thái đã đến boss room
        if (usePlayerPrefs)
        {
            PlayerPrefs.SetInt("HasVisitedBossRoom", 1);
            PlayerPrefs.Save();
        }

        StartCoroutine(BossIntroSequence());
    }

    IEnumerator BossIntroSequence()
    {
        if (introStarted) yield break;
        introStarted = true;

        // Disable player movement trong suốt intro
        if (player != null)
        {
            PlayerController pc = player.GetComponent<PlayerController>();
            PlayerAttack pa = player.GetComponent<PlayerAttack>();
            if (pc != null) pc.SetCanMove(false);
            if (pa != null) pa.SetCanAct(false);
        }

        // Bước 1: Camera di chuyển về phía boss
        yield return StartCoroutine(MoveCameraToBoss());

        // Bước 2: Boss nói chuyện (dùng chung panel)
        yield return StartCoroutine(BossChatSequence());

        // Bước 3: Ẩn panel, camera trả về, bắt đầu đánh nhau
        yield return StartCoroutine(StartBattle());
    }

    IEnumerator MoveCameraToBoss()
    {
        if (mainCamera == null || boss == null) yield break;

        Vector3 targetPosition = new Vector3(
            boss.position.x + cameraOffsetX,
            boss.position.y + cameraOffsetY,
            mainCamera.transform.position.z
        );

        float t = 0f;
        Vector3 startPosition = mainCamera.transform.position;

        while (t < 1f)
        {
            t += Time.deltaTime * cameraMoveSpeed * 0.5f;
            mainCamera.transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            yield return null;
        }

        // Đợi thêm một chút sau khi camera đến
        yield return new WaitForSeconds(0.5f);
    }

    IEnumerator BossChatSequence()
    {
        if (sharedPanel == null || messageText == null)
        {
            Debug.LogWarning("Shared Panel chưa được gán!");
            yield break;
        }

        // Chọn messages phù hợp với lần đầu hay quay lại
        List<string> messagesToUse = isFirstTime ? firstTimeMessages : returningMessages;

        // Hiện panel chat
        sharedPanel.SetActive(true);

        foreach (string message in messagesToUse)
        {
            messageText.text = "";
            foreach (char letter in message.ToCharArray())
            {
                messageText.text += letter;
                yield return new WaitForSeconds(typingSpeed);
            }
            yield return new WaitForSeconds(delayBetweenMessages);
        }

        // Đợi thêm một chút
        yield return new WaitForSeconds(0.5f);

        // Ẩn panel chat
        sharedPanel.SetActive(false);
    }
    IEnumerator StartBattle()
    {
        // Trả camera về vị trí ban đầu (theo player)
        if (mainCamera != null && player != null)
        {
            Vector3 targetPosition = new Vector3(
                player.position.x + cameraOffsetX,
                player.position.y + cameraOffsetY,
                mainCamera.transform.position.z
            );

            float t = 0f;
            Vector3 startPosition = mainCamera.transform.position;

            while (t < 1f)
            {
                t += Time.deltaTime * cameraMoveSpeed * 0.5f;
                mainCamera.transform.position = Vector3.Lerp(startPosition, targetPosition, t);
                yield return null;
            }
        }
        // Đợi một chút trước khi bắt đầu chiến đấu
        yield return new WaitForSeconds(delayBeforeBattle);
        battleStarted = true;

        // Enable player movement again
        if (player != null)
        {
            PlayerController pc = player.GetComponent<PlayerController>();
            PlayerAttack pa = player.GetComponent<PlayerAttack>();
            if (pc != null) pc.SetCanMove(true);
            if (pa != null) pa.SetCanAct(true);
        }

        if (bossAnimator != null)
        {
            bossAnimator.SetBool("IntroComplete", true);
        }
        BossController bossController = boss.GetComponent<BossController>();
        if (bossController != null)
        {
            bossController.StartBattle();
        }

        Debug.Log("Battle Started!");
    }

    // Getter để kiểm tra battle đã bắt đầu chưa
    public bool IsBattleStarted()
    {
        return battleStarted;
    }

    // Reset lại trạng thái (gọi khi player chết để quay lại từ đầu)
    public void ResetBossIntro()
    {
        if (usePlayerPrefs)
        {
            PlayerPrefs.DeleteKey("HasVisitedBossRoom");
            PlayerPrefs.Save();
        }
        isFirstTime = true;
        introStarted = false;
        battleStarted = false;
        playerInBossZone = false;
    }

    // Thiết lập thủ công là lần đầu hay không
    public void SetFirstTime(bool value)
    {
        isFirstTime = value;
    }

    // Hiện panel với text bất kỳ (có thể gọi từ script khác)
    public void ShowPanel(string text, float duration = 0f)
    {
        if (sharedPanel != null && messageText != null)
        {
            sharedPanel.SetActive(true);
            messageText.text = text;

            if (duration > 0)
            {
                StartCoroutine(HidePanelAfterDelay(duration));
            }
        }
    }

    // Hiện panel với hiệu ứng gõ chữ (có thể gọi từ script khác)
    public void ShowPanelWithTyping(string text, float typingDelay = 0.05f)
    {
        if (sharedPanel != null && messageText != null)
        {
            sharedPanel.SetActive(true);
            StartCoroutine(TypeText(text, typingDelay));
        }
    }

    IEnumerator TypeText(string text, float delay)
    {
        messageText.text = "";
        foreach (char letter in text.ToCharArray())
        {
            messageText.text += letter;
            yield return new WaitForSeconds(delay);
        }
    }

    // Ẩn panel
    public void HidePanel()
    {
        if (sharedPanel != null)
        {
            sharedPanel.SetActive(false);
        }
    }

    IEnumerator HidePanelAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        sharedPanel.SetActive(false);
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if (introStarted || battleStarted) return;

        if (other.CompareTag("Player"))
        {
            playerInBossZone = true;
            OnPlayerEnterBossZone();
        }
    }
}
