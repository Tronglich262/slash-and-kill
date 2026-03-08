using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class BossIntroManager : MonoBehaviour
{
    [Header("Boss References")]
    public BossController[] bosses;
    public Animator[] bossAnimators;
    public Transform player;

    [Header("Camera Settings")]
    public float cameraMoveSpeed = 2f;
    public float cameraOffsetX = 0f;
    public float cameraOffsetY = 1f;

    [Header("Shared Panel")]
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

        if (usePlayerPrefs)
        {
            isFirstTime = !PlayerPrefs.HasKey("HasVisitedBossRoom");
        }

        if (sharedPanel != null)
        {
            sharedPanel.SetActive(false);
        }

        if (player == null)
        {
            GameObject p = GameObject.FindWithTag("Player");
            if (p != null)
                player = p.transform;
        }

        StartCoroutine(ShowNotificationThenWait());
    }

    IEnumerator ShowNotificationThenWait()
    {
        if (player != null)
        {
            PlayerController pc = player.GetComponent<PlayerController>();
            PlayerAttack pa = player.GetComponent<PlayerAttack>();

            if (pc != null) pc.SetCanMove(false);
            if (pa != null) pa.SetCanAct(false);
        }

        if (sharedPanel != null && messageText != null)
        {
            sharedPanel.SetActive(true);

            if (isFirstTime)
                messageText.text = "CẢNH BÁO: Xâm nhập vào Lãnh thổ BOSS";
            else
                messageText.text = "Wel Wel ai đây";

            yield return new WaitForSeconds(notificationDuration);

            sharedPanel.SetActive(false);
        }

        if (player != null)
        {
            PlayerController pc = player.GetComponent<PlayerController>();
            PlayerAttack pa = player.GetComponent<PlayerAttack>();

            if (pc != null) pc.SetCanMove(true);
            if (pa != null) pa.SetCanAct(true);
        }

        while (!playerInBossZone)
        {
            yield return null;
        }
    }

    public void OnPlayerEnterBossZone()
    {
        if (introStarted || battleStarted) return;

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

        if (player != null)
        {
            PlayerController pc = player.GetComponent<PlayerController>();
            PlayerAttack pa = player.GetComponent<PlayerAttack>();

            if (pc != null) pc.SetCanMove(false);
            if (pa != null) pa.SetCanAct(false);
        }

        yield return StartCoroutine(MoveCameraToBoss());

        yield return StartCoroutine(BossChatSequence());

        yield return StartCoroutine(StartBattle());
    }

    IEnumerator MoveCameraToBoss()
    {
        if (mainCamera == null || bosses.Length == 0) yield break;

        Vector3 bossPos = bosses[0].transform.position;

        Vector3 targetPosition = new Vector3(
            bossPos.x + cameraOffsetX,
            bossPos.y + cameraOffsetY,
            mainCamera.transform.position.z
        );

        float t = 0f;
        Vector3 startPosition = mainCamera.transform.position;

        while (t < 1f)
        {
            t += Time.deltaTime * cameraMoveSpeed * 0.5f;
            mainCamera.transform.position =
                Vector3.Lerp(startPosition, targetPosition, t);

            yield return null;
        }

        yield return new WaitForSeconds(0.5f);
    }

    IEnumerator BossChatSequence()
    {
        if (sharedPanel == null || messageText == null)
            yield break;

        List<string> messages =
            isFirstTime ? firstTimeMessages : returningMessages;

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

                mainCamera.transform.position =
                    Vector3.Lerp(startPosition, targetPosition, t);

                yield return null;
            }
        }

        yield return new WaitForSeconds(delayBeforeBattle);

        battleStarted = true;

        if (player != null)
        {
            PlayerController pc = player.GetComponent<PlayerController>();
            PlayerAttack pa = player.GetComponent<PlayerAttack>();

            if (pc != null) pc.SetCanMove(true);
            if (pa != null) pa.SetCanAct(true);
        }

        foreach (Animator anim in bossAnimators)
        {
            if (anim != null)
            {
                anim.SetBool("IntroComplete", true);
            }
        }

        foreach (BossController boss in bosses)
        {
            if (boss != null)
            {
                boss.StartBattle();
            }
        }

        Debug.Log("Battle Started!");
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