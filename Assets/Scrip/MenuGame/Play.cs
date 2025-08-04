using JetBrains.Annotations;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
public class Play : MonoBehaviour
{
    public GameObject Setting;
    public GameObject menuGame;
    public GameObject dichuyen, tancong , dpg;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public void PlayGame()
    {
        SceneManager.LoadScene("ThiTran");
    }
    public void SettingMenu()
    {
        Setting.SetActive(true);
        menuGame.SetActive(!menuGame.activeSelf);
        dpg.SetActive(true);
        dichuyen.SetActive(false);
        tancong.SetActive(false);
    }
    public void CloseMenu()
    {
        Setting.SetActive(!Setting.activeSelf);
        menuGame.SetActive(!menuGame.activeSelf);
    }
    public void Phim()
    {
        dichuyen.SetActive(true);
        tancong.SetActive(true);
        dpg.SetActive(false);
    }
    public void Catdat()
    {
        dichuyen.SetActive(false);
        tancong.SetActive(false);
        dpg.SetActive(true);

    }
    public void SetResolution1280x720()
    {
        Screen.SetResolution(1280, 720, FullScreenMode.Windowed);
    }

    public void SetResolution1920x1080()
    {
        Screen.SetResolution(1920, 1080, FullScreenMode.FullScreenWindow);
    }

    public void SetResolution1600x900()
    {
        Screen.SetResolution(1600, 900, FullScreenMode.Windowed);
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
