using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class HealthSystem : MonoBehaviour
{
    public int maxHP = 100;
    public int currentHP;
    public Image hpBar;
    public TextMeshProUGUI hpText;
    public int attackDamage = 10;
    private Animator animator;
    public bool check = false;

    // Hồi sinh
    public GameObject Hoisinh;
    private bool isDead = false;

    private void Start()
    {
        animator = GetComponent<Animator>();
        LoadHP(); // Tải lại currentHP khi scene được load
        UpdateHPUI();
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return; // Không nhận sát thương nếu đã chết

        currentHP -= damage;
        if (currentHP < 0) currentHP = 0;
        UpdateHPUI();
        SaveHP();
        StartCoroutine(deleyhurt());
        if (currentHP == 0)
        {
            isDead = true; // Đánh dấu đã chết
            StartCoroutine(Die());
        }
    }

    IEnumerator deleyhurt()
    {
        animator.SetBool("Hurt", true);
        yield return new WaitForSeconds(0.5f);
        animator.SetBool("Hurt", false);
    }

    IEnumerator Die()
    {
        check = true;
        animator.SetBool("Death", true);
        GetComponent<PlayerController>().enabled = false;
        GetComponent<PlayerAttack>().enabled = false;
        GetComponent<PlayerJump>().enabled = false;

        yield return new WaitForSeconds(2f);
        Hoisinh.SetActive(true);
        //SceneManager.LoadScene("ThiTran");
    }

    public void Heal(int amount)
    {
        currentHP += amount;
        if (currentHP > maxHP) currentHP = maxHP;
        UpdateHPUI();
        SaveHP(); // Lưu currentHP sau khi hồi máu
    }

    public void UpdateHPUI()
    {
        hpBar.fillAmount = (float)currentHP / maxHP;
        hpText.text = currentHP + " / " + maxHP;
    }

    public void UpdateMaxHP(int newMaxHP)
    {
        maxHP = newMaxHP;
        UpdateHPUI();
        SaveHP(); // Lưu currentHP sau khi cập nhật maxHP
    }

    // Thêm hàm SaveHP() và LoadHP()
    public void SaveHP()
    {
        PlayerPrefs.SetInt("CurrentHP", currentHP);
        PlayerPrefs.Save();
    }

    public void LoadHP()
    {
        if (PlayerPrefs.HasKey("CurrentHP"))
        {
            currentHP = PlayerPrefs.GetInt("CurrentHP");
        }
        else
        {
            currentHP = maxHP; // Nếu không có dữ liệu, đặt currentHP về maxHP
        }
    }

    //Hồi sinh
    public void ToggleYeshoisinh()
    {
        check = false;

        if (CoinManager.Instance != null) // check tiền
        {
            if (CoinManager.Instance.coinCount >= 500)
            {
                CoinManager.Instance.AddCoin(-500);
                Hoisinh.SetActive(false);
                animator.SetBool("Death", false);
                Heal(maxHP);
                isDead = false;
                GetComponent<PlayerController>().enabled = true;
                GetComponent<PlayerAttack>().enabled = true;
                GetComponent<PlayerJump>().enabled = true;

            }
            else
            {
                Debug.Log("Không đủ tiền Hồi sinh");

            }
        }
    }

    public void ToggleNoHoisinh()
    {
        SceneManager.LoadScene("ThiTran");
    }
}