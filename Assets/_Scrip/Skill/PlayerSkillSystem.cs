using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class PlayerSkillSystem : MonoBehaviour
{
    private static readonly WaitForSeconds AttackAnimationDuration = new WaitForSeconds(0.5f);
    private static readonly List<PlayerSkillSystem> Instances = new List<PlayerSkillSystem>();
    private static PlayerSkillSystem inputOwner;
    private static readonly Dictionary<GameObject, Stack<GameObject>> EffectPools =
        new Dictionary<GameObject, Stack<GameObject>>();
    private static readonly Dictionary<GameObject, ParticleSystem[]> EffectParticles =
        new Dictionary<GameObject, ParticleSystem[]>();
    private static readonly Dictionary<GameObject, Animator> EffectAnimators =
        new Dictionary<GameObject, Animator>();
    private static readonly List<GameObject> DeadEffectKeys = new List<GameObject>();
    private static readonly List<GameObject> LivePooledEffects = new List<GameObject>();

    public enum SkillType { Skill1, Skill2, Skill3, Skill4, Dash }

    [System.Serializable]
    public class SkillData
    {
        public SkillType skillType;

        // Thường dùng cho attack skill
       [SerializeField] public GameObject skillPrefab;
        [SerializeField] public float offsetX = 1f;
        [SerializeField] public float spawnY = 0.5f;
        [SerializeField] public float skillDuration = 2f;
        [SerializeField] public bool followPlayer = false;

        // MP Cost
        [SerializeField] public int mpCost = 0;

        // UI
        [SerializeField] public Button skillButton;
        [SerializeField] public Image cooldownBar;
        [SerializeField] public Text cooldownText;
        [SerializeField] public float cooldownTime = 5f;

        // Dành cho Dash
        [SerializeField] public float dashDistance = 4f;
        [SerializeField] public float dashDuration = 0.2f;

        // Dành cho Heal
        [SerializeField] public GameObject healEffect;
        [SerializeField] public float healDuration = 10f;
        [SerializeField] public HealthSystem healthSystem;
    }

    public Transform player;
    private Animator playerAnimator;
    private Rigidbody2D rb;
    private HealthSystem playerHealth;
    private bool[] isOnCooldown;
    private UnityAction[] skillButtonActions;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticState()
    {
        Instances.Clear();
        inputOwner = null;
        EffectPools.Clear();
        EffectParticles.Clear();
        EffectAnimators.Clear();
        DeadEffectKeys.Clear();
        LivePooledEffects.Clear();

        SceneManager.sceneUnloaded -= OnSceneUnloaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private void OnEnable()
    {
        if (!Instances.Contains(this))
            Instances.Add(this);

        if (inputOwner == null)
            inputOwner = this;
    }

    private void OnDisable()
    {
        Instances.Remove(this);

        if (inputOwner == this)
            inputOwner = Instances.Count > 0 ? Instances[0] : null;
    }

    void Start()
    {
        if (player != null)
        {
            playerAnimator = player.GetComponent<Animator>();
            rb = player.GetComponent<Rigidbody2D>();
            playerHealth = player.GetComponent<HealthSystem>();
        }

        isOnCooldown = new bool[skills.Length];
        skillButtonActions = new UnityAction[skills.Length];

        for (int i = 0; i < skills.Length; i++)
        {
            int index = i;
            if (skills[i].skillButton != null)
            {
                UnityAction action = () => UseSkill(index);
                skillButtonActions[i] = action;
                skills[i].skillButton.onClick.AddListener(action);
            }
            if (skills[i].cooldownBar != null)
                skills[i].cooldownBar.fillAmount = 0;
            if (skills[i].healEffect != null)
                skills[i].healEffect.SetActive(false);
        }
    }

    public SkillData[] skills;

    void Update()
    {
        if (inputOwner != this)
            return;

        for (int i = 0; i < Instances.Count; i++)
            Instances[i].ProcessKeyboardInput();
    }

    private void ProcessKeyboardInput()
    {
        if (isOnCooldown == null)
            return;

        for (int i = 0; i < skills.Length; i++)
        {
            KeyCode hotkey = GetHotkey(skills[i].skillType);
            if (hotkey != KeyCode.None && Input.GetKeyDown(hotkey) && !isOnCooldown[i])
                UseSkill(i);
        }
    }

    private void OnDestroy()
    {
        if (skills == null || skillButtonActions == null)
            return;

        int count = Mathf.Min(skills.Length, skillButtonActions.Length);
        for (int i = 0; i < count; i++)
        {
            if (skills[i].skillButton != null && skillButtonActions[i] != null)
                skills[i].skillButton.onClick.RemoveListener(skillButtonActions[i]);
        }
    }

    private static KeyCode GetHotkey(SkillType skillType)
    {
        switch (skillType)
        {
            case SkillType.Skill1: return KeyCode.Alpha1;
            case SkillType.Skill2: return KeyCode.Alpha2;
            case SkillType.Skill3: return KeyCode.Alpha3;
            case SkillType.Skill4: return KeyCode.Alpha4;
            case SkillType.Dash: return KeyCode.E;
            default: return KeyCode.None;
        }
    }

    void UseSkill(int index)
    {
        if (index < 0 || index >= skills.Length)
            return;

        if (playerHealth != null && playerHealth.isDead)
            return;

        SkillData skill = skills[index];
        if (isOnCooldown[index] || player == null) return;

        if (skill.skillType != SkillType.Dash && skill.skillPrefab == null)
        {
            Debug.LogWarning($"Skill '{skill.skillType}' has no prefab assigned.");
            return;
        }

        if (skill.skillType == SkillType.Dash && rb == null)
        {
            Debug.LogWarning("Dash requires a Rigidbody2D on the player.");
            return;
        }

        // Kiểm tra đủ MP không
        if (skill.mpCost > 0)
        {
            if (playerHealth == null || playerHealth.currentMP < skill.mpCost)
            {
                if (GameManager.Instance != null)
                    GameManager.Instance.ShowNotEnoughMana();
                else
                    Debug.LogWarning("Not enough mana and no GameManager is available.");
                return;
            }
            // Trừ MP
            playerHealth.UseMP(skill.mpCost);
        }

        isOnCooldown[index] = true;
        if (skill.skillButton != null)
            skill.skillButton.interactable = false;
        if (skill.cooldownBar != null)
            skill.cooldownBar.fillAmount = 1;
        if (skill.cooldownText != null)
            skill.cooldownText.text = Mathf.Ceil(skill.cooldownTime).ToString();

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
                {
                    playerAnimator.ResetTrigger("Dash");
                    playerAnimator.SetTrigger("Dash");
                }
                StartCoroutine(Dash(skill));
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
                spawnedSkill = SpawnEffect(skill.skillPrefab, spawnPos);
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
            ReleaseEffect(skill.skillPrefab, spawnedSkill);
    }

    private static GameObject SpawnEffect(GameObject prefab, Vector3 position)
    {
        if (!EffectPools.TryGetValue(prefab, out Stack<GameObject> pool))
        {
            pool = new Stack<GameObject>();
            EffectPools.Add(prefab, pool);
        }

        GameObject effect = null;
        while (pool.Count > 0)
        {
            GameObject pooledEffect = pool.Pop();
            if (pooledEffect != null)
            {
                effect = pooledEffect;
                break;
            }

            // Scene changes destroy pooled GameObjects. Remove their cached
            // components as they are encountered so the static dictionaries
            // cannot grow with dead references across repeated map loads.
            EffectParticles.Remove(pooledEffect);
            EffectAnimators.Remove(pooledEffect);
        }

        if (effect == null)
            effect = Instantiate(prefab, position, Quaternion.identity);
        else
        {
            effect.transform.SetPositionAndRotation(position, Quaternion.identity);
            effect.SetActive(true);
        }

        if (!EffectAnimators.TryGetValue(effect, out Animator effectAnimator))
        {
            effectAnimator = effect.GetComponent<Animator>();
            EffectAnimators[effect] = effectAnimator;
        }

        if (effectAnimator != null)
        {
            effectAnimator.Rebind();
            effectAnimator.Update(0f);
        }

        ParticleSystem[] particles = GetEffectParticles(effect);
        for (int i = 0; i < particles.Length; i++)
        {
            particles[i].Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            particles[i].Play(true);
        }

        return effect;
    }

    private static void ReleaseEffect(GameObject prefab, GameObject effect)
    {
        if (effect == null)
            return;

        ParticleSystem[] particles = GetEffectParticles(effect);
        for (int i = 0; i < particles.Length; i++)
            particles[i].Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        effect.SetActive(false);
        if (!EffectPools.TryGetValue(prefab, out Stack<GameObject> pool))
        {
            pool = new Stack<GameObject>();
            EffectPools.Add(prefab, pool);
        }

        pool.Push(effect);
    }

    private static ParticleSystem[] GetEffectParticles(GameObject effect)
    {
        if (!EffectParticles.TryGetValue(effect, out ParticleSystem[] particles))
        {
            particles = effect.GetComponentsInChildren<ParticleSystem>(true);
            EffectParticles.Add(effect, particles);
        }

        return particles;
    }

    private static void OnSceneUnloaded(Scene unloadedScene)
    {
        // Keep live pooled effects from other additive scenes, but discard
        // objects Unity destroyed with the unloaded scene.
        foreach (Stack<GameObject> pool in EffectPools.Values)
        {
            LivePooledEffects.Clear();
            while (pool.Count > 0)
            {
                GameObject effect = pool.Pop();
                if (effect != null)
                    LivePooledEffects.Add(effect);
            }

            for (int i = LivePooledEffects.Count - 1; i >= 0; i--)
                pool.Push(LivePooledEffects[i]);
        }
        LivePooledEffects.Clear();

        RemoveDestroyedCacheKeys(EffectParticles);
        RemoveDestroyedCacheKeys(EffectAnimators);
    }

    private static void RemoveDestroyedCacheKeys<T>(Dictionary<GameObject, T> cache)
    {
        DeadEffectKeys.Clear();
        foreach (GameObject effect in cache.Keys)
        {
            if (effect == null)
                DeadEffectKeys.Add(effect);
        }

        for (int i = 0; i < DeadEffectKeys.Count; i++)
            cache.Remove(DeadEffectKeys[i]);
        DeadEffectKeys.Clear();
    }

    IEnumerator ResetAttackSkill()
    {
        yield return AttackAnimationDuration;
        if (playerAnimator != null)
            playerAnimator.SetBool("skill", false);
    }
    #endregion

    #region Dash Skill
    IEnumerator Dash(SkillData skill)
    {
        float dashDirection = player.localScale.x >= 0 ? 1f : -1f;
        Vector2 startPos = rb.position;
        Vector2 targetPos = startPos + new Vector2(skill.dashDistance * dashDirection, 0);

        float elapsed = 0f;
        while (elapsed < skill.dashDuration)
        {
            if (playerHealth != null && playerHealth.isDead)
            {
                rb.linearVelocity = Vector2.zero;
                yield break;
            }

            rb.MovePosition(Vector2.Lerp(startPos, targetPos, elapsed / skill.dashDuration));
            elapsed += Time.deltaTime;
            yield return null;
        }
        rb.MovePosition(targetPos);

    }
    #endregion

    

    IEnumerator StartCooldown(SkillData skill, int index)
    {
        float elapsed = 0f;
        int lastDisplayedSeconds = -1;
        while (elapsed < skill.cooldownTime)
        {
            elapsed += Time.deltaTime;
            if (skill.cooldownBar != null)
                skill.cooldownBar.fillAmount = 1 - (elapsed / skill.cooldownTime);
            
            // Cập nhật text cooldown
            if (skill.cooldownText != null)
            {
                int remaining = Mathf.Max(0, Mathf.CeilToInt(skill.cooldownTime - elapsed));
                if (remaining != lastDisplayedSeconds)
                {
                    skill.cooldownText.text = remaining > 0 ? remaining.ToString() : "";
                    lastDisplayedSeconds = remaining;
                }
            }
            
            yield return null;
        }

        if (skill.cooldownBar != null)
            skill.cooldownBar.fillAmount = 0;
        if (skill.skillButton != null)
            skill.skillButton.interactable = true;
        if (skill.cooldownText != null)
            skill.cooldownText.text = "";

        isOnCooldown[index] = false;
    }
}
