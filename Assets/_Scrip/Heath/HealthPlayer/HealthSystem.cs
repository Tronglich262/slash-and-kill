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
    private PlayerController playerController;
    private PlayerAttack playerAttack;
    private PlayerJump playerJump;
    private PlayerWallSlide playerWallSlide;
    private PlayerEdgeIdle playerEdgeIdle;
    private LadderClimb ladderClimb;
    private Rigidbody2D playerRigidbody;
    private Coroutine hurtRoutine;
    private static readonly WaitForSeconds HurtDuration = new WaitForSeconds(0.5f);
    private static readonly int HurtParameter = Animator.StringToHash("Hurt");
    private static readonly int HurtState = Animator.StringToHash("Base Layer.Hurt");
    private static readonly int DeathParameter = Animator.StringToHash("Death");
    private static readonly int IdleState = Animator.StringToHash("Idle");
    private static readonly int SpeedParameter = Animator.StringToHash("Speed");
    private static readonly int VerticalSpeedParameter = Animator.StringToHash("VerticalSpeed");
    private static readonly int AttackIndexParameter = Animator.StringToHash("AttackIndex");
    private float defaultGravityScale;
    private bool isReviving;
    private bool mpLoaded;
    public bool check = false;

    // Hồi sinh
    public GameObject Hoisinh;
    public bool isDead = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            enabled = false;
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        animator = GetComponent<Animator>();
        playerController = GetComponent<PlayerController>();
        playerAttack = GetComponent<PlayerAttack>();
        playerJump = GetComponent<PlayerJump>();
        playerWallSlide = GetComponent<PlayerWallSlide>();
        playerEdgeIdle = GetComponent<PlayerEdgeIdle>();
        ladderClimb = GetComponent<LadderClimb>();
        playerRigidbody = GetComponent<Rigidbody2D>();
        if (playerRigidbody != null)
            defaultGravityScale = playerRigidbody.gravityScale;
        LoadHP(); 
        LoadMP();
        UpdateHPUI();
        UpdateMPUI();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
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
        if (isDead) return; 

        // Kiểm tra né đòn dựa trên netranh (Speed stat)
        if (CheckDodge())
        {
            if (FloatingTextManager.Instance != null)
            {
                FloatingTextManager.Instance.ShowDodge();
            }
            return;
        }

        // Defense uses diminishing returns; every successful hit still deals at least 1 damage.
        int defense = LevelSystem.Instance != null ? Mathf.Max(0, LevelSystem.Instance.Phongthu) : 0;
        int finalDamage = Mathf.Max(1, Mathf.CeilToInt(damage * 100f / (100f + defense)));
        currentHP -= finalDamage;
        if (currentHP < 0) currentHP = 0;
        UpdateHPUI();
        SaveHP();
        if (playerAttack != null)
            playerAttack.ResetAttackState();

        if (hurtRoutine != null)
            StopCoroutine(hurtRoutine);
        hurtRoutine = StartCoroutine(DelayHurt());

        if (currentHP == 0)
        {
            isDead = true; // Đánh dấu đã chết
            StartCoroutine(Die());
        }
    }

    IEnumerator DelayHurt()
    {
        if (animator != null)
        {
            animator.ResetTrigger(HurtParameter);
            animator.SetTrigger(HurtParameter);
            // Some action states can interrupt or consume the trigger before the
            // Any State transition evaluates. Force the visual hit reaction too.
            if (animator.HasState(0, HurtState))
                animator.CrossFade(HurtState, 0.04f, 0, 0f);
        }
        yield return HurtDuration;
        hurtRoutine = null;
    }

    IEnumerator Die()
    {
        check = true;
        isReviving = false;

        if (hurtRoutine != null)
        {
            StopCoroutine(hurtRoutine);
            hurtRoutine = null;
        }

        SetGameplayEnabled(false);
        ResetPlayerMotionAndTraversal();

        if (animator != null)
        {
            animator.ResetTrigger(HurtParameter);
            animator.ResetTrigger(DeathParameter);
            animator.SetTrigger(DeathParameter);
        }

        if (Hoisinh != null)
            Hoisinh.SetActive(true);
        //SceneManager.LoadScene("ThiTran");
        yield break;
    }

    public void Heal(int amount)
    {
        int oldHP = currentHP;
        currentHP += amount;
        if (currentHP > maxHP) currentHP = maxHP;
        
        int healAmount = currentHP - oldHP;
        
        // Hiển thị floating text HP hồi
        if (healAmount > 0 && FloatingTextManager.Instance != null)
        {
            FloatingTextManager.Instance.ShowHP(healAmount);
        }
        
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
        maxHP = Mathf.Max(1, newMaxHP);
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);
        UpdateHPUI();
        SaveHP(); // Lưu currentHP sau khi cập nhật maxHP
    }

    // ================= MP System =================
    public void UpdateMaxMP(int newMaxMP)
    {
        maxMP = Mathf.Max(1, newMaxMP);
        // Raising an equipment/stat cap does not restore mana for free.
        currentMP = Mathf.Clamp(currentMP, 0, maxMP);
        UpdateMPUI();

        // LevelSystem applies base stats during Awake, before Start has loaded
        // saved mana. Do not overwrite a valid save with the default value 0.
        if (mpLoaded)
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
        int oldMP = currentMP;
        currentMP += amount;
        if (currentMP > maxMP) currentMP = maxMP;
        
        int restoreAmount = currentMP - oldMP;
        
        // Hiển thị floating text Mana hồi
        if (restoreAmount > 0 && FloatingTextManager.Instance != null)
        {
            FloatingTextManager.Instance.ShowMana(restoreAmount);
        }
        
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
    }

    public void LoadMP()
    {
        if (PlayerPrefs.HasKey("CurrentMP"))
        {
            currentMP = Mathf.Clamp(PlayerPrefs.GetInt("CurrentMP"), 0, maxMP);
        }
        else
        {
            currentMP = maxMP;
        }

        mpLoaded = true;
    }

    public void RestoreFullMPForNewScene()
    {
        currentMP = maxMP;
        mpLoaded = true;
        UpdateMPUI();
        SaveMP();
    }

    // Thêm hàm SaveHP() và LoadHP()
    public void SaveHP()
    {
        PlayerPrefs.SetInt("CurrentHP", currentHP);
    }

    public void LoadHP()
    {
        if (PlayerPrefs.HasKey("CurrentHP"))
        {
            currentHP = Mathf.Clamp(PlayerPrefs.GetInt("CurrentHP"), 0, maxHP);
        }
        else
        {
            currentHP = maxHP; // Nếu không có dữ liệu, đặt currentHP về maxHP
        }
    }

    //Hồi sinh
    public void ToggleYeshoisinh()
    {
        if (!isDead || isReviving)
            return;

        if (CoinManager.Instance != null) // check tiền
        {
            if (CoinManager.Instance.coinCount >= 500)
            {
                CoinManager.Instance.AddCoin(-500);
                if (Hoisinh != null)
                    Hoisinh.SetActive(false);
                isReviving = true;
                StartCoroutine(ReviveSequence());
            }
            else
            {
                Debug.Log("Không đủ tiền Hồi sinh");
            }
        }
    }

    IEnumerator ReviveSequence()
    {
        Heal(maxHP);
        ResetPlayerMotionAndTraversal();
        ResetAnimatorToIdle();

        yield return HurtDuration;

        check = false;
        isDead = false;
        SetGameplayEnabled(true);
        isReviving = false;
    }

    private void SetGameplayEnabled(bool value)
    {
        if (playerController != null)
        {
            if (!value)
                playerController.SetCanMove(false);
            playerController.enabled = value;
        }

        if (playerAttack != null)
        {
            if (!value)
                playerAttack.ResetAttackState();
            playerAttack.enabled = value;
        }

        if (playerJump != null) playerJump.enabled = value;
        if (playerWallSlide != null) playerWallSlide.enabled = value;
        if (playerEdgeIdle != null) playerEdgeIdle.enabled = value;
        if (ladderClimb != null) ladderClimb.enabled = value;
    }

    private void ResetPlayerMotionAndTraversal()
    {
        if (playerRigidbody != null)
        {
            playerRigidbody.linearVelocity = Vector2.zero;
            playerRigidbody.angularVelocity = 0f;
            playerRigidbody.gravityScale = defaultGravityScale;
        }

        if (playerJump != null)
            playerJump.ResetStateForRevive();
        if (playerWallSlide != null)
            playerWallSlide.ResetStateForRevive();
        if (playerEdgeIdle != null)
            playerEdgeIdle.ResetStateForRevive();
    }

    private void ResetAnimatorToIdle()
    {
        if (animator == null)
            return;

        animator.ResetTrigger(HurtParameter);
        animator.ResetTrigger(DeathParameter);
        animator.ResetTrigger("Jump");
        animator.ResetTrigger("Attack");
        animator.ResetTrigger("Dash");

        animator.SetFloat(SpeedParameter, 0f);
        animator.SetFloat(VerticalSpeedParameter, 0f);
        animator.SetInteger(AttackIndexParameter, 0);
        animator.SetBool("Run", false);
        animator.SetBool("IsJumping", false);
        animator.SetBool("Fall", false);
        animator.SetBool("Ladder", false);
        animator.SetBool("isEdgeIdle", false);
        animator.SetBool("isWallSliding", false);
        animator.SetBool("iswallidle", false);
        animator.SetBool("iswallgrab", false);
        animator.SetBool("skill", false);
        animator.SetBool("Attackskill", false);
        animator.SetBool("AttackSkill1", false);

        // Death has no outgoing transition, so clearing its trigger is not enough.
        animator.Play(IdleState, 0, 0f);
        animator.Update(0f);

        if (playerController != null)
            playerController.ResetStateForRevive();
    }

    public void ToggleNoHoisinh()
    {
        SceneManager.LoadScene("ThiTran");
    }
}
