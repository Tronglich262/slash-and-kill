using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHealth : MonoBehaviour
{
    public Slider healthBar;
    public float maxHealth = 100f;
    public float currentHealth;
    public GameObject damageTextPrefab;
    private Animator animator;

    
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
    private bool isKnockback = false; // Trạng thái đang bị đẩy
    private Vector2 knockbackDirection; // Hướng đẩy

    void Start()
    {
        animator = GetComponent<Animator>();
        currentHealth = maxHealth;
        UpdateHealthBar();


        if (levelSystem == null)
        {
            levelSystem = FindFirstObjectByType<LevelSystem>();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Chieu1")) StartCoroutine(DameChieu1());
        if (other.CompareTag("Chieu2")) StartCoroutine(DameChieu2());
        if (other.CompareTag("Chieu3")) StartCoroutine(DameChieu3());
        if (other.CompareTag("Chieu4")) StartCoroutine(DameChieu4());
        if (other.CompareTag("Chieu5")) StartCoroutine(DameChieu5());
    }

    // Hàm tính damage với khả năng chí mạng - trả về cả damage và isCritical
    void CalculateDamageWithCritical(float baseDamage, out float finalDamage, out bool isCritical)
    {
        isCritical = Random.value < criticalChance;
        finalDamage = baseDamage + levelSystem.attack;
        
        if (isCritical)
        {
            finalDamage *= criticalMultiplier;
        }
    }

    IEnumerator DameChieu1() { yield return new WaitForSeconds(0.3f); StartCoroutine(hit()); CalculateDamageWithCritical(baseDame1, out float damage1, out bool isCrit1); TakeDamage(damage1, isCrit1); }
    IEnumerator DameChieu2() { yield return new WaitForSeconds(0.3f); StartCoroutine(hit()); CalculateDamageWithCritical(baseDame2, out float damage2, out bool isCrit2); TakeDamage(damage2, isCrit2); }
    IEnumerator DameChieu3() { yield return new WaitForSeconds(0.3f); StartCoroutine(hit()); CalculateDamageWithCritical(baseDame3, out float damage3, out bool isCrit3); TakeDamage(damage3, isCrit3); }
    IEnumerator DameChieu4() { yield return new WaitForSeconds(0.3f); StartCoroutine(hit()); CalculateDamageWithCritical(baseDame4, out float damage4, out bool isCrit4); TakeDamage(damage4, isCrit4); }
    IEnumerator DameChieu5() { yield return new WaitForSeconds(0.3f); StartCoroutine(hit()); CalculateDamageWithCritical(levelSystem.attack, out float damage5, out bool isCrit5); TakeDamage(damage5, isCrit5); }

    IEnumerator hit()
    {
        animator.SetBool("Hit1", true);
        yield return new WaitForSeconds(1f);
        animator.SetBool("Hit1", false);
    }

    IEnumerator Death()
    {
        animator.SetBool("Death1", true);
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);

        // Hiển thị floating text Gold (EXP sẽ hiển thị từ LevelSystem)
        if (FloatingTextManager.Instance != null)
        {
            FloatingTextManager.Instance.ShowGold(goldReward);
        }

        // Cộng EXP cho player
        if (levelSystem != null)
        {
            levelSystem.GainExp(expReward);
        }

        // Cộng Gold cho player
        if (CoinManager.Instance != null)
        {
            CoinManager.Instance.AddCoin(goldReward);
        }

        // Spawn coin rải rác dưới đất
        int coinCount = Random.Range(1, 11);
        for (int i = 0; i < coinCount; i++)
        {
            Vector3 spawnOffset = new Vector3(
                Random.Range(-1f, 1f),    
                Random.Range(-1f, -0.5f), 
                0
            );

            Vector3 spawnPos = transform.position + spawnOffset;

            GameObject coin = Instantiate(coinPrefab, spawnPos, Quaternion.identity);
        }

        Destroy(gameObject);
    }



    public void TakeDamage(float damage, bool isCritical = false)
    {
        currentHealth -= damage;
        UpdateHealthBar();
        ShowDamageText(damage, isCritical);

        // Tính hướng đẩy (hướng ngược lại với player)
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            Vector2 direction = transform.position - player.transform.position;
            direction.Normalize();
            StartCoroutine(Knockback(direction));
        }

        if (currentHealth <= 0) StartCoroutine(Death());
    }

    // Coroutine xử lý knockback - đẩy lùi enemy khi nhận damage
    IEnumerator Knockback(Vector2 direction)
    {
        if (isKnockback) yield break; // Nếu đang bị đẩy thì không đẩy tiếp

        isKnockback = true;

        // Đồng bộ trạng thái với EnemyFSM
        EnemyFSM enemyFSM = GetComponent<EnemyFSM>();
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

    void ShowDamageText(float damage, bool isCritical = false)
    {
        if (damageTextPrefab != null)
        {
            Vector3 screenPosition = Camera.main.WorldToScreenPoint(transform.position + new Vector3(0, 1f, 0));
            GameObject text = Instantiate(damageTextPrefab, GameObject.Find("Canvas").transform);
            text.GetComponent<DamageText>().Setup((int)damage, this.transform, isCritical);
        }
    }

    void UpdateHealthBar()
    {
        if (healthBar != null)
        {
            healthBar.value = currentHealth / maxHealth;
        }
    }
}
