using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossController : MonoBehaviour
{
    [Header("Target and Movement")]
    public Transform player;
    public float speed = 2f;
    public float attackRange = 1f;
    public float rangedAttackRange = 8f;
    public float attackCooldown = 2f;
    public int attackDamage = 10;
    [Min(0f)] public float meleeHitPadding = 0.35f;
    public float jumpBackDistance = 2f;
    public float jumpSpeed = 4f;
    [Tooltip("Use for standalone bosses that do not have a BossIntroManager trigger.")]
    public bool autoStartBattle;
    public bool IsBattleActive => battleStarted && !combatStopped;
    public int HealthResetCount => healthResetCount;
    public event System.Action ShadowRainStarted;
    public event System.Action SummonStarted;

    [Header("Teleport")]
    public Transform[] teleportPoints;
    public float teleportCooldown = 5f;
    [Tooltip("For bosses that should materialize near the player instead of using distant arena points.")]
    public bool teleportNearPlayer;
    [Min(0.5f)] public float teleportArrivalDistance = 3f;
    [Min(0.5f)] public float maxTeleportTravelDistance = 6f;
    [Min(0.5f)] public float minimumTeleportDisplacement = 2.25f;

    [Header("Projectile")]
    public GameObject attackProjectile;
    public Transform attackSpawnPoint;
    public float rangedAttackCooldown = 15f;
    public float projectileSpeed = 10f;
    [Range(1, 5)] public int rangedBurstCount = 2;
    public float projectileBurstInterval = 0.18f;
    [Range(0f, 0.6f)] public float projectilePredictionTime = 0.2f;
    [Header("Animation Action Overrides")]
    public bool useCastForRangedAttack;
    public bool useSpellForSkyAttack;
    [Tooltip("Alternates Attack with the controller's Attack-NoEffect state. Useful when damage/effects are driven by code.")]
    public bool alternateNoEffectMelee;
    [Tooltip("Flip the SpriteRenderer instead of Transform scale. Use for sprites whose hierarchy has child offsets.")]
    public bool useSpriteRendererFacing;
    public bool invertSpriteFacing;
    [Tooltip("Plays Spell-NoEffect while teleporting, then creates portals at the departure and arrival points.")]
    public bool useNoEffectSpellForTeleport;

    [Header("Void Rift Combo")]
    [Tooltip("Cast opens a damaging void rift at the predicted player position, then chains into Spell.")]
    public bool enableVoidRiftCombo;
    [Min(0.25f)] public float voidRiftRadius = 1.35f;
    [Min(0f)] public float voidRiftWarningTime = 0.42f;
    [Min(0f)] public float voidRiftDuration = 0.35f;
    [Min(0)] public int voidRiftDamageBonus = 8;
    [Tooltip("Time spent visibly channeling Spell after Cast opens the rift.")]
    [Min(0.1f)] public float voidRiftSpellChannelTime = 0.55f;
    [Tooltip("Delay before the visible Spell becomes dangerous. Once armed, contact damages immediately.")]
    [Min(0f)] public float spellDamageArmTime = 0.18f;
    [Tooltip("Height of the Spell visual above the targeted player position.")]
    [Min(0f)] public float spellEffectHeight = 1.1f;
    [Tooltip("Height of the Spell-NoEffect portal used for teleportation.")]
    [Min(0f)] public float teleportPortalHeight = 1.6f;
    [Min(0.1f)] public float teleportPortalEffectDuration = 0.95f;
    [Min(0.1f)] public float teleportArrivalRevealDelay = 0.72f;
    [Min(1)] public int voidRiftPhaseTwoCount = 2;
    [Min(0f)] public float voidRiftVolleyInterval = 0.2f;

    [Header("Shadow Step Combo")]
    public bool enableShadowStepCombo;
    [Range(0f, 1f)] public float shadowStepAfterSpellChance = 0.35f;
    [Min(0.5f)] public float shadowStepAttackDistance = 1.35f;
    [Min(0.5f)] public float shadowStepRetreatDistance = 3.5f;
    [Min(0f)] public float spellImpactShakeDuration = 0.32f;
    [Min(0f)] public float spellImpactShakeIntensity = 0.13f;
    [Tooltip("A light rumble when a void spell appears, before its stronger impact shake.")]
    [Min(0f)] public float spellCastShakeDuration = 0.14f;
    [Min(0f)] public float spellCastShakeIntensity = 0.045f;
    [Tooltip("Keeps the teleport-in slash animation on screen before the boss retreats.")]
    [Min(0.1f)] public float shadowStepAttackCommitTime = 1f;

    [Header("Reactive Defence")]
    [Tooltip("When the boss is hit in close range, it cancels its current plan and creates space.")]
    public bool enableReactiveDefence;
    [Min(0f)] public float reactiveHitPause = 0.12f;
    [Min(0.5f)] public float reactiveRetreatDistance = 3.25f;
    [Min(0.1f)] public float reactiveRetreatDuration = 0.65f;
    [Min(0.5f)] public float reactiveCounterRange = 1.25f;
    [Min(0f)] public float reactiveDefenceCooldown = 1.35f;
    [Tooltip("Chance that a defensive reaction uses Spell-NoEffect to blink away instead of running.")]
    [Range(0f, 1f)] public float reactiveTeleportChance = 0.55f;
    [Min(0.5f)] public float reactiveTeleportDistance = 4.25f;

    [Header("Shadow Rain Attack")]
    public bool enableShadowRain;
    [Min(0.5f)] public float shadowRainCooldown = 9f;
    [Range(1, 5)] public int shadowRainWaves = 3;
    [Min(0.25f)] public float shadowRainStartOffset = 2.2f;
    [Min(0.25f)] public float shadowRainLaneSpacing = 1.25f;
    [Range(0.25f, 1f)] public float shadowRainVisualScale = 0.72f;
    [Min(0.05f)] public float shadowRainWaveInterval = 0.24f;
    [Min(0.1f)] public float shadowRainHitDelay = 1.3f;
    [Min(0.25f)] public float shadowRainLaneRadius = 0.85f;

    [Header("Shadow Summon")]
    [Tooltip("Drag the Skeleton enemy prefab here. It emerges where the boss's Spell lands.")]
    public GameObject summonEnemyPrefab;
    [Min(1)] public int summonMinCount = 1;
    [Min(1)] public int summonMaxCount = 4;
    [Min(1)] public int maxActiveSummons = 6;
    [Min(0.1f)] public float summonCooldown = 12f;
    [Min(0.1f)] public float summonSpellDelay = 0.65f;
    [Min(0.1f)] public float summonInterval = 0.28f;
    [Min(0.5f)] public float summonSpread = 3.5f;
    [Range(0.25f, 1f)] public float summonSpellVisualScale = 0.72f;
    [Min(0f)] public float summonGroundOffset = 0.02f;
    [Min(0.5f)] public float summonGroundProbeDepth = 3f;
    [Min(0f)] public float summonHealOnPlayerHit = 40f;

    [Header("Sky Attack")]
    public float skyJumpCooldown = 8f;
    public float skyJumpHeight = 5f;
    public float skyJumpDuration = 1.5f;
    public float skyJumpSpeed = 6f;
    [Range(0f, 1f)] public float skyJumpChance = 0.3f;

    [Header("Impact Feedback")]
    public bool enableCameraShake = true;
    [Min(0f)] public float ascentShakeDuration = 0.3f;
    [Min(0f)] public float ascentShakeIntensity = 0.07f;
    [Min(0f)] public float apexShakeDuration = 0.45f;
    [Min(0f)] public float apexShakeIntensity = 0.16f;
    [Min(0f)] public float projectileShakeIntensity = 0.035f;
    [Min(0f)] public float landingShakeIntensity = 0.12f;

    [Header("Tactical AI")]
    [Min(0.05f)] public float decisionInterval = 0.15f;
    [Min(0f)] public float meleeWindup = 0.28f;
    [Tooltip("Minimum visible duration for Attack and Attack-NoEffect before movement can resume.")]
    [Min(0.1f)] public float meleeAttackCommitTime = 1f;
    [Min(0f)] public float rangedWindup = 0.35f;
    [Min(0f)] public float recoveryTime = 0.45f;
    [Min(0.1f)] public float verticalAttackTolerance = 1.5f;
    [Min(1f)] public float maxChaseDistance = 14f;
    [Range(0.05f, 0.95f)] public float phaseTwoHealthThreshold = 0.35f;
    [Range(0.35f, 1f)] public float phaseTwoCooldownMultiplier = 0.75f;
    [Range(0, 3)] public int allowedRepeatedActions = 1;
    public bool showDebugLogs;
    [Range(0f, 0.15f)] public float dodgeIncreasePerHealthReset = 0.025f;

    [Header("Final Boss Positioning")]
    [Tooltip("Keeps a spell boss at a deliberate casting distance, retreating when pressured and returning to attack range.")]
    public bool enableTacticalPositioning;
    [Min(1f)] public float preferredCombatDistance = 4.5f;
    [Min(0.1f)] public float combatDistanceTolerance = 0.85f;
    [Min(0.25f)] public float tacticalRepositionInterval = 1.15f;

    [Header("Arena")]
    public float minYPosition = -2f;

    private enum BossAction
    {
        None,
        Melee,
        Ranged,
        ShadowRain,
        Summon,
        Evade,
        SkyAttack,
        Teleport
    }

    private static readonly int RunParameter = Animator.StringToHash("Run");
    private static readonly int WalkParameter = Animator.StringToHash("Walk");
    private static readonly int AttackParameter = Animator.StringToHash("Attack");
    private static readonly int Attack1Parameter = Animator.StringToHash("Attack1");
    private static readonly int Attack2Parameter = Animator.StringToHash("Attack2");
    private static readonly int CastParameter = Animator.StringToHash("Cast");
    private static readonly int SpellParameter = Animator.StringToHash("Spell");
    private static readonly int JumpParameter = Animator.StringToHash("Jump");
    private static readonly int IdleState = Animator.StringToHash("Idle");
    private static readonly int SpellState = Animator.StringToHash("Spell");
    private static readonly int AttackNoEffectState = Animator.StringToHash("Attack-NoEffect");
    private static readonly int SpellNoEffectState = Animator.StringToHash("Spell-NoEffect");

    private readonly HashSet<int> animatorParameters = new HashSet<int>();
    private readonly List<int> meleeAttackParameters = new List<int>(3);

    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private EnemyHealth enemyHealth;
    private HealthSystem playerHealth;
    private Rigidbody2D playerRigidbody;
    private Collider2D playerCollider;
    private Collider2D bossCollider;
    private Rigidbody2D bossRigidbody;
    private Coroutine activeActionRoutine;
    private BossAction activeAction;
    private BossAction previousAction;
    private int repeatedActionCount;
    private int meleeAnimationIndex;
    private int activeAttackParameter;
    private bool runAnimationState;
    private bool jumpAnimationState;
    private bool spriteHiddenForTeleport;
    private bool playedDirectAnimation;
    private bool useNoEffectMeleeNext;
    private bool battleStarted;
    private bool combatStopped;
    private bool encounterDialogueLocked;
    private bool phaseTwo;
    private float nextDecisionTime;
    private float nextMeleeTime;
    private float nextRangedTime;
    private float nextShadowRainTime;
    private float nextSummonTime;
    private float nextTeleportTime;
    private float nextSkyAttackTime;
    private float nextReactiveDefenceTime;
    private float nextPlayerSearchTime;
    private float nextTacticalRepositionTime;
    private float combatPositionSide = 1f;
    private int healthResetCount;
    private float initialDodgeChance;
    private readonly List<GameObject> activeSummons = new List<GameObject>();
    private Vector3 homePosition;

    private const float PlayerSearchInterval = 0.5f;

    private void Start()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        enemyHealth = GetComponent<EnemyHealth>();
        bossCollider = GetComponent<Collider2D>();
        bossRigidbody = GetComponent<Rigidbody2D>();
        homePosition = transform.position;
        initialDodgeChance = enemyHealth != null ? enemyHealth.dodgeChance : 0f;

        CacheAnimatorParameters();
        ResetCombatAnimations();
        TryResolvePlayer();

        if (enemyHealth != null)
            enemyHealth.Damaged += OnBossDamaged;

        if (autoStartBattle)
            StartBattle();
    }

    private void Update()
    {
        if (!battleStarted)
            return;

        if (!TryResolvePlayer())
        {
            SetRun(false);
            return;
        }

        if (enemyHealth != null && enemyHealth.currentHealth <= 0f)
        {
            if (!combatStopped)
                StopCombat();
            return;
        }

        if (playerHealth != null && playerHealth.isDead)
        {
            if (!combatStopped)
                StopCombat();
            return;
        }

        if (combatStopped)
            ResumeCombatAfterPlayerRevive();

        ClampToGround();
        phaseTwo = enemyHealth != null &&
                   enemyHealth.currentHealth <= enemyHealth.maxHealth * phaseTwoHealthThreshold;

        if (activeActionRoutine != null)
            return;

        if (Time.time >= nextDecisionTime)
        {
            nextDecisionTime = Time.time + Mathf.Max(0.05f, decisionInterval);
            if (TryStartBestAction())
                return;
        }

        UpdatePositioning();
    }

    private void CacheAnimatorParameters()
    {
        animatorParameters.Clear();
        meleeAttackParameters.Clear();

        if (animator == null)
            return;

        foreach (AnimatorControllerParameter parameter in animator.parameters)
            animatorParameters.Add(parameter.nameHash);

        if (animatorParameters.Contains(AttackParameter))
            meleeAttackParameters.Add(AttackParameter);
        if (animatorParameters.Contains(Attack1Parameter))
            meleeAttackParameters.Add(Attack1Parameter);
        if (animatorParameters.Contains(Attack2Parameter))
            meleeAttackParameters.Add(Attack2Parameter);
    }

    private bool TryResolvePlayer()
    {
        if (player != null)
        {
            if (playerHealth == null)
                playerHealth = player.GetComponentInParent<HealthSystem>();
            if (playerHealth == null)
                playerHealth = player.GetComponentInChildren<HealthSystem>();
            if (playerRigidbody == null)
                playerRigidbody = player.GetComponentInParent<Rigidbody2D>();
            if (playerCollider == null)
                playerCollider = player.GetComponentInParent<Collider2D>();
            if (playerCollider == null)
                playerCollider = player.GetComponentInChildren<Collider2D>();
            return playerHealth != null;
        }

        playerHealth = null;
        playerRigidbody = null;
        playerCollider = null;

        if (Time.unscaledTime < nextPlayerSearchTime)
            return false;

        nextPlayerSearchTime = Time.unscaledTime + PlayerSearchInterval;
        GameObject foundPlayer = GameObject.FindWithTag("Player");
        if (foundPlayer == null)
            return false;

        player = foundPlayer.transform;
        playerHealth = foundPlayer.GetComponentInParent<HealthSystem>();
        if (playerHealth == null)
            playerHealth = HealthSystem.Instance;
        playerRigidbody = foundPlayer.GetComponentInParent<Rigidbody2D>();
        playerCollider = foundPlayer.GetComponentInParent<Collider2D>();
        if (playerCollider == null)
            playerCollider = foundPlayer.GetComponentInChildren<Collider2D>();
        return playerHealth != null;
    }

    private bool IsCombatFinished()
    {
        return playerHealth != null && playerHealth.isDead ||
               enemyHealth != null && enemyHealth.currentHealth <= 0f;
    }

    private bool TryStartBestAction()
    {
        float horizontalDistance = Mathf.Abs(player.position.x - transform.position.x);
        float verticalDistance = Mathf.Abs(player.position.y - transform.position.y);
        float now = Time.time;

        BossAction selectedAction = BossAction.None;
        float bestScore = float.MinValue;

        if (now >= nextMeleeTime &&
            horizontalDistance <= attackRange * 1.15f &&
            verticalDistance <= verticalAttackTolerance)
        {
            float closeness = 1f - Mathf.Clamp01(horizontalDistance / Mathf.Max(0.1f, attackRange));
            float playerFinishBonus = GetPlayerHealthPercent() <= 0.3f ? 25f : 0f;
            ConsiderAction(
                BossAction.Melee,
                90f + closeness * 35f + playerFinishBonus,
                ref selectedAction,
                ref bestScore);
        }

        float rangedMinimumDistance = enableVoidRiftCombo
            ? attackRange * 1.2f
            : attackRange * 0.75f;
        if ((enableVoidRiftCombo ||
             (attackProjectile != null && attackSpawnPoint != null)) &&
            now >= nextRangedTime &&
            horizontalDistance >= rangedMinimumDistance &&
            horizontalDistance <= rangedAttackRange)
        {
            float rangePosition = Mathf.InverseLerp(attackRange, rangedAttackRange, horizontalDistance);
            float phaseBonus = phaseTwo ? 20f : 0f;
            ConsiderAction(
                BossAction.Ranged,
                65f + rangePosition * 28f + phaseBonus,
                ref selectedAction,
                ref bestScore);
        }

        if (enableShadowRain &&
            now >= nextShadowRainTime &&
            horizontalDistance >= attackRange * 1.35f &&
            horizontalDistance <= rangedAttackRange * 1.1f)
        {
            ConsiderAction(
                BossAction.ShadowRain,
                72f + (phaseTwo ? 18f : 0f),
                ref selectedAction,
                ref bestScore);
        }

        if (healthResetCount >= 2 && HasSummonPrefab() &&
            now >= nextSummonTime &&
            horizontalDistance >= attackRange * 1.25f &&
            horizontalDistance <= rangedAttackRange * 1.1f)
        {
            int availableSlots = Mathf.Max(0, maxActiveSummons - GetActiveSummonCount());
            if (availableSlots > 0)
            {
                ConsiderAction(
                    BossAction.Summon,
                    // A summon must compete with Spell/Rain instead of being
                    // permanently starved by their higher tactical scores.
                    86f + (phaseTwo ? 18f : 0f) + availableSlots * 1.5f,
                    ref selectedAction,
                    ref bestScore);
            }
        }

        if (now >= nextSkyAttackTime &&
            skyJumpHeight > 0f &&
            horizontalDistance <= rangedAttackRange * 1.15f)
        {
            float pressureBonus = horizontalDistance <= attackRange * 1.5f ? 22f : 0f;
            float configuredWeight = Mathf.Lerp(-15f, 25f, skyJumpChance);
            ConsiderAction(
                BossAction.SkyAttack,
                48f + pressureBonus + configuredWeight + (phaseTwo ? 12f : 0f),
                ref selectedAction,
                ref bestScore);
        }

        if (HasValidTeleportPoint() && now >= nextTeleportTime)
        {
            float spacingNeed = 0f;
            if (horizontalDistance < attackRange * 0.7f)
                spacingNeed = 55f;
            else if (horizontalDistance > rangedAttackRange * 0.9f)
                spacingNeed = 35f;

            ConsiderAction(
                BossAction.Teleport,
                28f + spacingNeed + (phaseTwo ? 22f : 0f),
                ref selectedAction,
                ref bestScore);
        }

        if (selectedAction == BossAction.None)
            return false;

        StartAction(selectedAction);
        return true;
    }

    private void ConsiderAction(
        BossAction candidate,
        float score,
        ref BossAction selectedAction,
        ref float bestScore)
    {
        if (candidate == previousAction)
        {
            if (repeatedActionCount >= allowedRepeatedActions)
                return;

            score *= 0.45f;
        }

        // A small tie-breaker keeps the fight readable but not fully scripted.
        score += Random.Range(0f, 6f);
        if (score <= bestScore)
            return;

        bestScore = score;
        selectedAction = candidate;
    }

    private void StartAction(BossAction action)
    {
        activeAction = action;
        SetRun(false);

        switch (action)
        {
            case BossAction.Melee:
                nextMeleeTime = Time.time + GetCooldown(attackCooldown);
                activeActionRoutine = StartCoroutine(MeleeAttackRoutine());
                break;

            case BossAction.Ranged:
                nextRangedTime = Time.time + GetCooldown(rangedAttackCooldown);
                activeActionRoutine = StartCoroutine(RangedAttackRoutine());
                break;

            case BossAction.ShadowRain:
                nextShadowRainTime = Time.time + GetCooldown(shadowRainCooldown);
                activeActionRoutine = StartCoroutine(ShadowRainRoutine());
                break;

            case BossAction.Summon:
                nextSummonTime = Time.time + GetCooldown(summonCooldown);
                activeActionRoutine = StartCoroutine(SummonRoutine());
                break;

            case BossAction.SkyAttack:
                nextSkyAttackTime = Time.time + GetCooldown(skyJumpCooldown);
                activeActionRoutine = StartCoroutine(SkyAttackRoutine());
                break;

            case BossAction.Teleport:
                nextTeleportTime = Time.time + GetCooldown(teleportCooldown);
                activeActionRoutine = StartCoroutine(TeleportRoutine());
                break;
        }

        if (showDebugLogs)
            Debug.Log($"{name} chose {action}");
    }

    private void OnBossDamaged(EnemyHealth damagedBoss, float damage)
    {
        if (!enableReactiveDefence || !battleStarted || combatStopped ||
            damagedBoss == null || damagedBoss.currentHealth <= 0f ||
            Time.time < nextReactiveDefenceTime || !TryResolvePlayer())
            return;

        // Damage is a hard interruption. In particular, Cast/Spell must never
        // make this boss stand still and willingly absorb a player combo.
        nextReactiveDefenceTime = Time.time + reactiveDefenceCooldown;
        if (activeActionRoutine != null)
            StopCoroutine(activeActionRoutine);
        activeActionRoutine = null;
        AbortActiveAction();
        activeAction = BossAction.Evade;
        activeActionRoutine = StartCoroutine(ReactiveDefenceRoutine());
    }

    private IEnumerator ReactiveDefenceRoutine()
    {
        // Leave a short readable hit-stun window, then run away while facing
        // the player. This keeps Hit1 visible and gives the player a chance to chase.
        yield return WaitWhileCombatActive(reactiveHitPause);
        if (!CanContinueAction())
        {
            AbortActiveAction();
            yield break;
        }

        bool useTeleportEscape = useNoEffectSpellForTeleport &&
                                 Random.value <= reactiveTeleportChance;
        if (useTeleportEscape)
        {
            yield return ReactiveTeleportAway();
        }
        else
        {
            float endTime = Time.time + reactiveRetreatDuration;
            SetRun(true);
            while (CanContinueAction() && Time.time < endTime)
            {
                float direction = transform.position.x >= player.position.x ? 1f : -1f;
                float targetX = Mathf.Clamp(
                    player.position.x + direction * reactiveRetreatDistance,
                    homePosition.x - maxChaseDistance,
                    homePosition.x + maxChaseDistance);
                Vector3 position = transform.position;
                position.x = Mathf.MoveTowards(position.x, targetX, speed * 1.9f * Time.deltaTime);
                position.y = Mathf.Max(position.y, minYPosition);
                transform.position = position;
                FlipTowardsPlayer();

                if (Mathf.Abs(transform.position.x - player.position.x) >= reactiveRetreatDistance * 0.92f)
                    break;
                yield return null;
            }
            SetRun(false);
        }

        // A player who keeps chasing into the boss's personal space is met by
        // one quick retaliatory slash; otherwise the boss resumes ranged plans.
        if (CanContinueAction() && Time.time >= nextMeleeTime &&
            Mathf.Abs(player.position.x - transform.position.x) <= reactiveCounterRange)
        {
            nextMeleeTime = Time.time + GetCooldown(attackCooldown);
            FlipTowardsPlayer();
            BeginAttackAnimation(false);
            yield return WaitWhileCombatActive(meleeWindup * 0.65f);
            if (CanContinueAction() && IsPlayerInsideMeleeHitbox())
                playerHealth?.TakeDamage(attackDamage);
            yield return WaitWhileCombatActive(0.12f);
            EndAttackAnimation();
        }

        yield return WaitWhileCombatActive(recoveryTime * 0.5f);
        CompleteActiveAction();
    }

    private IEnumerator ReactiveTeleportAway()
    {
        PlayTeleportPortal(transform.position);
        SetBossSpriteVisible(false);
        yield return WaitWhileCombatActive(0.28f);
        if (!CanContinueAction())
        {
            AbortActiveAction();
            yield break;
        }

        // Keep the same side when possible, so the boss does not blink through
        // the player; only switch sides when it is pinned against arena bounds.
        float direction = transform.position.x >= player.position.x ? 1f : -1f;
        float targetX = player.position.x + direction * reactiveTeleportDistance;
        float clampedX = Mathf.Clamp(
            targetX,
            homePosition.x - maxChaseDistance,
            homePosition.x + maxChaseDistance);
        if (Mathf.Abs(clampedX - player.position.x) < reactiveTeleportDistance * 0.55f)
        {
            direction = -direction;
            clampedX = Mathf.Clamp(
                player.position.x + direction * reactiveTeleportDistance,
                homePosition.x - maxChaseDistance,
                homePosition.x + maxChaseDistance);
        }

        SetBossWorldPosition(new Vector3(
            clampedX,
            Mathf.Max(transform.position.y, minYPosition),
            transform.position.z));
        PlayTeleportPortal(transform.position);
        yield return WaitWhileCombatActive(teleportArrivalRevealDelay);
        SetBossSpriteVisible(true);
        FlipTowardsPlayer();
        nextTeleportTime = Time.time + GetCooldown(teleportCooldown);
    }

    private IEnumerator MeleeAttackRoutine()
    {
        FlipTowardsPlayer();
        BeginAttackAnimation(false);

        yield return WaitWhileCombatActive(meleeWindup);
        if (!CanContinueAction())
        {
            AbortActiveAction();
            yield break;
        }

        FlipTowardsPlayer();
        if (IsPlayerInsideMeleeHitbox())
            playerHealth?.TakeDamage(attackDamage);

        // Both Hac Am melee clips are one second. Keeping the follow-through
        // prevents Attack-NoEffect from being interrupted halfway through.
        yield return WaitWhileCombatActive(Mathf.Max(0f, meleeAttackCommitTime - meleeWindup));
        EndAttackAnimation();

        if (CanContinueAction() &&
            Mathf.Abs(player.position.x - transform.position.x) <= attackRange * 1.4f)
        {
            yield return MoveAwayFromPlayer(jumpBackDistance);
        }

        yield return WaitWhileCombatActive(recoveryTime);
        CompleteActiveAction();
    }

    private IEnumerator RangedAttackRoutine()
    {
        FlipTowardsPlayer();
        BeginAttackAnimation(true);

        yield return WaitWhileCombatActive(rangedWindup);
        if (!CanContinueAction())
        {
            AbortActiveAction();
            yield break;
        }

        if (enableVoidRiftCombo)
        {
            EndAttackAnimation();
            Vector2 strikePosition = GetPlayerStrikePosition();
            BossNoEffectAnimationVisual spellVisual = BossNoEffectAnimationVisual.Spawn(
                animator,
                spriteRenderer,
                "Spell",
                strikePosition + Vector2.up * spellEffectHeight,
                1.6f);
            spellVisual?.EnableContactDamage(
                playerHealth,
                Mathf.Max(1, attackDamage + voidRiftDamageBonus),
                spellDamageArmTime,
                voidRiftRadius,
                verticalAttackTolerance + 0.75f,
                () => ShakeCamera(spellImpactShakeDuration, spellImpactShakeIntensity));
            ShakeCamera(spellCastShakeDuration, spellCastShakeIntensity);
            yield return WaitWhileCombatActive(voidRiftSpellChannelTime);
            if (!CanContinueAction())
            {
                AbortActiveAction();
                yield break;
            }
            if (enableShadowStepCombo &&
                Random.value <= shadowStepAfterSpellChance &&
                CanContinueAction())
            {
                yield return ShadowStepMeleeCombo();
            }

            EndAttackAnimation();
            yield return WaitWhileCombatActive(recoveryTime);
            CompleteActiveAction();
            yield break;
        }

        int burstCount = Mathf.Clamp(
            rangedBurstCount + (phaseTwo ? 1 : 0) +
            (animatorParameters.Contains(Attack2Parameter) ? 1 : 0),
            1,
            5);

        for (int i = 0; i < burstCount; i++)
        {
            SpawnProjectileTowardsPredictedPlayer(1f + i * 0.05f);
            if (i + 1 < burstCount)
            {
                yield return WaitWhileCombatActive(projectileBurstInterval);
                if (!CanContinueAction())
                {
                    AbortActiveAction();
                    yield break;
                }
            }
        }

        yield return WaitWhileCombatActive(0.2f);
        EndAttackAnimation();
        yield return WaitWhileCombatActive(recoveryTime);
        CompleteActiveAction();
    }

    private IEnumerator ShadowRainRoutine()
    {
        ShadowRainStarted?.Invoke();
        // The boss deliberately holds position while it channels this attack.
        FlipTowardsPlayer();
        BeginAttackAnimation(true);
        yield return WaitWhileCombatActive(rangedWindup);
        if (!CanContinueAction())
        {
            AbortActiveAction();
            yield break;
        }

        EndAttackAnimation();
        Vector2 rainCenter = new Vector2(transform.position.x, transform.position.y);
        for (int wave = 0; wave < shadowRainWaves; wave++)
        {
            if (wave == 0)
            {
                SpawnShadowRainSpell(rainCenter);
            }
            else
            {
                float offset = shadowRainStartOffset + shadowRainLaneSpacing * (wave - 1);
                SpawnShadowRainSpell(rainCenter + Vector2.left * offset);
                SpawnShadowRainSpell(rainCenter + Vector2.right * offset);
            }

            // One soft rumble per expanding wave, rather than shaking once for
            // every lane. The player can read the coming lane and still dodge.
            ShakeCamera(spellCastShakeDuration, spellCastShakeIntensity);

            // Let this one visual wave finish before the next pair starts.
            yield return WaitWhileCombatActive(shadowRainHitDelay);
            if (!CanContinueAction())
            {
                AbortActiveAction();
                yield break;
            }

            yield return WaitWhileCombatActive(shadowRainWaveInterval);
        }

        yield return WaitWhileCombatActive(recoveryTime);
        CompleteActiveAction();
    }

    private IEnumerator SummonRoutine()
    {
        SummonStarted?.Invoke();
        FlipTowardsPlayer();
        BeginAttackAnimation(true); // Cast
        yield return WaitWhileCombatActive(rangedWindup);
        if (!CanContinueAction())
        {
            AbortActiveAction();
            yield break;
        }

        EndAttackAnimation();
        int availableSlots = Mathf.Max(0, maxActiveSummons - GetActiveSummonCount());
        int requestedCount = Random.Range(
            Mathf.Max(1, summonMinCount),
            Mathf.Max(summonMinCount, summonMaxCount) + 1);
        int summonCount = Mathf.Min(requestedCount, availableSlots);

        for (int i = 0; i < summonCount; i++)
        {
            if (!CanContinueAction())
            {
                AbortActiveAction();
                yield break;
            }

            Vector2 summonPosition = GetSummonPosition(i, summonCount);
            BossNoEffectAnimationVisual.Spawn(
                animator,
                spriteRenderer,
                "Spell",
                summonPosition + Vector2.up * spellEffectHeight,
                Mathf.Max(1.1f, summonSpellDelay + 0.45f),
                summonSpellVisualScale);
            ShakeCamera(spellCastShakeDuration, spellCastShakeIntensity);

            yield return WaitWhileCombatActive(summonSpellDelay);
            if (!CanContinueAction())
            {
                AbortActiveAction();
                yield break;
            }

            SpawnSummonedEnemy(summonPosition);
            yield return WaitWhileCombatActive(summonInterval);
        }

        yield return WaitWhileCombatActive(recoveryTime);
        CompleteActiveAction();
    }

    private Vector2 GetSummonPosition(int index, int total)
    {
        float playerX = player != null ? player.position.x : transform.position.x;
        float direction = index % 2 == 0 ? -1f : 1f;
        float ring = 1.25f + (index / 2) * summonSpread;
        if (total == 1)
            direction = transform.position.x >= playerX ? 1f : -1f;

        float x = Mathf.Clamp(
            playerX + direction * ring,
            homePosition.x - maxChaseDistance,
            homePosition.x + maxChaseDistance);
        // Probe only down from the player's floor height on the Ground layer.
        // This excludes upper platforms and background collision shapes.
        float playerFloorY = playerCollider != null
            ? playerCollider.bounds.min.y
            : Mathf.Max(minYPosition, player != null ? player.position.y : transform.position.y);
        return new Vector2(x, FindCombatGroundY(x, playerFloorY));
    }

    private void SpawnSummonedEnemy(Vector2 position)
    {
        GameObject prefab = GetRandomSummonPrefab();
        if (prefab == null)
            return;

        // This older Skeleton prefab exposes its clone as a Transform. Clone
        // that root explicitly, then use its GameObject instead of casting the
        // returned Unity Object to GameObject.
        Transform summonedTransform = Instantiate(prefab.transform, position, Quaternion.identity);
        if (summonedTransform == null)
        {
            Debug.LogError($"{name}: Could not instantiate summon prefab '{prefab.name}'.");
            return;
        }
        GameObject summoned = summonedTransform.gameObject;
        summoned.name = "HacAm Summon - " + prefab.name;

        // The source prefab's root is not always at its feet. Align its own
        // collider bottom to the raycast ground so every summon lands on floor.
        Collider2D summonedCollider = summoned.GetComponent<Collider2D>();
        if (summonedCollider == null)
            summonedCollider = summoned.GetComponentInChildren<Collider2D>();
        if (summonedCollider != null)
        {
            Physics2D.SyncTransforms();
            float correction = position.y + summonGroundOffset - summonedCollider.bounds.min.y;
            summonedTransform.position += Vector3.up * correction;
            Physics2D.SyncTransforms();
        }

        // The Skeleton prefab contains nested transforms whose visual pivot is
        // not its collider pivot. Do a final feet-to-floor alignment from the
        // actual rendered sprites so it never appears to float above the map.
        SpriteRenderer[] summonedRenderers = summoned.GetComponentsInChildren<SpriteRenderer>();
        float visualBottom = float.PositiveInfinity;
        for (int i = 0; i < summonedRenderers.Length; i++)
        {
            SpriteRenderer renderer = summonedRenderers[i];
            if (renderer != null && renderer.enabled && renderer.sprite != null)
                visualBottom = Mathf.Min(visualBottom, renderer.bounds.min.y);
        }
        if (!float.IsPositiveInfinity(visualBottom))
        {
            float visualCorrection = position.y + summonGroundOffset - visualBottom;
            summonedTransform.position += Vector3.up * visualCorrection;
            Physics2D.SyncTransforms();
        }

        EnemyFSM summonedBrain = summoned.GetComponent<EnemyFSM>();
        if (summonedBrain == null)
            summonedBrain = summoned.GetComponentInChildren<EnemyFSM>();
        if (summonedBrain != null)
        {
            summonedBrain.SetSummonedTarget(player);
            summonedBrain.PlayerDamaged += OnSummonedEnemyDamagedPlayer;
        }
        activeSummons.Add(summoned);
    }

    private void OnSummonedEnemyDamagedPlayer(EnemyFSM summonedEnemy)
    {
        if (enemyHealth == null || enemyHealth.currentHealth <= 0f)
            return;

        enemyHealth.RestoreHealth(summonHealOnPlayerHit);
    }

    private float FindCombatGroundY(float x, float playerFloorY)
    {
        LayerMask groundMask = 1 << LayerMask.NameToLayer("Ground");
        if (player != null)
        {
            PlayerJump playerJump = player.GetComponent<PlayerJump>();
            if (playerJump != null && playerJump.groundLayer.value != 0)
                groundMask = playerJump.groundLayer;
        }

        Vector2 origin = new Vector2(x, playerFloorY + 0.45f);
        RaycastHit2D[] hits = Physics2D.RaycastAll(
            origin,
            Vector2.down,
            summonGroundProbeDepth,
            groundMask);
        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D hitCollider = hits[i].collider;
            if (hitCollider != null && !hitCollider.isTrigger)
                return hits[i].point.y;
        }

        // A lane over a gap deliberately falls back to the active combat floor
        // rather than spawning an enemy on another level of the map.
        return playerFloorY;
    }

    private bool HasSummonPrefab()
    {
        return summonEnemyPrefab != null;
    }

    private GameObject GetRandomSummonPrefab()
    {
        return summonEnemyPrefab;
    }

    private int GetActiveSummonCount()
    {
        for (int i = activeSummons.Count - 1; i >= 0; i--)
        {
            if (activeSummons[i] == null)
                activeSummons.RemoveAt(i);
        }
        return activeSummons.Count;
    }

    private void SpawnShadowRainSpell(Vector2 targetPosition)
    {
        BossNoEffectAnimationVisual rainVisual = BossNoEffectAnimationVisual.Spawn(
            animator,
            spriteRenderer,
            "Spell",
            targetPosition + Vector2.up * spellEffectHeight,
            1.6f,
            shadowRainVisualScale);
        rainVisual?.EnableContactDamage(
            playerHealth,
            Mathf.Max(1, attackDamage + voidRiftDamageBonus),
            spellDamageArmTime,
            shadowRainLaneRadius,
            verticalAttackTolerance + 0.75f,
            () => ShakeCamera(0.16f, projectileShakeIntensity * 1.5f));
    }

    private IEnumerator ShadowStepMeleeCombo()
    {
        PlayTeleportPortal(transform.position);
        SetBossSpriteVisible(false);
        yield return WaitWhileCombatActive(0.38f);
        if (!CanContinueAction())
        {
            AbortActiveAction();
            yield break;
        }

        float side = transform.position.x <= player.position.x ? -1f : 1f;
        Vector3 destination = new Vector3(
            player.position.x + side * shadowStepAttackDistance,
            transform.position.y,
            transform.position.z);
        SetBossWorldPosition(destination);
        PlayTeleportPortal(destination);
        yield return WaitWhileCombatActive(teleportArrivalRevealDelay);
        SetBossSpriteVisible(true);
        if (!CanContinueAction())
        {
            AbortActiveAction();
            yield break;
        }

        FlipTowardsPlayer();
        BeginAttackAnimation(false);
        yield return WaitWhileCombatActive(meleeWindup);
        if (CanContinueAction() && IsPlayerInsideShadowStepHitbox())
            playerHealth?.TakeDamage(attackDamage);

        // Attack and Attack-NoEffect are both one-second clips. Do not cancel
        // them at the hit frame: the follow-through is what makes the shadow
        // step feel like a real slash instead of a teleporting snap.
        float followThrough = Mathf.Max(0f, shadowStepAttackCommitTime - meleeWindup);
        yield return WaitWhileCombatActive(followThrough);
        EndAttackAnimation();
        if (CanContinueAction())
            yield return MoveAwayFromPlayer(shadowStepRetreatDistance);
    }

    private IEnumerator SkyAttackRoutine()
    {
        SetJump(true);
        Vector3 groundPosition = transform.position;
        Vector3 skyPosition = groundPosition + Vector3.up * skyJumpHeight;

        ShakeCamera(ascentShakeDuration, ascentShakeIntensity);

        yield return MoveToPosition(skyPosition, skyJumpSpeed);
        if (!CanContinueAction())
        {
            AbortActiveAction();
            yield break;
        }

        ShakeCamera(apexShakeDuration, apexShakeIntensity);
        BeginAttackAnimation(true, true);
        int projectileCount = phaseTwo ? 3 : 2;
        for (int i = 0; i < projectileCount; i++)
        {
            SpawnProjectileTowardsPredictedPlayer(1f + i * 0.08f);
            ShakeCamera(0.12f, projectileShakeIntensity);
            yield return WaitWhileCombatActive(projectileBurstInterval);
        }
        EndAttackAnimation();

        yield return WaitWhileCombatActive(skyJumpDuration);
        if (CanContinueAction())
            yield return MoveToPosition(groundPosition, skyJumpSpeed);

        ShakeCamera(0.28f, landingShakeIntensity);
        SetJump(false);
        yield return WaitWhileCombatActive(recoveryTime);
        CompleteActiveAction();
    }

    private IEnumerator TeleportRoutine()
    {
        SetJump(true);
        if (useNoEffectSpellForTeleport)
        {
            PlayTeleportPortal(transform.position);
            SetBossSpriteVisible(false);
        }
        yield return WaitWhileCombatActive(useNoEffectSpellForTeleport ? 0.48f : 0.3f);
        if (!CanContinueAction())
        {
            AbortActiveAction();
            yield break;
        }

        Transform targetPoint = SelectTacticalTeleportPoint();
        if (teleportNearPlayer && player != null)
        {
            Vector3 destination = GetNearPlayerTeleportPosition();
            SetBossWorldPosition(destination);
            if (useNoEffectSpellForTeleport)
                PlayTeleportPortal(destination);
        }
        else if (targetPoint != null)
        {
            Vector3 destination = targetPoint.position;
            destination.y = Mathf.Max(destination.y, minYPosition);
            SetBossWorldPosition(destination);
            if (useNoEffectSpellForTeleport)
                PlayTeleportPortal(destination);
        }

        FlipTowardsPlayer();
        yield return WaitWhileCombatActive(
            useNoEffectSpellForTeleport ? teleportArrivalRevealDelay : 0.2f);
        if (useNoEffectSpellForTeleport)
            SetBossSpriteVisible(true);
        SetJump(false);
        yield return WaitWhileCombatActive(recoveryTime);
        CompleteActiveAction();
    }

    private void UpdatePositioning()
    {
        if (player == null || player.position.y <= minYPosition)
        {
            SetRun(false);
            return;
        }

        float horizontalDistance = Mathf.Abs(player.position.x - transform.position.x);
        if (enableTacticalPositioning)
        {
            if (nextTacticalRepositionTime <= 0f)
            {
                combatPositionSide = transform.position.x >= player.position.x ? 1f : -1f;
                nextTacticalRepositionTime = Time.time + tacticalRepositionInterval;
            }
            else if (Time.time >= nextTacticalRepositionTime)
            {
                nextTacticalRepositionTime = Time.time + tacticalRepositionInterval;
                if (Random.value < 0.42f)
                    combatPositionSide *= -1f;
            }

            float desiredCombatDistance = Mathf.Max(attackRange * 1.35f, preferredCombatDistance);
            float combatTargetX = player.position.x + combatPositionSide * desiredCombatDistance;
            combatTargetX = Mathf.Clamp(combatTargetX,
                homePosition.x - maxChaseDistance,
                homePosition.x + maxChaseDistance);

            if (Mathf.Abs(horizontalDistance - desiredCombatDistance) > combatDistanceTolerance)
            {
                Vector3 tacticalPosition = transform.position;
                tacticalPosition.x = Mathf.MoveTowards(tacticalPosition.x, combatTargetX, speed * Time.deltaTime);
                tacticalPosition.y = Mathf.Max(tacticalPosition.y, minYPosition);
                transform.position = tacticalPosition;
                SetRun(true);
            }
            else
            {
                SetRun(false);
            }

            FlipTowardsPlayer();
            return;
        }

        float desiredDistance = Mathf.Max(attackRange * 0.8f, 0.35f);
        if (horizontalDistance <= desiredDistance)
        {
            SetRun(false);
            FlipTowardsPlayer();
            return;
        }

        float targetX = Mathf.Clamp(
            player.position.x,
            homePosition.x - maxChaseDistance,
            homePosition.x + maxChaseDistance);

        Vector3 position = transform.position;
        position.x = Mathf.MoveTowards(position.x, targetX, speed * Time.deltaTime);
        position.y = Mathf.Max(position.y, minYPosition);
        transform.position = position;
        FlipTowardsPlayer();
        SetRun(true);
    }

    private IEnumerator MoveAwayFromPlayer(float distance)
    {
        SetJump(true);
        float direction = transform.position.x >= player.position.x ? 1f : -1f;
        float targetX = Mathf.Clamp(
            transform.position.x + direction * distance,
            homePosition.x - maxChaseDistance,
            homePosition.x + maxChaseDistance);
        Vector3 target = new Vector3(targetX, Mathf.Max(transform.position.y, minYPosition), transform.position.z);

        yield return MoveToPosition(target, jumpSpeed);
        SetJump(false);
    }

    private IEnumerator MoveToPosition(Vector3 target, float movementSpeed)
    {
        movementSpeed = Mathf.Max(0.1f, movementSpeed);
        while (CanContinueAction() && (transform.position - target).sqrMagnitude > 0.01f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                target,
                movementSpeed * Time.deltaTime);
            yield return null;
        }
    }

    private IEnumerator WaitWhileCombatActive(float duration)
    {
        float endTime = Time.time + Mathf.Max(0f, duration);
        while (CanContinueAction() && Time.time < endTime)
            yield return null;
    }

    private bool CanContinueAction()
    {
        return battleStarted &&
               !combatStopped &&
               player != null &&
               !IsCombatFinished();
    }

    private bool IsPlayerInsideMeleeHitbox()
    {
        if (player == null || playerHealth == null)
            return false;

        if (bossCollider != null && playerCollider != null)
        {
            ColliderDistance2D colliderDistance = bossCollider.Distance(playerCollider);
            if (colliderDistance.isOverlapped || colliderDistance.distance <= meleeHitPadding)
                return true;
        }

        return Mathf.Abs(player.position.x - transform.position.x) <= attackRange * 1.1f &&
               Mathf.Abs(player.position.y - transform.position.y) <= verticalAttackTolerance;
    }

    private bool IsPlayerInsideShadowStepHitbox()
    {
        if (player == null || playerHealth == null)
            return false;

        Vector2 playerCenter = playerCollider != null
            ? playerCollider.bounds.center
            : (Vector2)player.position;
        Vector2 bossCenter = bossCollider != null
            ? bossCollider.bounds.center
            : (Vector2)transform.position;
        float strikeReach = Mathf.Max(attackRange * 1.25f, shadowStepAttackDistance + meleeHitPadding);
        return Mathf.Abs(playerCenter.x - bossCenter.x) <= strikeReach &&
               Mathf.Abs(playerCenter.y - bossCenter.y) <= verticalAttackTolerance;
    }

    private void SpawnProjectileTowardsPredictedPlayer(float speedMultiplier)
    {
        if (attackProjectile == null || attackSpawnPoint == null || player == null)
            return;

        Vector2 targetPosition = GetPredictedPlayerPosition();

        Vector2 direction = (targetPosition - (Vector2)attackSpawnPoint.position).normalized;
        Dan.Spawn(
            attackProjectile,
            attackSpawnPoint.position,
            direction * Mathf.Max(0.1f, projectileSpeed) * speedMultiplier,
            attackDamage);
    }

    private Vector2 GetPredictedPlayerPosition()
    {
        if (player == null)
            return transform.position;

        Vector2 targetPosition = player.position;
        if (playerRigidbody == null)
            return targetPosition;

        Vector2 prediction = playerRigidbody.linearVelocity *
                             Mathf.Clamp(projectilePredictionTime, 0f, 0.6f);
        return targetPosition + Vector2.ClampMagnitude(prediction, 2.5f);
    }

    private Vector2 GetPlayerStrikePosition()
    {
        if (playerCollider != null)
        {
            Bounds bounds = playerCollider.bounds;
            return new Vector2(bounds.center.x, bounds.min.y);
        }

        return player != null ? player.position : transform.position;
    }

    private void ApplyVoidStrike(Vector2 strikePosition)
    {
        if (playerHealth == null || player == null)
            return;

        float playerX = playerCollider != null
            ? playerCollider.bounds.center.x
            : player.position.x;
        if (Mathf.Abs(playerX - strikePosition.x) <= voidRiftRadius)
            playerHealth.TakeDamage(Mathf.Max(1, attackDamage + voidRiftDamageBonus));
    }

    private void SpawnVoidRiftAtPredictedPosition(float horizontalOffset)
    {
        Vector2 riftPosition = GetPredictedPlayerPosition();
        riftPosition.x += horizontalOffset;
        BossVoidRift2D.SpawnAttack(
            riftPosition,
            voidRiftRadius,
            voidRiftWarningTime,
            voidRiftDuration,
            Mathf.Max(1, attackDamage + voidRiftDamageBonus),
            playerHealth);
    }

    private Transform SelectTacticalTeleportPoint()
    {
        if (teleportPoints == null || player == null)
            return null;

        float currentDistance = Mathf.Abs(player.position.x - transform.position.x);
        float preferredDistance = Mathf.Lerp(attackRange, rangedAttackRange, 0.6f);
        Transform bestPoint = null;
        float bestScore = float.MinValue;

        foreach (Transform point in teleportPoints)
        {
            if (point == null)
                continue;

            float playerDistance = Mathf.Abs(point.position.x - player.position.x);
            float movementDistance = Mathf.Abs(point.position.x - transform.position.x);
            float score;

            if (currentDistance < attackRange * 0.8f || phaseTwo)
                score = playerDistance * 3f;
            else
                score = -Mathf.Abs(playerDistance - preferredDistance) * 2f + movementDistance * 0.25f;

            if (movementDistance < 0.5f)
                score -= 20f;

            if (score > bestScore)
            {
                bestScore = score;
                bestPoint = point;
            }
        }

        return bestPoint;
    }

    private Vector3 GetNearPlayerTeleportPosition()
    {
        float side = transform.position.x <= player.position.x ? -1f : 1f;
        float currentX = transform.position.x;
        float limitedX = GetLimitedTeleportX(currentX, player.position.x + side * teleportArrivalDistance);

        // If the desired flank is effectively where the boss already stands,
        // blink through to the opposite flank instead of playing a fake tele.
        if (Mathf.Abs(limitedX - currentX) < minimumTeleportDisplacement)
            limitedX = GetLimitedTeleportX(currentX, player.position.x - side * teleportArrivalDistance);

        if (Mathf.Abs(limitedX - currentX) < minimumTeleportDisplacement)
        {
            float fallbackDirection = currentX >= player.position.x ? 1f : -1f;
            limitedX = Mathf.Clamp(
                currentX + fallbackDirection * minimumTeleportDisplacement,
                homePosition.x - maxChaseDistance,
                homePosition.x + maxChaseDistance);
        }

        return new Vector3(limitedX, Mathf.Max(minYPosition, transform.position.y), transform.position.z);
    }

    private float GetLimitedTeleportX(float currentX, float desiredX)
    {
        float movedX = Mathf.MoveTowards(
            currentX,
            desiredX,
            Mathf.Max(0.5f, maxTeleportTravelDistance));
        return Mathf.Clamp(
            movedX,
            homePosition.x - maxChaseDistance,
            homePosition.x + maxChaseDistance);
    }

    private bool HasValidTeleportPoint()
    {
        if (teleportPoints == null)
            return false;

        for (int i = 0; i < teleportPoints.Length; i++)
        {
            if (teleportPoints[i] != null)
                return true;
        }

        return false;
    }

    private void BeginAttackAnimation(bool ranged, bool skySpell = false)
    {
        EndAttackAnimation();

        if (!ranged && alternateNoEffectMelee)
        {
            bool useNoEffect = useNoEffectMeleeNext;
            useNoEffectMeleeNext = !useNoEffectMeleeNext;
            if (useNoEffect && PlayNoEffectState(AttackNoEffectState))
                return;
        }

        if (skySpell && useSpellForSkyAttack && animatorParameters.Contains(SpellParameter))
        {
            activeAttackParameter = SpellParameter;
        }
        else if (ranged && useCastForRangedAttack && animatorParameters.Contains(CastParameter))
        {
            activeAttackParameter = CastParameter;
        }
        else if (ranged && animatorParameters.Contains(Attack2Parameter))
        {
            activeAttackParameter = Attack2Parameter;
        }
        else if (meleeAttackParameters.Count > 0)
        {
            activeAttackParameter = meleeAttackParameters[
                meleeAnimationIndex % meleeAttackParameters.Count];
            meleeAnimationIndex++;
        }
        else
        {
            return;
        }

        animator.SetBool(activeAttackParameter, true);
    }

    private void BeginSpellAnimationFromCast()
    {
        EndAttackAnimation();
        if (animator == null || !animatorParameters.Contains(SpellParameter))
        {
            BeginAttackAnimation(true, true);
            return;
        }

        if (animator.HasState(0, SpellState))
            animator.CrossFade(SpellState, 0.08f, 0, 0f);

        activeAttackParameter = SpellParameter;
        animator.SetBool(SpellParameter, true);
    }

    private void EndAttackAnimation()
    {
        if (animator != null && activeAttackParameter != 0)
            animator.SetBool(activeAttackParameter, false);

        if (animator != null && playedDirectAnimation)
        {
            playedDirectAnimation = false;
            if (animator.HasState(0, IdleState))
                animator.CrossFade(IdleState, 0.1f, 0, 0f);
        }

        activeAttackParameter = 0;
    }

    private void PlayTeleportPortal(Vector3 position)
    {
        BossNoEffectAnimationVisual.Spawn(
            animator,
            spriteRenderer,
            "Spell-NoEffect",
            (Vector2)position + Vector2.up * teleportPortalHeight,
            teleportPortalEffectDuration);
    }

    private void SetBossSpriteVisible(bool visible)
    {
        spriteHiddenForTeleport = !visible;
        if (spriteRenderer != null)
            spriteRenderer.enabled = visible;

        // Teleportation hides only the sprite. The boss stays a legal target
        // for the player's immediate OverlapCircle attack at both portals.
        if (enemyHealth != null && enemyHealth.currentHealth > 0f)
            enemyHealth.enabled = true;
        if (bossCollider != null)
            bossCollider.enabled = true;
        if (bossRigidbody != null)
            bossRigidbody.simulated = true;
        if (visible)
            Physics2D.SyncTransforms();
    }

    private void SetBossWorldPosition(Vector3 position)
    {
        position.y = Mathf.Max(position.y, minYPosition);
        if (bossRigidbody != null)
            bossRigidbody.position = position;
        else
            transform.position = position;

        // AnimatorAttack checks colliders in the same frame. Keep physics and
        // the arrival visual in sync instead of leaving the collider behind.
        Physics2D.SyncTransforms();
    }

    private bool PlayNoEffectState(int stateHash)
    {
        if (animator == null || !animator.HasState(0, stateHash))
            return false;

        EndAttackAnimation();
        animator.CrossFade(stateHash, 0.1f, 0, 0f);
        playedDirectAnimation = true;
        return true;
    }

    private void SetRun(bool value)
    {
        if (runAnimationState == value)
            return;

        runAnimationState = value;
        if (animator == null)
            return;

        if (animatorParameters.Contains(RunParameter))
            animator.SetBool(RunParameter, value);
        else if (animatorParameters.Contains(WalkParameter))
            animator.SetBool(WalkParameter, value);
    }

    private void SetJump(bool value)
    {
        if (jumpAnimationState == value)
            return;

        jumpAnimationState = value;
        if (animator != null && animatorParameters.Contains(JumpParameter))
            animator.SetBool(JumpParameter, value);
    }

    private void FlipTowardsPlayer()
    {
        if (player == null)
            return;

        Vector3 scale = transform.localScale;
        float facing = player.position.x >= transform.position.x ? 1f : -1f;

        if (useSpriteRendererFacing && spriteRenderer != null)
        {
            bool shouldFlip = facing < 0f;
            spriteRenderer.flipX = invertSpriteFacing ? !shouldFlip : shouldFlip;
            return;
        }

        scale.x = Mathf.Abs(scale.x) * facing;
        transform.localScale = scale;
    }

    private float GetCooldown(float baseCooldown)
    {
        float healthMultiplier = phaseTwo ? phaseTwoCooldownMultiplier : 1f;
        float resetMultiplier = Mathf.Pow(0.91f, healthResetCount);
        return Mathf.Max(0.1f, baseCooldown * healthMultiplier * resetMultiplier);
    }

    public void OnHealthBarReset(int resetsCompleted)
    {
        healthResetCount = Mathf.Max(0, resetsCompleted);
        phaseTwo = healthResetCount > 0;
        if (enemyHealth != null)
        {
            enemyHealth.dodgeChance = Mathf.Clamp(
                initialDodgeChance + healthResetCount * dodgeIncreasePerHealthReset,
                0f,
                0.5f);
        }
        nextDecisionTime = Time.time + 0.35f;
        nextRangedTime = Time.time + 0.2f;
        nextShadowRainTime = Time.time + 0.5f;
        ShakeCamera(0.4f, 0.12f + healthResetCount * 0.015f);
    }

    private float GetPlayerHealthPercent()
    {
        if (playerHealth == null || playerHealth.maxHP <= 0)
            return 1f;
        return Mathf.Clamp01((float)playerHealth.currentHP / playerHealth.maxHP);
    }

    private void ShakeCamera(float duration, float intensity)
    {
        if (enableCameraShake && duration > 0f && intensity > 0f)
            BossCameraShake2D.Shake(duration, intensity);
    }

    private void CompleteActiveAction()
    {
        EndAttackAnimation();
        SetJump(false);
        SetRun(false);

        if (activeAction == previousAction)
            repeatedActionCount++;
        else
            repeatedActionCount = 0;

        previousAction = activeAction;
        activeAction = BossAction.None;
        activeActionRoutine = null;
        nextDecisionTime = Time.time + Mathf.Max(0f, recoveryTime);
    }

    private void AbortActiveAction()
    {
        EndAttackAnimation();
        if (spriteHiddenForTeleport)
            SetBossSpriteVisible(true);
        SetJump(false);
        SetRun(false);
        activeAction = BossAction.None;
        activeActionRoutine = null;
    }

    private void ResetCombatAnimations()
    {
        if (spriteHiddenForTeleport)
            SetBossSpriteVisible(true);
        runAnimationState = true;
        jumpAnimationState = true;
        SetRun(false);
        SetJump(false);
        EndAttackAnimation();

        if (animator == null)
            return;

        for (int i = 0; i < meleeAttackParameters.Count; i++)
            animator.SetBool(meleeAttackParameters[i], false);

        if (animatorParameters.Contains(CastParameter))
            animator.SetBool(CastParameter, false);
        if (animatorParameters.Contains(SpellParameter))
            animator.SetBool(SpellParameter, false);
    }

    private void ClampToGround()
    {
        if (transform.position.y >= minYPosition)
            return;

        Vector3 position = transform.position;
        position.y = minYPosition;
        transform.position = position;
    }

    private void StopCombat()
    {
        combatStopped = true;
        if (activeActionRoutine != null)
        {
            StopCoroutine(activeActionRoutine);
            activeActionRoutine = null;
        }

        activeAction = BossAction.None;
        ResetCombatAnimations();
    }

    private void ResumeCombatAfterPlayerRevive()
    {
        combatStopped = false;
        activeAction = BossAction.None;
        activeActionRoutine = null;
        previousAction = BossAction.None;
        repeatedActionCount = 0;
        ResetCombatAnimations();

        float now = Time.time;
        nextDecisionTime = now + 0.35f;
        nextMeleeTime = now + 0.2f;
        nextRangedTime = now + 0.65f;
        nextTeleportTime = Mathf.Max(nextTeleportTime, now + 0.8f);
        nextSkyAttackTime = Mathf.Max(nextSkyAttackTime, now + 1f);

        if (showDebugLogs)
            Debug.Log($"{name} resumed combat after player revival.");
    }

    private void OnDisable()
    {
        if (enemyHealth != null)
            enemyHealth.Damaged -= OnBossDamaged;

        if (activeActionRoutine != null)
        {
            StopCoroutine(activeActionRoutine);
            activeActionRoutine = null;
        }

        activeAction = BossAction.None;
        if (animator != null)
            ResetCombatAnimations();
    }
    public void SetEncounterDialogueLock(bool locked)
    {
        encounterDialogueLocked = locked;
        if (locked) PauseForDialogue();
    }

    public void StartBattle()
    {
        if (battleStarted && !combatStopped)
            return;

        battleStarted = true;
        combatStopped = false;
        phaseTwo = false;
        previousAction = BossAction.None;
        repeatedActionCount = 0;
        nextDecisionTime = Time.time + 0.75f;
        nextMeleeTime = Time.time;
        nextRangedTime = Time.time + 0.5f;
        nextSummonTime = Time.time + 1.15f;
        nextTeleportTime = Time.time + Mathf.Max(1f, teleportCooldown * 0.5f);
        nextSkyAttackTime = Time.time + Mathf.Max(1f, skyJumpCooldown * 0.5f);

        if (showDebugLogs)
            Debug.Log($"{name} battle started with tactical AI.");
    }

    public void PauseForDialogue()
    {
        if (battleStarted && !combatStopped)
            StopCombat();
    }

    public void ResumeAfterDialogue()
    {
        if (battleStarted && combatStopped &&
            (enemyHealth == null || enemyHealth.currentHealth > 0f))
        {
            ResumeCombatAfterPlayerRevive();
        }
    }
}

