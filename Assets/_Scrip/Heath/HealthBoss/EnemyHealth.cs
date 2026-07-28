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
        if (animator != null)
        {
            if (HasAnimatorParameter(animator, "Death1"))
                animator.SetBool("Death1", true);
            else
                animator.Play("Death", 0, 0f);
        }
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);

        // Hiển thị floating text Gold (EXP sẽ hiển thị từ LevelSystem)
        // Cộng EXP cho player
        if (levelSystem != null)
        {
            levelSystem.GainExp(expReward);
        }

        // Gold is awarded through pickups only. Split the configured reward
        // across a few drops so the total remains exactly goldReward.
        int remainingGold = Mathf.Max(0, goldReward);
        int coinCount = Mathf.Min(remainingGold, Random.Range(1, 4));
        for (int i = 0; i < coinCount; i++)
        {
            Vector3 spawnOffset = new Vector3(
                Random.Range(-1f, 1f),    
                Random.Range(-1f, -0.5f), 
                0
            );

            Vector3 spawnPos = transform.position + spawnOffset;

            if (coinPrefab == null) break;

            int value = Mathf.CeilToInt((float)remainingGold / (coinCount - i));
            CoinPickup pickup = CoinPickup.Spawn(coinPrefab, spawnPos, value);
            if (pickup != null)
            {
                remainingGold -= value;
            }
        }

        Destroy(gameObject);
    }



    public void TakeDamage(float damage, bool isCritical = false)
    {
        if (isDead || damageLocked) return;

        if (dodgeChance > 0f && Random.value < dodgeChance)
            return;

        float nextHealth = currentHealth - damage;
        if (!nearDeathTriggered && NearDeath != null &&
            nextHealth <= maxHealth * nearDeathThreshold)
        {
            nearDeathTriggered = true;
            damageLocked = true;
            // Keep the boss alive long enough for its last words even if one
            // very large hit would otherwise kill it immediately.
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

        // Tính hướng đẩy (hướng ngược lại với player)
        if (playerTransform == null)
            CachePlayer();

        if (allowKnockback && playerTransform != null)
        {
            Vector2 direction = transform.position - playerTransform.position;
            direction.Normalize();
            StartCoroutine(Knockback(direction));
        }

        if (currentHealth <= 0)
        {
            isDead = true;
            currentHealth = 0;
            UpdateHealthBar();
            StartCoroutine(Death());
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

        while (elapsed < knockbackDuration)
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
