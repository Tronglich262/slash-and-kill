using System.Collections;
using UnityEngine;

public class ActiveSkillCharacterui : MonoBehaviour
{
    private static readonly WaitForSeconds NotificationDuration = new WaitForSeconds(0.5f);
    private Coroutine notificationRoutine;
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
        if (levelSystem == null)
            levelSystem = LevelSystem.Instance;

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
            ShowNotification(dudieukien);
            skill2.SetActive(true);
            PlayerPrefs.SetInt("Skill2_Active", 1); // Lưu trạng thái
        }
        else
        {
            ShowNotification(khongdudieukien);
        }
    }

    public void ToggleSkill3()
    {
        if (levelSystem.level >= 10)
        {
            ShowNotification(dudieukien);
            skill3.SetActive(true);
            PlayerPrefs.SetInt("Skill3_Active", 1);
        }
        else
        {
            ShowNotification(khongdudieukien);
        }
    }

    public void ToggleSkill4()
    {
        if (levelSystem.level >= 15)
        {
            ShowNotification(dudieukien);
            skill4.SetActive(true);
            PlayerPrefs.SetInt("Skill4_Active", 1);
        }
        else
        {
            ShowNotification(khongdudieukien);
        }
    }

    public void ToggleSkill5()
    {
        if (levelSystem.level >= 10)
        {
            ShowNotification(dudieukien);
            skill5.SetActive(true);
            PlayerPrefs.SetInt("Skill5_Active", 1);
        }
        else
        {
            ShowNotification(khongdudieukien);
        }
    }
    public void ToggleSkill6()
    {
        if (levelSystem.level >= 1)
        {
            ShowNotification(dudieukien);
            skill6.SetActive(true);
            PlayerPrefs.SetInt("Skill6_Active", 1);
        }
        else
        {
            ShowNotification(khongdudieukien);
        }
    }

    private void ShowNotification(GameObject notification)
    {
        if (notificationRoutine != null)
            StopCoroutine(notificationRoutine);

        if (dudieukien != null)
            dudieukien.SetActive(false);
        if (khongdudieukien != null)
            khongdudieukien.SetActive(false);

        notificationRoutine = StartCoroutine(ShowNotificationRoutine(notification));
    }

    private IEnumerator ShowNotificationRoutine(GameObject notification)
    {
        if (notification == null)
        {
            notificationRoutine = null;
            yield break;
        }

        notification.SetActive(true);
        yield return NotificationDuration;
        notification.SetActive(false);
        notificationRoutine = null;
    }

}
