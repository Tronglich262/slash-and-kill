using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class LevelSystem : MonoBehaviour
{
    public int level;
    public int currentExp;
    public int expToNextLevel;
    public int statPoints;
    public int attack;
    public int maxHP; // Lưu trữ maxHP thay vì hp
    public HealthSystem healthSystem;
    public static bool isGameRestarted = false;

    public TextMeshProUGUI levelText;
    public Slider expSlider;
    public TextMeshProUGUI statPointsText;
    public TextMeshProUGUI attackText;
    public TextMeshProUGUI hpText;
    public GameObject skillPointPanel;
    public static LevelSystem Instance;

    // Nhanqua
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

    // thông báo nhận quà
    public GameObject dudieukien;
    public GameObject khongdudieukien;
    public void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        LoadLevelData();
        UpdateUI();
    }

    public void GainExp(int amount)
    {
        currentExp += amount;
        while (currentExp >= expToNextLevel)
        {
            LevelUp();
        }

        SaveLevelData();
        UpdateUI();
    }

    private void LevelUp()
    {
        currentExp -= expToNextLevel;
        level++;
        expToNextLevel += 1000;
        statPoints += 10; // Nhận 10 điểm kỹ năng khi lên cấp

        skillPointPanel.SetActive(true); // Mở bảng cộng điểm kỹ năng khi lên cấp

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
            healthSystem.UpdateMaxHP(maxHP); // Cập nhật maxHP trong HealthSystem
            UpdateUI();
            SaveLevelData();
        }
    }

    public void UpdateUI()
    {
        levelText.text = "" + level;
        expSlider.value = (float)currentExp / expToNextLevel;
        statPointsText.text = "Stat Points: " + statPoints;
        attackText.text = "Attack: " + attack;
        hpText.text = "HP: " + maxHP; // Hiển thị maxHP
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
            healthSystem.UpdateMaxHP(maxHP);
            healthSystem.Heal(maxHP);

            // Load trạng thái nhận quà
            checkqua1 = PlayerPrefs.GetInt("CheckQua1", 0) == 1;
            checkqua2 = PlayerPrefs.GetInt("CheckQua2", 0) == 1;
            checkqua3 = PlayerPrefs.GetInt("CheckQua3", 0) == 1;
            checkqua4 = PlayerPrefs.GetInt("CheckQua4", 0) == 1;
            checkqua5 = PlayerPrefs.GetInt("CheckQua5", 0) == 1;

            Debug.Log("Loaded data: Level=" + level + ", EXP=" + currentExp);
        }
        else
        {
            Debug.Log("No save data found, resetting.");
            ResetLevelData();
        }

        UpdateGiftButtons(); // Cập nhật UI trạng thái quà
    }


    public void ResetLevelData()
    {
        level = 1;
        currentExp = 0;
        expToNextLevel = 500;
        statPoints = 0;
        attack = 10;
        maxHP = 100;

        checkqua1 = false;
        checkqua2 = false;
        checkqua3 = false;
        checkqua4 = false;
        checkqua5 = false;

        SaveLevelData();
    }


    public void ResetGame()
    {
        ResetLevelData();
        UpdateUI();
    }

    private void OnApplicationQuit()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
    }

    // Nhận quà
    public void UpdateGiftButtons()
    {
        if (checkqua1 && qua1Button != null) qua1Button.interactable = false;
        if (checkqua2 && qua2Button != null) qua2Button.interactable = false;
        if (checkqua3 && qua3Button != null) qua3Button.interactable = false;
        if (checkqua4 && qua4Button != null) qua4Button.interactable = false;
        if (checkqua5 && qua5Button != null) qua5Button.interactable = false;
    }

    public void Nhanqua1()
    {
        if (level >= 1 && !checkqua1)
        {
            StartCoroutine(Dieukien());
            GainExp(200000);
            CoinManager.Instance.AddCoin(5000);
            checkqua1 = true;
            SaveLevelData();
            UpdateGiftButtons();
        }
        else
        {
            StartCoroutine(khongduDieukien());
        }
    }

    public void Nhanqua2()
    {
        if (level >= 5 && !checkqua2)
        {
            StartCoroutine(Dieukien());
            GainExp(20000);
            CoinManager.Instance.AddCoin(5000);
            checkqua2 = true;
            SaveLevelData();
            UpdateGiftButtons();
        }
        else
        {
            StartCoroutine(khongduDieukien());
        }
    }

    public void Nhanqua3()
    {
        if (level >= 10 && !checkqua3)
        {
            StartCoroutine(Dieukien());
            GainExp(50000);
            CoinManager.Instance.AddCoin(15000);
            checkqua3 = true;
            SaveLevelData();
            UpdateGiftButtons();
        }
        else
        {
            StartCoroutine(khongduDieukien());
        }
    }

    public void Nhanqua4()
    {
        if (level >= 15 && !checkqua4)
        {
            StartCoroutine(Dieukien());
            GainExp(100000);
            CoinManager.Instance.AddCoin(20000);
            checkqua4 = true;
            SaveLevelData();
            UpdateGiftButtons();
        }
        else
        {
            StartCoroutine(khongduDieukien());
        }
    }

    public void Nhanqua5()
    {
        if (level >= 20 && !checkqua5)
        {
            StartCoroutine(Dieukien());
            GainExp(200000);
            CoinManager.Instance.AddCoin(50000);
            checkqua5 = true;
            SaveLevelData();
            UpdateGiftButtons();
        }
        else
        {
            StartCoroutine(khongduDieukien());
        }
    }

    public IEnumerator Dieukien()
    {
        dudieukien.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        dudieukien.SetActive(false);
    }

    public IEnumerator khongduDieukien()
    {
        khongdudieukien.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        khongdudieukien.SetActive(false);
    }

    /*public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        healthSystem.Heal(maxHP); // Hồi đầy máu khi load scene
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }*/
}