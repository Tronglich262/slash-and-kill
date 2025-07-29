using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthTwo : MonoBehaviour
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

    void Start()
    {
        animator = GetComponent<Animator>();
        currentHealth = maxHealth;
        UpdateHealthBar();
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
        yield return new WaitForSeconds(0.5f);
        animator.SetBool("Hit1", false);
    }

    IEnumerator Death()
    {
        animator.SetBool("Death1", true);
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);

        // 🪙 Spawn coin rải rác dưới đất
        int coinCount = Random.Range(1, 11);
        for (int i = 0; i < coinCount; i++)
        {
            Vector3 spawnOffset = new Vector3(
                Random.Range(-1f, 1f),    // rải ngang
                Random.Range(-1f, -0.5f), // thấp xuống so với chân quái
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
        if (currentHealth <= 0) StartCoroutine(Death());
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
