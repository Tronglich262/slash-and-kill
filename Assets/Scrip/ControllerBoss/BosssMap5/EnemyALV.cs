using UnityEngine;
using System.Collections;

public class BossTeleportController : MonoBehaviour
{
    public Transform pointA;
    public Transform pointB;
    public Transform player;

    public float moveSpeed = 2f;
    public float detectionRange = 8f;
    public float teleportOffsetX = 2f;

    public Transform[] teleportPoints;

    private Vector3 targetPoint;
    private Animator animator;
    private bool isMoving = true;
    private bool isFacingRight = true;
    private bool isTeleporting = false;
    private SpriteRenderer spriteRenderer;

    [Header("Bomb Rain Settings")]
    public GameObject bombPrefab;
    public int bombCount = 10;
    public float bombRadius = 5f;
    public float bombSpawnHeight = 8f;
    
    private bool isCameraShaking = false;
    private Vector3 originalCamPos;



    void Start()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        targetPoint = pointB.position;
        if (Camera.main != null)
        {
            originalCamPos = Camera.main.transform.localPosition;
        }
    }

    void Update()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (!isTeleporting && distanceToPlayer <= detectionRange)
        {
            StartCoroutine(TeleportToPlayer());
        }

        if (isMoving && !isTeleporting)
        {
            MoveBetweenPoints();
        }
    }

    void MoveBetweenPoints()
    {
        animator.SetBool("Walk", true);
        transform.position = Vector2.MoveTowards(transform.position, targetPoint, moveSpeed * Time.deltaTime);

        if (Vector2.Distance(transform.position, targetPoint) < 0.1f)
        {
            targetPoint = targetPoint == pointA.position ? pointB.position : pointA.position;
            Flip();
        }
    }

    void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 localScale = transform.localScale;
        localScale.x *= -1;
        transform.localScale = localScale;
    }

    IEnumerator TeleportToPlayer()
    {
        isTeleporting = true;
        isMoving = false;

        // 1. Biến mất mở cổng
        animator.SetBool("Walk", false);
        animator.SetBool("Spell", true);
        yield return new WaitForSeconds(0.5f);

        spriteRenderer.enabled = false;
        animator.SetBool("Spell", false);
        yield return new WaitForSeconds(0.5f);

        // 2. Dịch tới gần Player
        Vector3 teleportPos = player.position + new Vector3(-teleportOffsetX, 0, 0);
        transform.position = teleportPos;

        if ((teleportPos.x > player.position.x && isFacingRight) ||
            (teleportPos.x < player.position.x && !isFacingRight))
        {
            Flip();
        }

        // 3. Hiện lại và mở cổng
        spriteRenderer.enabled = true;
        animator.SetBool("Spell", true);
        yield return new WaitForSeconds(0.5f);
        animator.SetBool("Spell", false);

        // 4. Chờ 1s trước khi tung chiêu
        yield return new WaitForSeconds(1f);

        // 5. Random chọn chiêu
        float rand = Random.value;
        if (rand < 0.5f)
        {
            // Cast: Mưa bom
            animator.SetBool("Cast", true);
            yield return new WaitForSeconds(0.5f);
            SpawnBombRain();
            yield return new WaitForSeconds(1.5f);
            animator.SetBool("Cast", false);
        }
        else
        {
            // Attack: Tấn công gần
            animator.SetBool("Attack", true);
            yield return new WaitForSeconds(1f);
            StartCoroutine(ShakeCamera());

            animator.SetBool("Attack", false);
        }

        // 6. Đợi thêm 2s rồi dịch sang vị trí khác
        yield return new WaitForSeconds(2f);
        yield return StartCoroutine(TeleportToRandomPoint());

        isMoving = true;
        isTeleporting = false;
    }

    IEnumerator TeleportToRandomPoint()
    {
        animator.SetBool("Spell", true);
        yield return new WaitForSeconds(0.5f);
        spriteRenderer.enabled = false;
        animator.SetBool("Spell", false);
        yield return new WaitForSeconds(0.3f);

        if (teleportPoints.Length > 0)
        {
            int index = Random.Range(0, teleportPoints.Length);
            transform.position = teleportPoints[index].position;
        }

        spriteRenderer.enabled = true;
        animator.SetBool("Spell", true);
        yield return new WaitForSeconds(0.5f);
        animator.SetBool("Spell", false);
    }

    void SpawnBombRain()
    {
        StartCoroutine(SpawnBombsCoroutine());
    }

    IEnumerator SpawnBombsCoroutine()
    {
        int bombAmount = Random.Range(10, 15); // Random từ 7 đến 9 quả bom

        for (int i = 0; i < bombAmount; i++)
        {
            Vector3 spawnPosition = transform.position + new Vector3(
                Random.Range(-bombRadius, bombRadius),
                bombSpawnHeight,
                0f
            );

            Instantiate(bombPrefab, spawnPosition, Quaternion.identity);

            // Delay ngẫu nhiên để bom rơi không cùng lúc
            yield return new WaitForSeconds(Random.Range(0.05f, 0.2f));
        }

        // Sau khi spawn xong hết thì rung camera
        StartCoroutine(ShakeCamera());
    }


    IEnumerator ShakeCamera()
    {
        isCameraShaking = true;
        float duration = 0.2f;
        float magnitude = 0.1f;
        float elapsed = 0f;

        // Lưu vị trí gốc camera trước khi rung
        originalCamPos = Camera.main.transform.localPosition;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            Camera.main.transform.localPosition = originalCamPos + new Vector3(x, y, 0);
            elapsed += Time.deltaTime;

            yield return null;
        }

        Camera.main.transform.localPosition = originalCamPos;
        isCameraShaking = false;
    }


}
