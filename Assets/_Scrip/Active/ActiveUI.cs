using UnityEngine;
using UnityEngine.SceneManagement;



public class ActiveUI : MonoBehaviour
{
    [Header("Gameobject của bản đồ")]
    public GameObject ActiveBando;
    public GameObject help;
    public GameObject ActiveCoins1;
    public GameObject ActiveCoins2;
    public GameObject ActiveCoins3;
    public GameObject ActiveCoins4;
    public GameObject ActiveCoins5;
    public GameObject ActiveCoins6;
    public GameObject ActiveCoins7;
    public GameObject gohome;

    [Header("Tắt bật UI")]
    public GameObject anUI;
    public GameObject hienUI;

    [Header("Setting UI")]
    public GameObject Buttonbando;
    public GameObject ButtonHelp;
    public GameObject Buttongohome;
    public GameObject Buttonthanhtich;

    [Header("Bảng nhận thưởng nhiệm vụ")]
    public GameObject Bangthanhtich;
    public GameObject buttonthanhtich1;

    [Header("Trang bị , túi đồ, kỹ năng player")]
    public GameObject buttontrangbi;
    public GameObject buttontuidodung;
    public GameObject buttonkynang;
    public GameObject buttonChiso;
    public GameObject trangbipanel;
    public GameObject tuidodungpanel;
    public GameObject kynangpanel;
    public GameObject Chisopanel;
    public GameObject skillPointPanel;

    [Header("Character UI")]
    public GameObject SkilCharacterUI;
    public NPCClick npcClick; // Tham chiếu đến script NPCClick để tắt/bật raycast

    [Header("LevelSystem")]
    public LevelSystem levelSystem; // Gán LevelSystem trong inspector hoặc dùng LevelSystem.Instance
    public static ActiveUI instance;
    public string spawnPointName = "TownGate"; // Đặt tên spawn point ở map thị trấn

   public void Awake()
{
    if (instance != null && instance != this)
    {
        Destroy(gameObject);
        return;
    }
    instance = this;
    DontDestroyOnLoad(gameObject);
}



    // ================= Character UI =================
    public void ToggleCharacterUI()
    {
        bool isOpening = !SkilCharacterUI.activeSelf;
        SkilCharacterUI.SetActive(isOpening);

        // Tắt/Bật raycast khi Character UI bật/tắt
        if (npcClick != null)
        {
            npcClick.enabled = !isOpening;
        }
    }
    public void DisCharacterUI()
    {
        SkilCharacterUI.SetActive(false);
        // Bật lại raycast khi tắt Character UI
        if (npcClick != null)
        {
            npcClick.enabled = true;
        }
    }
    public void Trangbi()
    {
        trangbipanel.SetActive(true);
        tuidodungpanel.SetActive(true);
        kynangpanel.SetActive(false);
        Chisopanel.SetActive(false);
        skillPointPanel.SetActive(false);
    }
    public void TuiDoDung()
    {
        trangbipanel.SetActive(true);
        tuidodungpanel.SetActive(true);
        kynangpanel.SetActive(false);
        Chisopanel.SetActive(false);
        skillPointPanel.SetActive(false);
    }
    public void KyNang()
    {
        trangbipanel.SetActive(true);
        tuidodungpanel.SetActive(false);
        kynangpanel.SetActive(true);
        Chisopanel.SetActive(false);
        skillPointPanel.SetActive(false);
    }
    public void ChiSo()
    {
        trangbipanel.SetActive(true);
        tuidodungpanel.SetActive(false);
        kynangpanel.SetActive(false);
        Chisopanel.SetActive(true);
        skillPointPanel.SetActive(false);
    }
    public void TiemNang()
    {
        skillPointPanel.SetActive(true);
        trangbipanel.SetActive(true);
        tuidodungpanel.SetActive(false);
        kynangpanel.SetActive(false);
        Chisopanel.SetActive(false);
    }

    // ================= Map UI =================
    public void ToggleBando() => ActiveBando.SetActive(!ActiveBando.activeSelf);
    public void EndbanDo() => ActiveBando.SetActive(false);

    public void toggleHelp() => help.SetActive(!help.activeSelf);
    public void EndHelp() => help.SetActive(false);

    public void ToggleGoHome()
    {
        gohome.SetActive(!gohome.activeSelf);
    }
    public void ToggleYes()
    {
        SpawnManager.nextSpawnPoint = spawnPointName;
        SceneManager.LoadScene("ThiTran");
        gohome.SetActive(false);
        Time.timeScale = 1f;
    }
    public void ToggleNo()
    {
        gohome.SetActive(!gohome.activeSelf);
        Time.timeScale = gohome.activeSelf ? 0 : 1;
    }

