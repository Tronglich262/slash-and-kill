using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

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
        "Chúc mừng đã tới được đây...",
        "Ngươi dám quay lại sao?",
        "Hãy chiến đấu đi!"
    };

    [Header("Cài đặt Camera")]
    public float cameraMoveSpeed = 2f;
    public float cameraOffsetX = 0f;
    public float cameraOffsetY = 1f;

    [Header("UI Panel")]
    public GameObject sharedPanel;
    public TextMeshProUGUI messageText;
    public float typingSpeed = 0.05f;

    [Header("Cài đặt khác")]
    public float delayBetweenMessages = 1.5f;
    public float delayBeforeBattle = 2f;
    public float notificationDuration = 3f;
    public bool usePlayerPrefs = true;

    // Khóa PlayerPrefs riêng cho từng boss
    public string playerPrefsKey = "HasVisitedBossRoom";

    private bool introStarted = false;
    private bool battleStarted = false;
    private bool isFirstTime = true;
    private Transform player;
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;

        // Tìm player
        GameObject p = GameObject.FindWithTag("Player");
        if (p != null)
            player = p.transform;

        // Kiểm tra lần đầu
        if (usePlayerPrefs)
        {
            isFirstTime = !PlayerPrefs.HasKey(playerPrefsKey);
        }

        // Ẩn panel ban đầu
        if (sharedPanel != null)
            sharedPanel.SetActive(false);

        // Bắt đầu hiện thông báo
        StartCoroutine(ShowNotificationThenWait());
    }

    IEnumerator ShowNotificationThenWait()
    {
        // Tắt di chuyển player
        if (player != null)
        {
            PlayerController pc = player.GetComponent<PlayerController>();
            PlayerAttack pa = player.GetComponent<PlayerAttack>();
            if (pc != null) pc.SetCanMove(false);
            if (pa != null) pa.SetCanAct(false);
        }

        // Hiện thông báo
        if (sharedPanel != null && messageText != null)
        {
            sharedPanel.SetActive(true);
            messageText.text = isFirstTime ? "CẢNH BÁO: Xâm nhập vào Lãnh thổ BOSS" : "Wel Wel ai đây";
            yield return new WaitForSeconds(notificationDuration);
            sharedPanel.SetActive(false);
        }

        // Bật lại di chuyển
        if (player != null)
        {
            PlayerController pc = player.GetComponent<PlayerController>();
            PlayerAttack pa = player.GetComponent<PlayerAttack>();
            if (pc != null) pc.SetCanMove(true);
            if (pa != null) pa.SetCanAct(true);
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
        if (player != null)
        {
            PlayerController pc = player.GetComponent<PlayerController>();
            PlayerAttack pa = player.GetComponent<PlayerAttack>();
            if (pc != null) pc.SetCanMove(false);
            if (pa != null) pa.SetCanAct(false);
        }

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
        if (sharedPanel == null || messageText == null) yield break;

        List<string> messages = isFirstTime ? firstTimeMessages : returningMessages;

        sharedPanel.SetActive(true);

        foreach (string msg in messages)
        {
            messageText.text = "";
            foreach (char c in msg)
            {
                messageText.text += c;
                yield return new WaitForSeconds(typingSpeed);
            }
            yield return new WaitForSeconds(delayBetweenMessages);
        }

        yield return new WaitForSeconds(0.5f);
        sharedPanel.SetActive(false);
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
        if (player != null)
        {
            PlayerController pc = player.GetComponent<PlayerController>();
            PlayerAttack pa = player.GetComponent<PlayerAttack>();
            if (pc != null) pc.SetCanMove(true);
            if (pa != null) pa.SetCanAct(true);
        }

        // Khởi động boss
        if (myBossAnimator != null)
            myBossAnimator.SetBool("IntroComplete", true);

        if (myBoss != null)
            myBoss.StartBattle();

        Debug.Log("Battle Started: " + myBoss.name);
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