[DefaultExecutionOrder(10000)]
internal sealed class BossCameraShake2D : MonoBehaviour
{
    private float remainingTime;
    private float totalDuration;
    private float intensity;
    private float noiseSeed;
    private Vector3 lastOffset;
    private Vector3 lastShakenPosition;
    private bool hasAppliedOffset;

    public static void Shake(float duration, float shakeIntensity)
    {
        Camera targetCamera = Camera.main;
        if (targetCamera == null)
            return;

        BossCameraShake2D shaker = targetCamera.GetComponent<BossCameraShake2D>();
        if (shaker == null)
            shaker = targetCamera.gameObject.AddComponent<BossCameraShake2D>();

        shaker.AddShake(duration, shakeIntensity);
    }

    private void AddShake(float duration, float shakeIntensity)
    {
        remainingTime = Mathf.Max(remainingTime, duration);
        totalDuration = Mathf.Max(totalDuration, duration);
        intensity = Mathf.Max(intensity, shakeIntensity);
        noiseSeed = Random.Range(0f, 1000f);
    }

    private void LateUpdate()
    {
        Vector3 basePosition = transform.localPosition;
        if (hasAppliedOffset &&
            (basePosition - lastShakenPosition).sqrMagnitude <= 0.00000001f)
        {
            basePosition -= lastOffset;
        }

        lastOffset = Vector3.zero;
        hasAppliedOffset = false;

        if (remainingTime <= 0f)
        {
            transform.localPosition = basePosition;
            intensity = 0f;
            totalDuration = 0f;
            return;
        }

        remainingTime = Mathf.Max(0f, remainingTime - Time.unscaledDeltaTime);
        float envelope = totalDuration > 0f
            ? Mathf.Clamp01(remainingTime / totalDuration)
            : 0f;
        envelope *= envelope;

        float sampleTime = Time.unscaledTime * 24f;
        float offsetX = Mathf.PerlinNoise(noiseSeed, sampleTime) * 2f - 1f;
        float offsetY = Mathf.PerlinNoise(noiseSeed + 37.1f, sampleTime) * 2f - 1f;
        lastOffset = new Vector3(offsetX, offsetY, 0f) * intensity * envelope;
        lastShakenPosition = basePosition + lastOffset;
        transform.localPosition = lastShakenPosition;
        hasAppliedOffset = true;
    }

    private void OnDisable()
    {
        if (hasAppliedOffset &&
            (transform.localPosition - lastShakenPosition).sqrMagnitude <= 0.00000001f)
        {
            transform.localPosition -= lastOffset;
        }

        lastOffset = Vector3.zero;
        hasAppliedOffset = false;
        remainingTime = 0f;
    }
}