    public void Mapboss1() => LoadSceneWithTimeScale("Map2");
    public void Mapboss2() => LoadSceneWithTimeScale("Map1");

    private void LoadSceneWithTimeScale(string sceneName)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }

    // ================= Coin Maps =================
    private void OpenCoinPanel(GameObject coinPanel)
    {
        ActiveBando.SetActive(false);
        coinPanel.SetActive(true);
    }

    private void CloseCoinPanel(GameObject coinPanel)
    {
        coinPanel.SetActive(false);
        ActiveBando.SetActive(false);
    }

    private void TryEnterMap(GameObject coinPanel, int requiredLevel, string sceneName, string spawnPointName)
    {
        if (CoinManager.Instance != null && LevelSystem.Instance != null)
        {
            if (CoinManager.Instance.coinCount >= 500 && LevelSystem.Instance.level >= requiredLevel)
            {
                CoinManager.Instance.AddCoin(-500);
                //  StartCoroutine(LevelSystem.Instance.Dieukien());
                GoToMap(sceneName, spawnPointName);

            }
            else
            {
                //  StartCoroutine(LevelSystem.Instance.khongduDieukien());
                Debug.Log("Không đủ tiền hoặc cấp độ vào Scene");
            }
        }
        CloseCoinPanel(coinPanel);
    }


    // Coin1
    public void ToggleCoin1() => OpenCoinPanel(ActiveCoins1);
    public void ToggleNo1() => CloseCoinPanel(ActiveCoins1);
    public void ToggleYes1() => TryEnterMap(ActiveCoins1, 10, "Map1", "SpawnMap1");

    // Coin2
    public void ToggleCoin2() => OpenCoinPanel(ActiveCoins2);
    public void ToggleNo2() => CloseCoinPanel(ActiveCoins2);
    public void ToggleYes2() => TryEnterMap(ActiveCoins2, 20, "Map2", "SpawnMap2");

    // Coin3
    public void ToggleCoin3() => OpenCoinPanel(ActiveCoins3);
    public void ToggleNo3() => CloseCoinPanel(ActiveCoins3);
    public void ToggleYes3() => TryEnterMap(ActiveCoins3, 25, "Map4", "SpawnMap4");

    // Coin4
    public void ToggleCoin4() => OpenCoinPanel(ActiveCoins4);
    public void ToggleNo4() => CloseCoinPanel(ActiveCoins4);
    public void ToggleYes4() => TryEnterMap(ActiveCoins4, 30, "Map5", "SpawnMap5");

    // Coin5
    public void ToggleCoin5() => OpenCoinPanel(ActiveCoins5);
    public void ToggleNo5() => CloseCoinPanel(ActiveCoins5);
    public void ToggleYes5() => TryEnterMap(ActiveCoins5, 35, "Map6", "SpawnMap6");

    // Coin6
    public void ToggleCoin6() => OpenCoinPanel(ActiveCoins6);
    public void ToggleNo6() => CloseCoinPanel(ActiveCoins6);
    public void ToggleYes6() => TryEnterMap(ActiveCoins6, 40, "Map7", "SpawnMap7");

    // Coin7
    public void ToggleCoin7() => OpenCoinPanel(ActiveCoins7);
    public void ToggleNo7() => CloseCoinPanel(ActiveCoins7);
    public void ToggleYes7() => TryEnterMap(ActiveCoins7, 40, "Map8", "SpawnMap8");

    // ================= UI Buttons =================
    public void AnUI()
    {
        Buttonbando.SetActive(false);
        ButtonHelp.SetActive(false);
        Buttongohome.SetActive(false);
        Buttonthanhtich.SetActive(false);
        anUI.SetActive(false);
        hienUI.SetActive(true);
    }

    public void HienUI()
    {
        Buttonbando.SetActive(true);
        ButtonHelp.SetActive(true);
        Buttongohome.SetActive(true);
        Buttonthanhtich.SetActive(true);
        hienUI.SetActive(false);
        anUI.SetActive(true);
    }

    // ================= Thành tích =================
    public void ToggleThanhTich() => Bangthanhtich.SetActive(!Bangthanhtich.activeSelf);
    public void endThanhTich() => Bangthanhtich.SetActive(false);
    //Hàm dùng chung để load map + set spawn
    public void GoToMap(string sceneName, string spawnPointName)
    {
        SpawnManager.nextSpawnPoint = spawnPointName; // Lưu vị trí spawn
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }

}
   