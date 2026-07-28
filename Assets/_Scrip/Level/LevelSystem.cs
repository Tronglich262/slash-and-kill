using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelSystem : MonoBehaviour
{
    public static LevelSystem Instance;

    [SerializeField] public int level;
    [SerializeField] public int currentExp;
    [SerializeField] public int expToNextLevel;
    [SerializeField] public int statPoints;

    [SerializeField] public int attack;
    [SerializeField] public int maxHP;
    [SerializeField] public int maxMP; // MP mới
    [SerializeField] public int Phongthu;
    [SerializeField] public int netranh;
    [SerializeField] public int tocdo;

    public HealthSystem healthSystem;
    private PlayerController playerController;

    public TextMeshProUGUI levelText;
    public Slider expSlider;
    public TextMeshProUGUI statPointsText;
    public TextMeshProUGUI attackText;
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI mpText; // UI cho MP
    public TextMeshProUGUI speedText; // UI cho Speed
    public GameObject skillPointPanel;

    private bool checkqua1 = false;
    private bool checkqua2 = false;
    private bool checkqua3 = false;
    private bool checkqua4 = false;
    private bool checkqua5 = false;
    private static readonly WaitForSeconds GiftMessageDuration = new WaitForSeconds(0.5f);
    private Coroutine giftMessageRoutine;

    public Button qua1Button;
    public Button qua2Button;
    public Button qua3Button;
    public Button qua4Button;
    public Button qua5Button;

    public GameObject dudieukien;
    public GameObject khongdudieukien;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        LoadLevelData();
        UpdateUI();
    }

    void Start()
    {
        if (healthSystem != null)
        {
            healthSystem.UpdateMaxHP(maxHP);
            healthSystem.Heal(maxHP);
            healthSystem.UpdateMaxMP(maxMP);
            // HP is restored when entering the game scene; MP must follow the
            // same rule so a stale PlayerPrefs value cannot start the player at 0.
            healthSystem.RestoreFullMPForNewScene();
        }
    }

    public void GainExp(int amount)
    {
        currentExp += amount;
        
        // Hiển thị floating text EXP
        if (FloatingTextManager.Instance != null)
        {
            FloatingTextManager.Instance.ShowEXP(amount);
        }

        while (currentExp >= expToNextLevel)
            LevelUp();

        SaveLevelData();
        UpdateUI();
    }

    private void LevelUp()
    {
        currentExp -= expToNextLevel;
        level++;
        expToNextLevel += 1000;
        statPoints += 10;

        // Hiển thị floating text Level Up
        if (FloatingTextManager.Instance != null)
        {
            FloatingTextManager.Instance.ShowLevelUp();
        }

        if (skillPointPanel != null)
            skillPointPanel.SetActive(true);

        SaveLevelData();
    }

    public void IncreaseAttack()
    {
        if (statPoints > 0)
        {
            attack += 3;
            statPoints--;
            UpdateUI();
            SaveLevelData();
        }
    }

    public void IncreaseHP()
    {
        if (statPoints > 0)
        {
            maxHP += 10;
            statPoints--;
            if (healthSystem != null)
                healthSystem.UpdateMaxHP(maxHP);
            UpdateUI();
            SaveLevelData();
        }
    }

    public void IncreaseMP()
    {
        if (statPoints > 0)
        {
            maxMP += 5;
            statPoints--;
            if (healthSystem != null)
                healthSystem.UpdateMaxMP(maxMP);
            UpdateUI();
            SaveLevelData();
        }
    }

    public void IncreaseSpeed()
    {
        if (statPoints > 0)
        {
            tocdo += 1;
            statPoints--;
            UpdateUI();
            SaveLevelData();
            ApplySpeedToPlayer();
        }
    }

    public void UpdateUI()
    {
        if (levelText != null) levelText.text = level.ToString();
        if (expSlider != null) expSlider.value = (float)currentExp / expToNextLevel;
        if (statPointsText != null) statPointsText.text = "Stat Points: " + statPoints;
        if (attackText != null) attackText.text = "Attack: " + attack;
        if (hpText != null) hpText.text = "HP: " + maxHP;
        if (mpText != null) mpText.text = "MP: " + maxMP;
        if (speedText != null) speedText.text = "Speed: " + tocdo;
    }

    public void SaveLevelData()
    {
        GetEquippedStatBonuses(
            out int equippedAttack,
            out int equippedHP,
            out int equippedMP,
            out int equippedDefense,
            out int equippedEvasion,
            out int equippedSpeed);

        PlayerPrefs.SetInt("Level", level);
        PlayerPrefs.SetInt("CurrentExp", currentExp);
        PlayerPrefs.SetInt("ExpToNextLevel", expToNextLevel);
        PlayerPrefs.SetInt("StatPoints", statPoints);
        // Runtime values include equipment. Persist only the character's base
        // stats, otherwise every load applies the same equipment bonus again.
        PlayerPrefs.SetInt("Attack", attack - equippedAttack);
        PlayerPrefs.SetInt("MaxHP", maxHP - equippedHP);
        PlayerPrefs.SetInt("MaxMP", maxMP - equippedMP);
        PlayerPrefs.SetInt("PhongThu", Phongthu - equippedDefense);
        PlayerPrefs.SetInt("NeTranh", netranh - equippedEvasion);
        PlayerPrefs.SetInt("TocDo", tocdo - equippedSpeed);

        PlayerPrefs.SetInt("CheckQua1", checkqua1 ? 1 : 0);
        PlayerPrefs.SetInt("CheckQua2", checkqua2 ? 1 : 0);
        PlayerPrefs.SetInt("CheckQua3", checkqua3 ? 1 : 0);
        PlayerPrefs.SetInt("CheckQua4", checkqua4 ? 1 : 0);
        PlayerPrefs.SetInt("CheckQua5", checkqua5 ? 1 : 0);

    }

    private void LoadLevelData()
    {
        if (PlayerPrefs.HasKey("Level"))
        {
            level = PlayerPrefs.GetInt("Level");
            currentExp = PlayerPrefs.GetInt("CurrentExp");
            expToNextLevel = PlayerPrefs.GetInt("ExpToNextLevel");
            statPoints = PlayerPrefs.GetInt("StatPoints");
            attack = PlayerPrefs.GetInt("Attack", 10);
            maxHP = PlayerPrefs.GetInt("MaxHP", 100);
            maxMP = PlayerPrefs.GetInt("MaxMP", 50);
            Phongthu = PlayerPrefs.GetInt("PhongThu", 0);
            netranh = PlayerPrefs.GetInt("NeTranh", 0);
            tocdo = PlayerPrefs.GetInt("TocDo", 0);

            checkqua1 = PlayerPrefs.GetInt("CheckQua1", 0) == 1;
            checkqua2 = PlayerPrefs.GetInt("CheckQua2", 0) == 1;
            checkqua3 = PlayerPrefs.GetInt("CheckQua3", 0) == 1;
            checkqua4 = PlayerPrefs.GetInt("CheckQua4", 0) == 1;
            checkqua5 = PlayerPrefs.GetInt("CheckQua5", 0) == 1;
        }
        else
        {
            ResetLevelData();
        }

        UpdateGiftButtons();
        ApplyStatsToPlayer();
    }

    public void ResetLevelData()
    {
        level = 1;
        currentExp = 0;
        expToNextLevel = 500;
        statPoints = 0;
        attack = 10;
        maxHP = 100;
        maxMP = 50;
        tocdo = 0;

        checkqua1 = checkqua2 = checkqua3 = checkqua4 = checkqua5 = false;

        SaveLevelData();
        ApplyStatsToPlayer();
    }

    // Reset tất cả chỉ số (gọi từ GameManager)
    public void ResetAllStats()
    {
        // Reset về giá trị mặc định
        level = 1;
        currentExp = 0;
        expToNextLevel = 500;
        statPoints = 0;
        attack = 10;
        maxHP = 100;
        maxMP = 50;
        Phongthu = 0;
        netranh = 0;
        tocdo = 0;

        GetEquippedStatBonuses(
            out int equippedAttack,
            out int equippedHP,
            out int equippedMP,
            out int equippedDefense,
            out int equippedEvasion,
            out int equippedSpeed);
        attack += equippedAttack;
        maxHP += equippedHP;
        maxMP += equippedMP;
        Phongthu += equippedDefense;
        netranh += equippedEvasion;
        tocdo += equippedSpeed;

        // Reset quà
        checkqua1 = checkqua2 = checkqua3 = checkqua4 = checkqua5 = false;

        // Reset HP/MP hiện tại
        if (healthSystem != null)
        {
            healthSystem.maxHP = maxHP;
            healthSystem.maxMP = maxMP;
            healthSystem.currentHP = maxHP;
            healthSystem.currentMP = maxMP;
            healthSystem.UpdateHPUI();
            healthSystem.UpdateMPUI();
            healthSystem.SaveHP();
            healthSystem.SaveMP();
        }

        // Áp dụng stats cho player
        ApplyStatsToPlayer();
        UpdateUI();
        UpdateGiftButtons();
        SaveLevelData();

        Debug.Log("Đã reset tất cả chỉ số!");
    }

    public void UpdateGiftButtons()
    {
        if (qua1Button != null) qua1Button.interactable = !checkqua1;
        if (qua2Button != null) qua2Button.interactable = !checkqua2;
        if (qua3Button != null) qua3Button.interactable = !checkqua3;
        if (qua4Button != null) qua4Button.interactable = !checkqua4;
        if (qua5Button != null) qua5Button.interactable = !checkqua5;
    }

    public void Nhanqua1() { GiveGift(1, ref checkqua1, 200000, 5000); }
    public void Nhanqua2() { GiveGift(5, ref checkqua2, 20000, 5000); }
    public void Nhanqua3() { GiveGift(10, ref checkqua3, 50000, 15000); }
    public void Nhanqua4() { GiveGift(15, ref checkqua4, 100000, 20000); }
    public void Nhanqua5() { GiveGift(20, ref checkqua5, 200000, 50000); }

    private void GiveGift(int requiredLevel, ref bool checkGift, int expAmount, int coinAmount)
    {
        if (level >= requiredLevel && !checkGift)
        {
            ShowGift(dudieukien);
            GainExp(expAmount);
            CoinManager.Instance?.AddCoin(coinAmount);

            checkGift = true;
            SaveLevelData();
            UpdateGiftButtons();
        }
        else
        {
            ShowGift(khongdudieukien);
        }
    }

    private void ShowGift(GameObject giftObj)
    {
        if (giftMessageRoutine != null)
            StopCoroutine(giftMessageRoutine);

        if (dudieukien != null)
            dudieukien.SetActive(false);
        if (khongdudieukien != null)
            khongdudieukien.SetActive(false);

        giftMessageRoutine = StartCoroutine(ShowGiftRoutine(giftObj));
    }

    private IEnumerator ShowGiftRoutine(GameObject giftObj)
    {
        if (giftObj != null)
        {
            giftObj.SetActive(true);
            yield return GiftMessageDuration;
            giftObj.SetActive(false);
        }

        giftMessageRoutine = null;
    }

    //MỚI: CỘNG/TRỪ STAT THEO InventoryItem (có levelDo)
    public void ApplyItemStats(InventoryItem item)
    {
        if (item == null) return;

        if (item.itemData == null)
            item.itemData = ItemDatabase.Instance.GetItemByID(item.itemID);

        if (item.itemData == null) return;

        attack += item.GetAttack();
        maxHP += item.GetHP();
        maxMP += item.GetMP();
        Phongthu += item.GetPhongThu();
        netranh += item.GetNeTranh();
        tocdo += item.GetTocDo();

        if (healthSystem != null)
        {
            healthSystem.UpdateMaxHP(maxHP);
            healthSystem.UpdateMaxMP(maxMP);
        }

        ApplySpeedToPlayer();
        UpdateUI();
    }

    public void RemoveItemStats(InventoryItem item)
    {
        if (item == null) return;

        if (item.itemData == null)
            item.itemData = ItemDatabase.Instance.GetItemByID(item.itemID);

        if (item.itemData == null) return;

        attack -= item.GetAttack();
        maxHP -= item.GetHP();
        maxMP -= item.GetMP();
        Phongthu -= item.GetPhongThu();
        netranh -= item.GetNeTranh();
        tocdo -= item.GetTocDo();

        if (healthSystem != null)
        {
            healthSystem.UpdateMaxHP(maxHP);
            healthSystem.UpdateMaxMP(maxMP);
        }

        ApplySpeedToPlayer();
        UpdateUI();
    }

    // Áp dụng tất cả stats cho player
    public void ApplyStatsToPlayer()
    {
        if (healthSystem != null)
        {
            healthSystem.UpdateMaxHP(maxHP);
            healthSystem.UpdateMaxMP(maxMP);
        }
        ApplySpeedToPlayer();
    }

    // Áp dụng tốc độ di chuyển cho PlayerController
    public void ApplySpeedToPlayer()
    {
        if (playerController == null && healthSystem != null)
            playerController = healthSystem.GetComponent<PlayerController>();

        if (playerController == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                playerController = player.GetComponent<PlayerController>();
        }

        if (playerController == null)
            return;

        // Tốc độ cơ bản là 5, mỗi điểm tocdo thêm 1%.
        const float baseSpeed = 5f;
        playerController.speed = baseSpeed * (1f + tocdo * 0.01f);
#if UNITY_EDITOR
        Debug.Log("Player speed updated: " + playerController.speed);
#endif
    }

    private static void GetEquippedStatBonuses(
        out int equippedAttack,
        out int equippedHP,
        out int equippedMP,
        out int equippedDefense,
        out int equippedEvasion,
        out int equippedSpeed)
    {
        equippedAttack = 0;
        equippedHP = 0;
        equippedMP = 0;
        equippedDefense = 0;
        equippedEvasion = 0;
        equippedSpeed = 0;

        EquipmentManager equipment = EquipmentManager.Instance;
        if (equipment == null || equipment.slots == null)
            return;

        foreach (EquipmentSlot slot in equipment.slots)
        {
            InventoryItem item = slot != null ? slot.currentItem : null;
            if (item == null)
                continue;

            if (item.itemData == null && ItemDatabase.Instance != null)
                item.itemData = ItemDatabase.Instance.GetItemByID(item.itemID);

            if (item.itemData == null)
                continue;

            equippedAttack += item.GetAttack();
            equippedHP += item.GetHP();
            equippedMP += item.GetMP();
            equippedDefense += item.GetPhongThu();
            equippedEvasion += item.GetNeTranh();
            equippedSpeed += item.GetTocDo();
        }
    }
}
