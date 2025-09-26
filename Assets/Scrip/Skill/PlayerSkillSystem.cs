using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSkillSystem : MonoBehaviour
{
    public enum SkillType { Skill1, Skill2, Skill3, Skill4, Dash }

    [System.Serializable]
    public class SkillData
    {
        public SkillType skillType;

        // Thường dùng cho attack skill
        public GameObject skillPrefab;
        public float offsetX = 1f;
        public float spawnY = 0.5f;
        public float skillDuration = 2f;
        public bool followPlayer = false;

        // UI
        public Button skillButton;
        public Image cooldownBar;
        public float cooldownTime = 5f;

        // Dành cho Dash
        public float dashDistance = 4f;
        public float dashDuration = 0.2f;

        // Dành cho Heal
        public GameObject healEffect;
        public float healDuration = 10f;
        public HealthSystem healthSystem;
    }

    public Transform player;
    private Animator playerAnimator;
    private Rigidbody2D rb;
    private bool[] isOnCooldown;

    void Start()
    {
        if (player != null)
        {
            playerAnimator = player.GetComponent<Animator>();
            rb = player.GetComponent<Rigidbody2D>();
        }

        isOnCooldown = new bool[6]; // 6 skill

        for (int i = 0; i < 6; i++)
        {
            int index = i;
            if (i < skills.Length && skills[i].skillButton != null)
                skills[i].skillButton.onClick.AddListener(() => UseSkill(index));
            if (i < skills.Length && skills[i].cooldownBar != null)
                skills[i].cooldownBar.fillAmount = 0;
            if (i < skills.Length && skills[i].healEffect != null)
                skills[i].healEffect.SetActive(false);
        }
    }

    public SkillData[] skills;

    void Update()
    {
        for (int i = 0; i < skills.Length; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i) && !isOnCooldown[i])
            {
                UseSkill(i);
            }
        }

        // Dash hỗ trợ phím E
        for (int i = 0; i < skills.Length; i++)
        {
            if (skills[i].skillType == SkillType.Dash && Input.GetKeyDown(KeyCode.E) && !isOnCooldown[i])
            {
                UseSkill(i);
            }
        }
    }

    void UseSkill(int index)
    {
        SkillData skill = skills[index];
        if (isOnCooldown[index] || player == null) return;

        isOnCooldown[index] = true;
        if (skill.skillButton != null)
            skill.skillButton.interactable = false;
        if (skill.cooldownBar != null)
            skill.cooldownBar.fillAmount = 1;

        switch (skill.skillType)
        {
            case SkillType.Skill1:
            case SkillType.Skill2:
            case SkillType.Skill3:
            case SkillType.Skill4:
                if (playerAnimator != null)
                {
                    playerAnimator.SetBool("skill", true);
                    StartCoroutine(ResetAttackSkill());
                }
                StartCoroutine(SpawnSkill(skill));
                break;

            case SkillType.Dash:
                if (playerAnimator != null)
                    playerAnimator.SetBool("Dash", true);
                StartCoroutine(Dash(skill, index));
                break;

           
        }

        StartCoroutine(StartCooldown(skill, index));
    }

    #region Attack Skills
    IEnumerator SpawnSkill(SkillData skill)
    {
        GameObject spawnedSkill = null;
        float elapsed = 0f;

        while (elapsed < skill.skillDuration)
        {
            float direction = Mathf.Sign(player.localScale.x);
            Vector3 spawnPos = player.position + new Vector3(direction * skill.offsetX, skill.spawnY, 0);

            if (spawnedSkill == null)
            {
                spawnedSkill = Instantiate(skill.skillPrefab, spawnPos, Quaternion.identity);
                Vector3 scale = spawnedSkill.transform.localScale;
                scale.x = Mathf.Abs(scale.x) * direction;
                spawnedSkill.transform.localScale = scale;
            }

            if (skill.followPlayer && spawnedSkill != null)
                spawnedSkill.transform.position = spawnPos;

            elapsed += Time.deltaTime;
            yield return null;
        }

        if (spawnedSkill != null)
            Destroy(spawnedSkill);
    }

    IEnumerator ResetAttackSkill()
    {
        yield return new WaitForSeconds(0.5f);
        if (playerAnimator != null)
            playerAnimator.SetBool("skill", false);
    }
    #endregion

    #region Dash Skill
    IEnumerator Dash(SkillData skill, int index)
    {
        float dashDirection = player.localScale.x >= 0 ? 1f : -1f;
        Vector2 startPos = rb.position;
        Vector2 targetPos = startPos + new Vector2(skill.dashDistance * dashDirection, 0);

        float elapsed = 0f;
        while (elapsed < skill.dashDuration)
        {
            rb.MovePosition(Vector2.Lerp(startPos, targetPos, elapsed / skill.dashDuration));
            elapsed += Time.deltaTime;
            yield return null;
        }
        rb.MovePosition(targetPos);

        if (playerAnimator != null)
            playerAnimator.SetBool("Dash", false);
    }
    #endregion

    

    IEnumerator StartCooldown(SkillData skill, int index)
    {
        float elapsed = 0f;
        while (elapsed < skill.cooldownTime)
        {
            elapsed += Time.deltaTime;
            if (skill.cooldownBar != null)
                skill.cooldownBar.fillAmount = 1 - (elapsed / skill.cooldownTime);
            yield return null;
        }

        if (skill.cooldownBar != null)
            skill.cooldownBar.fillAmount = 0;
        if (skill.skillButton != null)
            skill.skillButton.interactable = true;

        isOnCooldown[index] = false;
    }
}
