using System.Collections;
using UnityEngine;

public class ActiveSkillCharacterui : MonoBehaviour
{
    public LevelSystem levelSystem;
    public GameObject skill2;
    public GameObject skill3;
    public GameObject skill4;
    public GameObject skill5;
    public GameObject skill6;

    // Thông báo
    public GameObject dudieukien;
    public GameObject khongdudieukien;

    void Start()
    {
        // Load trạng thái skill đã mở
        if (PlayerPrefs.GetInt("Skill2_Active", 0) == 1)
            skill2.SetActive(true);

        if (PlayerPrefs.GetInt("Skill3_Active", 0) == 1)
            skill3.SetActive(true);

        if (PlayerPrefs.GetInt("Skill4_Active", 0) == 1)
            skill4.SetActive(true);

        if (PlayerPrefs.GetInt("Skill5_Active", 0) == 1)
            skill5.SetActive(true);
        if (PlayerPrefs.GetInt("Skill6_Active", 0) == 1)
            skill6.SetActive(true);
    }

    public void ToggleSkill2()
    {
        if (levelSystem.level >= 5)
        {
            StartCoroutine(Dieukien());
            skill2.SetActive(true);
            PlayerPrefs.SetInt("Skill2_Active", 1); // Lưu trạng thái
        }
        else
        {
            StartCoroutine(khongduDieukien());
            Debug.Log("Không đủ điều kiện");
        }
    }

    public void ToggleSkill3()
    {
        if (levelSystem.level >= 10)
        {
            StartCoroutine(Dieukien());
            skill3.SetActive(true);
            PlayerPrefs.SetInt("Skill3_Active", 1);
        }
        else
        {
            StartCoroutine(khongduDieukien());
            Debug.Log("Không đủ điều kiện");
        }
    }

    public void ToggleSkill4()
    {
        if (levelSystem.level >= 15)
        {
            StartCoroutine(Dieukien());
            skill4.SetActive(true);
            PlayerPrefs.SetInt("Skill4_Active", 1);
        }
        else
        {
            StartCoroutine(khongduDieukien());
            Debug.Log("Không đủ điều kiện");
        }
    }

    public void ToggleSkill5()
    {
        if (levelSystem.level >= 10)
        {
            StartCoroutine(Dieukien());
            skill5.SetActive(true);
            PlayerPrefs.SetInt("Skill5_Active", 1);
        }
        else
        {
            StartCoroutine(khongduDieukien());
            Debug.Log("Không đủ điều kiện");
        }
    }
    public void ToggleSkill6()
    {
        if (levelSystem.level >= 1)
        {
            StartCoroutine(Dieukien());
            skill6.SetActive(true);
            PlayerPrefs.SetInt("Skill6_Active", 1);
        }
        else
        {
            StartCoroutine(khongduDieukien());
            Debug.Log("Không đủ điều kiện");
        }
    }

    IEnumerator Dieukien()
    {
        dudieukien.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        dudieukien.SetActive(false);
    }

    IEnumerator khongduDieukien()
    {
        khongdudieukien.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        khongdudieukien.SetActive(false);
    }

    // Nếu muốn reset khi thoát game (tùy chọn)
    void OnApplicationQuit()
    {
        PlayerPrefs.DeleteKey("Skill2_Active");
        PlayerPrefs.DeleteKey("Skill3_Active");
        PlayerPrefs.DeleteKey("Skill4_Active");
        PlayerPrefs.DeleteKey("Skill5_Active");
    }
}
