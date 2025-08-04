using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class ActiveUI : MonoBehaviour
{
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

    public GameObject anUI;
    public GameObject hienUI;

    public GameObject Buttonbando;
    public GameObject ButtonHelp;
    public GameObject Buttongohome;
    public GameObject Buttonthanhtich;

    public GameObject Bangthanhtich;
    public GameObject buttonthanhtich1;
    
    //character Ui
    public GameObject SkilCharacterUI;
    public string TagerNameScene;
    public LevelSystem levelSystem;
    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ToggleCharacterUI()
    {
        SkilCharacterUI.SetActive(!SkilCharacterUI.activeSelf);
    }

    public void ToggleBando()
    {
        ActiveBando.SetActive(!ActiveBando.activeSelf);
    }

    public void EndbanDo()
    {
        ActiveBando.SetActive(false);
    }
    public void toggleHelp()
    {
        help.SetActive(!help.activeSelf);
    }

    public void EndHelp()
    {
        help.SetActive(false);
    }
    public void ToggleGoHome()
    {
        gohome.SetActive(!gohome.activeSelf);
        Time.timeScale = gohome.activeSelf ? 0 : 1;
    }

    public void ToggleYes()
    {
        SceneManager.LoadScene("ThiTran");
        Time.timeScale = 1f;
    }

    public void ToggleNo()
    {
        gohome.SetActive(!gohome.activeSelf);
        Time.timeScale = gohome.activeSelf ? 0 : 1;
    }

    public void Mapboss1()
    {
        
        SceneManager.LoadScene("Map2");
        Time.timeScale = 1f;

    }
    public void Mapboss2()
    {
        SceneManager.LoadScene("Map1");
        Time.timeScale = 1f;

    }
     //ToggleMApBoss1
    public void ToggleCoin1()
    {
        ActiveBando.SetActive(!ActiveBando.activeSelf);
        ActiveCoins1.SetActive(true);
        
    }

    public void ToggleNo1()
    {
        ActiveCoins1.SetActive(false);
        ActiveBando.SetActive(true);
    }

    public void ToggleYes1()
    {
        if (CoinManager.Instance != null && LevelSystem.Instance != null && SceneLoader.Instance != null) // check tiền
        {
            if (CoinManager.Instance.coinCount >= 500 && LevelSystem.Instance.level >= 10)
            {
                CoinManager.Instance.AddCoin(-500);
                StartCoroutine(levelSystem.Dieukien());
                // Time.timeScale = 1;
                SceneLoader.Instance.LoadScene("Map1");

            }
            else
            {
                StartCoroutine(levelSystem.khongduDieukien());
                Debug.Log("Không đủ tiền vào Scene");
                
            }
        }
    }
    //ToggleMApBoss2
    public void ToggleCoin2()
    {
        ActiveBando.SetActive(!ActiveBando.activeSelf);
        ActiveCoins2.SetActive(true);
        
    }

    public void ToggleNo2()
    {
        ActiveCoins2.SetActive(false);
        ActiveBando.SetActive(true);
    }

    public void ToggleYes2()
    {
        if (CoinManager.Instance != null && LevelSystem.Instance != null && SceneLoader.Instance != null) // check tiền
        {
            if (CoinManager.Instance.coinCount >= 500 && LevelSystem.Instance.level >= 20)
            {
                CoinManager.Instance.AddCoin(-500); 
                StartCoroutine(levelSystem.Dieukien());
                Time.timeScale = 1;
            }
            else
            {
                StartCoroutine(levelSystem.khongduDieukien());
                Debug.Log("Không đủ tiền vào Scene");
                
            }
        }
    }
    //ToggleMApBoss3
    public void ToggleCoin3()
    {
        ActiveBando.SetActive(!ActiveBando.activeSelf);
        ActiveCoins3.SetActive(true);
        
    }

    public void ToggleNo3()
    {
        ActiveCoins3.SetActive(false);
        ActiveBando.SetActive(true);
    }

    public void ToggleYes3()
    {
        if (CoinManager.Instance != null && LevelSystem.Instance != null && SceneLoader.Instance != null) // check tiền
        {
            if (CoinManager.Instance.coinCount >= 500 && LevelSystem.Instance.level >= 25)
            {
                CoinManager.Instance.AddCoin(-500); 
                StartCoroutine(levelSystem.Dieukien());
                Time.timeScale = 1;
            }
            else
            {
                StartCoroutine(levelSystem.khongduDieukien());
                Debug.Log("Không đủ tiền vào Scene");
                
            }
        }
    }
    //ToggleMApBoss4
    public void ToggleCoin4()
    {
        ActiveBando.SetActive(!ActiveBando.activeSelf);
        ActiveCoins4.SetActive(true);
        
    }

    public void ToggleNo4()
    {
        ActiveCoins4.SetActive(false);
        ActiveBando.SetActive(true);
    }

    public void ToggleYes4()
    {
        if (CoinManager.Instance != null && LevelSystem.Instance != null && SceneLoader.Instance != null) // check tiền
        {
            if (CoinManager.Instance.coinCount >= 500 && LevelSystem.Instance.level >= 30)
            {
                CoinManager.Instance.AddCoin(-500); 
                StartCoroutine(levelSystem.Dieukien());

                Time.timeScale = 1;
            }
            else
            {
                StartCoroutine(levelSystem.khongduDieukien());
                Debug.Log("Không đủ tiền vào Scene");
                
            }
        }
    }
    //ToggleMApBoss5
    public void ToggleCoin5()
    {
        ActiveBando.SetActive(!ActiveBando.activeSelf);
        ActiveCoins5.SetActive(true);
        
    }

    public void ToggleNo5()
    {
        ActiveCoins5.SetActive(false);
        ActiveBando.SetActive(true);
    }

    public void ToggleYes5()
    {
        if (CoinManager.Instance != null && LevelSystem.Instance != null && SceneLoader.Instance != null) // check tiền
        {
            if (CoinManager.Instance.coinCount >= 500 && LevelSystem.Instance.level >= 35)
            {
                CoinManager.Instance.AddCoin(-500); 
                StartCoroutine(levelSystem.Dieukien());
                Time.timeScale = 1;
            }
            else
            {
                StartCoroutine(levelSystem.khongduDieukien());
                Debug.Log("Không đủ tiền vào Scene");
                
            }
        }
    }
    //ToggleMApBoss6
    public void ToggleCoin6()
    {
        ActiveBando.SetActive(!ActiveBando.activeSelf);
        ActiveCoins6.SetActive(true);
        
    }

    public void ToggleNo6()
    {
        ActiveCoins6.SetActive(false);
        ActiveBando.SetActive(true);
    }

    public void ToggleYes6()
    {
        if (CoinManager.Instance != null && LevelSystem.Instance != null && SceneLoader.Instance != null) // check tiền
        {
            if (CoinManager.Instance.coinCount >= 500 && LevelSystem.Instance.level >= 40)
            {
                CoinManager.Instance.AddCoin(-500); 
                StartCoroutine(levelSystem.Dieukien());
                Time.timeScale = 1;
            }
            else
            {
                StartCoroutine(levelSystem.khongduDieukien());
                Debug.Log("Không đủ tiền vào Scene");
                
            }
        }
    }
    //ToggleMApBoss7
    public void ToggleCoin7()
    {
        ActiveBando.SetActive(!ActiveBando.activeSelf);
        ActiveCoins7.SetActive(true);
        
    }

    public void ToggleNo7()
    {
        ActiveCoins7.SetActive(false);
        ActiveBando.SetActive(true);
    }

    public void ToggleYes7()
    {
        if (CoinManager.Instance != null && LevelSystem.Instance != null && SceneLoader.Instance != null) // check tiền
        {
            if (CoinManager.Instance.coinCount >= 500 && LevelSystem.Instance.level >= 40)
            {
                CoinManager.Instance.AddCoin(-500); 
                StartCoroutine(levelSystem.Dieukien());
                Time.timeScale = 1;
            }
            else
            {
                StartCoroutine(levelSystem.khongduDieukien());
                Debug.Log("Không đủ tiền vào Scene");
                
            }
        }
    }
   //bat tắt button ui game
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
    //bảng thành tích
    public void ToggleThanhTich()
    {
        Bangthanhtich.SetActive(!Bangthanhtich.activeSelf);
    }

    public void endThanhTich()
    {
        Bangthanhtich.SetActive(false);
    }
    //butotn thanh tich khi lv = 10
    
}
