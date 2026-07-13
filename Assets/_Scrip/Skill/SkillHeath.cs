using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SkillHeath : MonoBehaviour
{
    public GameObject heath;
    public Button ButtonHeath;
    public Image cooldownImage;
    public Text cooldownText;

    private bool isOnCooldown = false;
    public float skillDuration = 10f;    
    public float cooldownTime = 10f; 
    public int mpCost = 10; // MP tiêu hao

    [Header("Vị trí skill")]
    [SerializeField] public Vector3 skillPositionOffset = new Vector3(0, 0f, 0); 

    public HealthSystem healthSystem;
    public bool ischeck = false;

    void Start()
    {
        if (heath != null)
            heath.SetActive(false);

        if (cooldownImage != null)
            cooldownImage.fillAmount = 0f;

        if (cooldownText != null)
            cooldownText.text = "";

        if (ButtonHeath != null)
            ButtonHeath.onClick.AddListener(UseSkill);
    }

    void UseSkill()
    {
        if (!isOnCooldown)
        {
            // Kiểm tra đủ MP không
            if (healthSystem != null && healthSystem.currentMP < mpCost)
            {
                if (GameManager.Instance != null)
                    GameManager.Instance.ShowNotEnoughMana();
                else
                    Debug.Log("Not enough mana.");
                return;
            }

            // Trừ MP
            if (healthSystem != null)
            {
                healthSystem.UseMP(mpCost);
                // Hiển thị text mana tiêu hao
                if (FloatingTextManager.Instance != null)
                    FloatingTextManager.Instance.ShowMana(-mpCost);
            }

            StartCoroutine(Cooldown());        
            StartCoroutine(ActivateSkill());  
        }
    }

    IEnumerator ActivateSkill()
    {
        ischeck = false;

        // Đặt vị trí skill theo offset
        if (heath != null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
                heath.transform.position = player.transform.position + skillPositionOffset;
            
            heath.SetActive(true);
        }

        for (int i = 0; i < (int)skillDuration; i++)
        {
            if (healthSystem.currentHP < healthSystem.maxHP && healthSystem.check == false)
            {
                healthSystem.Heal(5);
                // Hiển text hồi HP
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
            
            if (cooldownText != null)
                cooldownText.text = Mathf.Ceil(cooldown).ToString();

            yield return null;
        }

        if (cooldownImage != null)
            cooldownImage.fillAmount = 0f;

        if (cooldownText != null)
            cooldownText.text = "";

        ButtonHeath.interactable = true;
        isOnCooldown = false;
    }
}
