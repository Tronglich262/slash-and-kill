using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class LevelSystem : MonoBehaviour
{
    public static LevelSystem Instance;

    public int level;
    public int currentExp;
    public int expToNextLevel;
    public int statPoints;
    public int attack;
    public int maxHP;
    public HealthSystem healthSystem;

    public TextMeshProUGUI levelText;
    public Slider expSlider;
    public TextMeshProUGUI statPointsText;
    public TextMeshProUGUI attackText;
    public TextMeshProUGUI hpText;
    public GameObject skillPointPanel;

    // Quà tặng
    private bool checkqua1 = false;
    private bool checkqua2 = false;
    private bool checkqua3 = false;
    private bool checkqua4 = false;
    private bool checkqua5 = false;

    public Button qua1Button;
    public Button qua2Button;
    public Button qua3Button;
    public Button qua4Button;
    public Button qua5Button;

    public GameObject dudieukien;
    public GameObject khongdudieukien;

    private void Awake()
    {
        // Singleton + DontDestroyOnLoad
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
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
        }
    }

    public void GainExp(int amount)
    {
        currentExp += amount;
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

    public void UpdateUI()
    {
        if (levelText != null) levelText.text = level.ToString();
        if (expSlider != null) expSlider.value = (float)currentExp / expToNextLevel;
        if (statPointsText != null) statPointsText.text = "Stat Points: " + statPoints;
        if (attackText != null) attackText.text = "Attack: " + attack;
        if (hpText != null) hpText.text = "HP: " + maxHP;
    }

    public void SaveLevelData()
    {
        PlayerPrefs.SetInt("Level", level);
        PlayerPrefs.SetInt("CurrentExp", currentExp);
        PlayerPrefs.SetInt("ExpToNextLevel", expToNextLevel);
        PlayerPrefs.SetInt("StatPoints", statPoints);
        PlayerPrefs.SetInt("Attack", attack);
        PlayerPrefs.SetInt("MaxHP", maxHP);

        PlayerPrefs.SetInt("CheckQua1", checkqua1 ? 1 : 0);
        PlayerPrefs.SetInt("CheckQua2", checkqua2 ? 1 : 0);
        PlayerPrefs.SetInt("CheckQua3", checkqua3 ? 1 : 0);
        PlayerPrefs.SetInt("CheckQua4", checkqua4 ? 1 : 0);
        PlayerPrefs.SetInt("CheckQua5", checkqua5 ? 1 : 0);

        PlayerPrefs.Save();
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
    }

    public void ResetLevelData()
    {
        level = 1;
        currentExp = 0;
        expToNextLevel = 500;
        statPoints = 0;
        attack = 10;
        maxHP = 100;

        checkqua1 = checkqua2 = checkqua3 = checkqua4 = checkqua5 = false;

        SaveLevelData();
    }

    public void ResetGame()
    {
        ResetLevelData();
        UpdateUI();
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
            StartCoroutine(ShowGift(dudieukien));
            GainExp(expAmount);
            CoinManager.Instance?.AddCoin(coinAmount);

            checkGift = true;
            SaveLevelData();
            UpdateGiftButtons();
        }
        else
        {
            StartCoroutine(ShowGift(khongdudieukien));
        }
    }

    private IEnumerator ShowGift(GameObject giftObj)
    {
        if (giftObj != null)
        {
            giftObj.SetActive(true);
            yield return new WaitForSeconds(0.5f);
            giftObj.SetActive(false);
        }
    }
    public IEnumerator Dieukien()
    {
        if (dudieukien != null) dudieukien.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        if (dudieukien != null) dudieukien.SetActive(false);
    }

    public IEnumerator khongduDieukien()
    {
        if (khongdudieukien != null) khongdudieukien.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        if (khongdudieukien != null) khongdudieukien.SetActive(false);
    }

}
