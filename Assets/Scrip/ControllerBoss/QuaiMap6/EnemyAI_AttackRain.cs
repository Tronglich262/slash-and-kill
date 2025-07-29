using UnityEngine;
using System.Collections;

public class EnemyCastRainAttack : MonoBehaviour
{
    public Transform player;
    public GameObject projectilePrefab;
    public float attackRange = 10f;
    public float shootCooldown = 3f;
    public int numberOfProjectiles = 6;
    public float spawnHeight = 10f;
    public float areaRadius = 5f;
    public float fallSpeed = 10f;

    private Animator animator;
    private bool isAttacking;
    private bool isCameraShaking = false;
    private Vector3 originalCamPos;

    
    public GameObject explosionProjectilePrefab;
    public float explosionSpeed = 7f;
    public int explosionCount = 8;
    
    public Transform[] teleportPoints;
    


    void Start()
    {
        animator = GetComponent<Animator>();
        originalCamPos = Camera.main.transform.localPosition;
    }

    void Update()
    {
        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= attackRange)
        {
            animator.SetBool("Attack", true);

            if (!isAttacking)
            {
                isAttacking = true;
                InvokeRepeating(nameof(RainAttack), 0f, shootCooldown);
            }
        }
        else
        {
            animator.SetBool("Attack", false);

            if (isAttacking)
            {
                isAttacking = false;
                CancelInvoke(nameof(RainAttack));
                StopAllCoroutines();
                ResetCamera();
            }
        }
    }

    void RainAttack()
    {
        if (!isAttacking)
        {
            isAttacking = true;
            Invoke(nameof(RainAttack), 0f);
        }

        for (int i = 0; i < numberOfProjectiles; i++)
        {
            Vector2 randomOffset = Random.insideUnitCircle * areaRadius;
            Vector2 spawnPos = new Vector2(player.position.x, player.position.y) + randomOffset;
            spawnPos.y += spawnHeight;

            GameObject proj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
            Rigidbody2D rb = proj.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.down * fallSpeed;
            }

            if (!isCameraShaking)
                StartCoroutine(ShakeCamera());

            Destroy(proj, 5f);
        }

        // Sau mưa thì tung chiêu tỏa ra
        StartCoroutine(DelayWaveExplosion());
    }

    IEnumerator ShakeCamera()
    {
        isCameraShaking = true;
        float duration = 0.2f;
        float magnitude = 0.1f;
        float elapsed = 0f;

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

    void ResetCamera()
    {
        Camera.main.transform.localPosition = originalCamPos;
        isCameraShaking = false;
    }
    void CastWaveExplosion()
    {
        Vector2 center = transform.position;

        for (int i = 0; i < explosionCount; i++)
        {
            float angle = i * (360f / explosionCount);
            Vector2 dir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));

            GameObject proj = Instantiate(explosionProjectilePrefab, center, Quaternion.identity);
            Rigidbody2D rb = proj.GetComponent<Rigidbody2D>();

            if (rb != null)
            {
                rb.linearVelocity = dir * explosionSpeed;
            }
            if (!isCameraShaking)
                StartCoroutine(ShakeCamera());

            Destroy(proj, 4f);
        }
    }
    
    IEnumerator DelayWaveExplosion()
    {
        yield return new WaitForSeconds(1f); // Delay sau mưa
        CastWaveExplosion();

        yield return new WaitForSeconds(0.5f); // Delay trước khi dịch chuyển
        TeleportToRandomPoint();
    }
    void TeleportToRandomPoint()
    {
        if (teleportPoints.Length == 0) return;

        int index = Random.Range(0, teleportPoints.Length);
        transform.position = teleportPoints[index].position;

        // Reset camera nếu cần
        ResetCamera();
    }




}
