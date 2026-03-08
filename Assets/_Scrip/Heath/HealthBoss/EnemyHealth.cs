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

    IEnumerator DameChieu1() { yield return new WaitForSeconds(0.3f); StartCoroutine(hit()); TakeDamage(baseDame1 + levelSystem.attack); }
    IEnumerator DameChieu2() { yield return new WaitForSeconds(0.3f); StartCoroutine(hit()); TakeDamage(baseDame2 + levelSystem.attack); }
    IEnumerator DameChieu3() { yield return new WaitForSeconds(0.3f); StartCoroutine(hit()); TakeDamage(baseDame3 + levelSystem.attack); }
    IEnumerator DameChieu4() { yield return new WaitForSeconds(0.3f); StartCoroutine(hit()); TakeDamage(baseDame4 + levelSystem.attack); }
    IEnumerator DameChieu5() { yield return new WaitForSeconds(0.3f); StartCoroutine(hit()); TakeDamage(levelSystem.attack); }

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



    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        UpdateHealthBar();
        ShowDamageText(damage);

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

    void ShowDamageText(float damage)
    {
        if (damageTextPrefab != null)
        {
            Vector3 screenPosition = Camera.main.WorldToScreenPoint(transform.position + new Vector3(0, 1f, 0));
            GameObject text = Instantiate(damageTextPrefab, GameObject.Find("Canvas").transform);
            text.GetComponent<DamageText>().Setup((int)damage, this.transform);
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
