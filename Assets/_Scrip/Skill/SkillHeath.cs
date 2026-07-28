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
    private static readonly WaitForSeconds HealTickDelay = new WaitForSeconds(1f);

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

    private void OnDestroy()
    {
        if (ButtonHeath != null)
            ButtonHeath.onClick.RemoveListener(UseSkill);
    }

    void UseSkill()
    {
        if (!isOnCooldown)
        {
            if (healthSystem == null)
                healthSystem = HealthSystem.Instance;
            if (healthSystem == null)
            {
                Debug.LogWarning("Heal skill requires a HealthSystem.");
                return;
            }

            if (healthSystem.isDead)
                return;

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
            if (healthSystem != null)
                heath.transform.position = healthSystem.transform.position + skillPositionOffset;
            
            heath.SetActive(true);
        }

        for (int i = 0; i < (int)skillDuration; i++)
        {
            if (healthSystem.currentHP < healthSystem.maxHP && healthSystem.check == false)
            {
                healthSystem.Heal(5);
#if UNITY_EDITOR
                Debug.Log("Hồi 1 máu. HP hiện tại: " + healthSystem.currentHP);
#endif
            }
            yield return HealTickDelay;
        }

        heath.SetActive(false);
        ischeck = true;
    }


    IEnumerator Cooldown()
    {
        isOnCooldown = true;
        if (ButtonHeath != null)
            ButtonHeath.interactable = false;

        float cooldown = cooldownTime;
        int lastDisplayedSeconds = -1;
        while (cooldown > 0)
        {
            cooldown -= Time.deltaTime;
            if (cooldownImage != null)
                cooldownImage.fillAmount = cooldown / cooldownTime;
            
            int displayedSeconds = Mathf.Max(0, Mathf.CeilToInt(cooldown));
            if (cooldownText != null && displayedSeconds != lastDisplayedSeconds)
            {
                cooldownText.text = displayedSeconds > 0 ? displayedSeconds.ToString() : "";
                lastDisplayedSeconds = displayedSeconds;
            }

            yield return null;
        }

        if (cooldownImage != null)
            cooldownImage.fillAmount = 0f;

        if (cooldownText != null)
            cooldownText.text = "";

        if (ButtonHeath != null)
            ButtonHeath.interactable = true;
        isOnCooldown = false;
    }
}
