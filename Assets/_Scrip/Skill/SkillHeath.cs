using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SkillHeath : MonoBehaviour
{
    public GameObject heath;
    public Button ButtonHeath;
    public Image cooldownImage;

    private bool isOnCooldown = false;
    public float skillDuration = 10f;    // Thời gian skill hoạt động
    public float cooldownTime = 10f; // Thời gian hồi chiêu

    public HealthSystem healthSystem;
    public bool ischeck = false;

    void Start()
    {
        if (heath != null)
            heath.SetActive(false);

        if (cooldownImage != null)
            cooldownImage.fillAmount = 0f;

        if (ButtonHeath != null)
            ButtonHeath.onClick.AddListener(UseSkill);
    }

    void UseSkill()
    {
        if (!isOnCooldown)
        {
            StartCoroutine(Cooldown());       // Bắt đầu hồi chiêu ngay
            StartCoroutine(ActivateSkill());  // Hiệu ứng skill diễn ra song song
        }
    }

    IEnumerator ActivateSkill()
    {
        ischeck = false;
        heath.SetActive(true);

        for (int i = 0; i < (int)skillDuration; i++)
        {
            if (healthSystem.currentHP < healthSystem.maxHP && healthSystem.check == false)
            {
                healthSystem.Heal(5);
                Debug.Log("Hồi 1 máu. HP hiện tại: " + healthSystem.currentHP);
            }
            yield return new WaitForSeconds(1f);
        }

        heath.SetActive(false);
        ischeck = true;
    }


    IEnumerator Cooldown()
    {
        isOnCooldown = true;
        ButtonHeath.interactable = false;

        float cooldown = cooldownTime;
        while (cooldown > 0)
        {
            cooldown -= Time.deltaTime;
            if (cooldownImage != null)
                cooldownImage.fillAmount = cooldown / cooldownTime;

            yield return null;
        }

        if (cooldownImage != null)
            cooldownImage.fillAmount = 0f;

        ButtonHeath.interactable = true;
        isOnCooldown = false;
    }
}