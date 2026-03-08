using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class HealthSystem : MonoBehaviour
{
    public static HealthSystem Instance;

    public int maxHP = 100;
    public int currentHP;
    public Image hpBar;
    public TextMeshProUGUI hpText;
    public int attackDamage = 10;

    // MP System
    public int maxMP = 50;
    public int currentMP; 
    public Image mpBar;
    public TextMeshProUGUI mpText;

    private Animator animator;
    public bool check = false;

    // Hồi sinh
    public GameObject Hoisinh;
    private bool isDead = false;

    private void Start()
    {
        // Singleton
        if (Instance == null) Instance = this;

        animator = GetComponent<Animator>();
        LoadHP(); // Tải lại currentHP khi scene được load
        LoadMP();
        UpdateHPUI();
        UpdateMPUI();
    }

    // Kiểm tra né đòn dựa trên netranh (Speed stat)
    // Mỗi 1 điểm netranh = 1% cơ hội né đòn, max 50%
    private bool CheckDodge()
    {
        if (LevelSystem.Instance == null) return false;

        int netranh = LevelSystem.Instance.netranh;
        // Giới hạn max dodge chance là 50%
        int dodgeChance = Mathf.Min(netranh, 50);

        int randomValue = Random.Range(0, 100);
        return randomValue < dodgeChance;
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return; // Không nhận sát thương nếu đã chết

        // Kiểm tra né đòn dựa trên netranh (Speed stat)
        if (CheckDodge())
        {
            Debug.Log("Né đòn thành công!");
            // Có thể hiện hiệu ứng né đòn ở đây
            return;
        }

        currentHP -= damage;
        if (currentHP < 0) currentHP = 0;
        UpdateHPUI();
        SaveHP();
        PlayerAttack playerAttack = GetComponent<PlayerAttack>();
        if (playerAttack != null)
        {
            playerAttack.ResetAttackState();
        }
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

    // ================= MP System =================
    public void UpdateMaxMP(int newMaxMP)
    {
        maxMP = newMaxMP;
        // Nếu currentMP = 0 hoặc nhỏ hơn maxMP cũ, set về maxMP mới
        if (currentMP == 0 || currentMP < maxMP)
            currentMP = maxMP;
        UpdateMPUI();
        SaveMP();
    }

    public void UseMP(int amount)
    {
        if (currentMP >= amount)
        {
            currentMP -= amount;
            UpdateMPUI();
            SaveMP();
        }
        else
        {
            Debug.Log("Không đủ MP!");
        }
    }

    public void RestoreMP(int amount)
    {
        currentMP += amount;
        if (currentMP > maxMP) currentMP = maxMP;
        UpdateMPUI();
        SaveMP();
    }

    public void UpdateMPUI()
    {
        if (mpBar != null)
            mpBar.fillAmount = (float)currentMP / maxMP;
        if (mpText != null)
            mpText.text = currentMP + " / " + maxMP;
    }

    public void SaveMP()
    {
        PlayerPrefs.SetInt("CurrentMP", currentMP);
        PlayerPrefs.Save();
    }

    public void LoadMP()
    {
        if (PlayerPrefs.HasKey("CurrentMP"))
        {
            currentMP = PlayerPrefs.GetInt("CurrentMP");
            // Đảm bảo không bị 0
            if (currentMP <= 0) currentMP = maxMP;
        }
        else
        {
            currentMP = maxMP;
        }
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