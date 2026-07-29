using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHealth : MonoBehaviour
{
    private static Transform damageTextCanvas;
    private static readonly WaitForSeconds DamageDelay = new WaitForSeconds(0.3f);
    private static readonly WaitForSeconds HitDuration = new WaitForSeconds(1f);
    public Slider healthBar;
    public float maxHealth = 100f;
    public float currentHealth;
    public GameObject damageTextPrefab;
    [Header("Evasion")]
    [Range(0f, 0.75f)] public float dodgeChance;
    private Animator animator;
    private bool isDead;
    private bool damageLocked;
    private bool nearDeathTriggered;

    [Header("Boss Last Words")]
    [Range(0.01f, 0.5f)] public float nearDeathThreshold = 0.2f;
    public event System.Action<EnemyHealth> NearDeath;
    public event System.Action<EnemyHealth> HealthChanged;
    // Raised only after damage has actually been accepted. Boss AI uses this
    // to react to a player hit instead of continuing a scripted attack.
    public event System.Action<EnemyHealth, float> Damaged;

    
    public float baseDame1 = 100f;
    public float baseDame2 = 60f;
    public float baseDame3 = 70f;
    public float baseDame4 = 50f;
    public float baseDame5 = 0f;

    public LevelSystem levelSystem;

    // Cơ chế chí mạng
    public float criticalChance = 0.2f; // 20% tỉ lệ chí mạng
    public float criticalMultiplier = 2f; // 2x damage khi chí mạng

    public GameObject coinPrefab;

    // EXP và Gold khi tiêu diệt quái
    public int expReward = 10;
    public int goldReward = 5;

  
    public float knockbackForce = 3f; // Lực đẩy
    public float knockbackDuration = 0.2f; // Thời gian đẩy
    [SerializeField] private bool allowKnockback = true;
    private bool isKnockback = false; // Trạng thái đang bị đẩy
    private Transform playerTransform;
    private EnemyFSM enemyFSM;
    private Coroutine hitRoutine;

    [Header("Regular Enemy Death Reaction")]
    [SerializeField, Min(0f)] private float deathPushDuration = 0.18f;
    [SerializeField, Min(0f)] private float deathPushImpulse = 2.2f;
    [SerializeField, Min(0.1f)] private float deathLifetime = 1.35f;
    private static readonly int SkeletonDeathState = Animator.StringToHash("Base Layer.death");

    void Start()
    {
        animator = GetComponent<Animator>();
        enemyFSM = GetComponent<EnemyFSM>();
        CachePlayer();
        currentHealth = maxHealth;
        UpdateHealthBar();


        if (levelSystem == null)
            levelSystem = LevelSystem.Instance;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Chieu1")) StartCoroutine(ApplyDelayedDamage(baseDame1));
        else if (other.CompareTag("Chieu2")) StartCoroutine(ApplyDelayedDamage(baseDame2));
        else if (other.CompareTag("Chieu3")) StartCoroutine(ApplyDelayedDamage(baseDame3));
        else if (other.CompareTag("Chieu4")) StartCoroutine(ApplyDelayedDamage(baseDame4));
        else if (other.CompareTag("Chieu5")) StartCoroutine(ApplyDelayedDamage(baseDame5));
    }

    // Hàm tính damage với khả năng chí mạng - trả về cả damage và isCritical
    void CalculateDamageWithCritical(float baseDamage, out float finalDamage, out bool isCritical)
    {
        isCritical = Random.value < criticalChance;
        finalDamage = baseDamage + (levelSystem != null ? levelSystem.attack : 0);
        
        if (isCritical)
        {
            finalDamage *= criticalMultiplier;
        }
    }

    IEnumerator ApplyDelayedDamage(float baseDamage)
    {
        yield return DamageDelay;
        if (isDead)
            yield break;

        if (hitRoutine != null)
            StopCoroutine(hitRoutine);
        hitRoutine = StartCoroutine(Hit());

        CalculateDamageWithCritical(baseDamage, out float damage, out bool isCritical);
        TakeDamage(damage, isCritical);
    }

    IEnumerator Hit()
    {
        if (animator != null)
            animator.SetBool("Hit1", true);

        yield return HitDuration;

        if (animator != null)
            animator.SetBool("Hit1", false);
        hitRoutine = null;
    }

    IEnumerator Death()
    {
        Vector3 rewardPosition = transform.position;
        bool useEnemyDeathReaction = enemyFSM != null;

        if (animator != null)
        {
            if (HasAnimatorParameter(animator, "Walk1")) animator.SetBool("Walk1", false);
            if (HasAnimatorParameter(animator, "Attack1")) animator.SetBool("Attack1", false);
            if (HasAnimatorParameter(animator, "Hit1")) animator.SetBool("Hit1", false);

            if (HasAnimatorParameter(animator, "Death1"))
                animator.SetBool("Death1", true);

            if (animator.HasState(0, SkeletonDeathState))
                animator.CrossFade(SkeletonDeathState, 0.04f, 0, 0f);
            else if (!HasAnimatorParameter(animator, "Death1"))
                animator.Play("Death", 0, 0f);
        }

        if (healthBar != null)
            healthBar.gameObject.SetActive(false);

        if (useEnemyDeathReaction)
        {
            Rigidbody2D body = GetComponent<Rigidbody2D>();
            if (body != null)
            {
                body.bodyType = RigidbodyType2D.Dynamic;
                body.simulated = true;
                body.gravityScale = 0f;
                body.constraints |= RigidbodyConstraints2D.FreezeRotation;
                body.linearVelocity = Vector2.zero;

                float pushDirection = playerTransform != null &&
                                      transform.position.x < playerTransform.position.x
                    ? -1f
                    : 1f;
                body.AddForce(
                    Vector2.right * pushDirection * deathPushImpulse,
                    ForceMode2D.Impulse);
                body.WakeUp();
            }

            yield return new WaitForSeconds(deathPushDuration);
            if (body != null)
                body.linearVelocity = Vector2.zero;

            yield return new WaitForSeconds(Mathf.Max(
                0.05f,
                deathLifetime - deathPushDuration));
        }
        else
        {
            yield return null;
            float animationLength = animator != null
                ? animator.GetCurrentAnimatorStateInfo(0).length
                : deathLifetime;
            yield return new WaitForSeconds(Mathf.Max(0.1f, animationLength));
        }

        if (levelSystem != null)
            levelSystem.GainExp(expReward);

        int remainingGold = Mathf.Max(0, goldReward);
        int coinCount = Mathf.Min(remainingGold, Random.Range(1, 4));
        for (int i = 0; i < coinCount; i++)
        {
            Vector3 spawnOffset = new Vector3(
                Random.Range(-1f, 1f),
                Random.Range(-1f, -0.5f),
                0f);

            if (coinPrefab == null)
                break;

            int value = Mathf.CeilToInt((float)remainingGold / (coinCount - i));
            CoinPickup pickup = CoinPickup.Spawn(
                coinPrefab,
                rewardPosition + spawnOffset,
                value);
            if (pickup != null)
                remainingGold -= value;
        }

        Destroy(gameObject);
    }

    public void TakeDamage(float damage, bool isCritical = false)
    {
        if (isDead || damageLocked)
            return;

        if (dodgeChance > 0f && Random.value < dodgeChance)
            return;

        float nextHealth = currentHealth - damage;
        if (!nearDeathTriggered && NearDeath != null &&
            nextHealth <= maxHealth * nearDeathThreshold)
        {
            nearDeathTriggered = true;
            damageLocked = true;
            currentHealth = Mathf.Max(1f, nextHealth);
            UpdateHealthBar();
            ShowDamageText(damage, isCritical);
            Damaged?.Invoke(this, damage);
            NearDeath.Invoke(this);
            return;
        }

        currentHealth = nextHealth;
        UpdateHealthBar();
        ShowDamageText(damage, isCritical);
        Damaged?.Invoke(this, damage);

        if (currentHealth <= 0f)
        {
            isDead = true;
            currentHealth = 0f;
            UpdateHealthBar();

            if (hitRoutine != null)
            {
                StopCoroutine(hitRoutine);
                hitRoutine = null;
            }

            isKnockback = false;
            enemyFSM?.EnterDeathState();
            StartCoroutine(Death());
            return;
        }

        if (playerTransform == null)
            CachePlayer();

        if (allowKnockback && playerTransform != null && !isKnockback)
        {
            Vector2 direction = transform.position - playerTransform.position;
            direction.Normalize();
            StartCoroutine(Knockback(direction));
        }
    }

    // Coroutine xử lý knockback - đẩy lùi enemy khi nhận damage
    IEnumerator Knockback(Vector2 direction)
    {
        if (isKnockback) yield break; // Nếu đang bị đẩy thì không đẩy tiếp

        isKnockback = true;

        // Đồng bộ trạng thái với EnemyFSM
        if (enemyFSM != null)
        {
            enemyFSM.SetKnockbackState(true);
        }

        // Chỉ lấy hướng ngang (X axis) - không đẩy xuống đất
        float knockbackDirectionX = direction.x;
        Vector2 originalPosition = transform.position;

        float elapsed = 0f;

        while (elapsed < knockbackDuration && !isDead)
        {
            // Di chuyển enemy theo hướng ngang, giữ nguyên Y
            float newX = transform.position.x + (knockbackDirectionX * knockbackForce * Time.deltaTime);
            transform.position = new Vector2(newX, transform.position.y);
            elapsed += Time.deltaTime;
            yield return null;
        }

        isKnockback = false;

        // Đồng bộ trạng thái với EnemyFSM
        if (enemyFSM != null)
        {
            enemyFSM.SetKnockbackState(false);
        }
    }

    private void CachePlayer()
    {
        if (HealthSystem.Instance != null)
        {
            playerTransform = HealthSystem.Instance.transform;
            return;
        }

        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
            playerTransform = player.transform;
    }

    void ShowDamageText(float damage, bool isCritical = false)
    {
        if (damageTextPrefab != null)
        {
            if (damageTextCanvas == null)
            {
                GameObject canvas = GameObject.Find("Canvas");
                if (canvas != null)
                    damageTextCanvas = canvas.transform;
            }

            if (damageTextCanvas == null)
            {
                Debug.LogWarning("Damage text skipped: Canvas was not found.");
                return;
            }

            DamageText text = DamageText.Spawn(damageTextPrefab, damageTextCanvas);
            if (text != null)
                text.Setup((int)damage, transform, isCritical);
        }
    }

    void UpdateHealthBar()
    {
        if (healthBar != null)
        {
            healthBar.value = currentHealth / maxHealth;
        }

        HealthChanged?.Invoke(this);
    }

    public void ReleaseNearDeathDamageLock()
    {
        damageLocked = false;
    }

    public void RestoreToFullHealth()
    {
        if (isDead)
            return;

        currentHealth = maxHealth;
        damageLocked = false;
        UpdateHealthBar();
    }

    public void RestoreHealth(float amount)
    {
        if (isDead || amount <= 0f || currentHealth >= maxHealth)
            return;

        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        UpdateHealthBar();
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
}
