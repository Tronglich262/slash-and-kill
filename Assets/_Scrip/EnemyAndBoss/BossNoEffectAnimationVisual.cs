using UnityEngine;

/// <summary>
/// Displays a no-effect animation clip as a detached world effect.
/// The real boss keeps its own Animator state and never moves to the target.
/// </summary>
internal sealed class BossNoEffectAnimationVisual : MonoBehaviour
{
    private float destroyAt;
    private float spawnedAt;
    private float fadeInDuration;
    private float fadeOutDuration;
    private SpriteRenderer effectRenderer;
    private HealthSystem damageTarget;
    private int contactDamage;
    private float damageArmedAt;
    private float damageRadius;
    private float damageVerticalTolerance;
    private bool damageEnabled;
    private bool damageDealt;
    private System.Action onDamageContact;
    private BoxCollider2D damageCollider;

    public static BossNoEffectAnimationVisual Spawn(
        Animator sourceAnimator,
        SpriteRenderer sourceRenderer,
        string stateName,
        Vector2 position,
        float lifetime,
        float scaleMultiplier = 1f,
        float fadeIn = 0.08f,
        float fadeOut = 0.18f)
    {
        if (sourceAnimator == null ||
            sourceAnimator.runtimeAnimatorController == null ||
            sourceRenderer == null)
        {
            return null;
        }

        GameObject effect = new GameObject("HacAm Darkness Strike");
        effect.transform.position = new Vector3(position.x, position.y, 0f);
        effect.transform.localScale = sourceRenderer.transform.lossyScale *
                                      Mathf.Max(0.01f, scaleMultiplier);

        SpriteRenderer renderer = effect.AddComponent<SpriteRenderer>();
        renderer.sharedMaterial = sourceRenderer.sharedMaterial;
        renderer.sortingLayerID = sourceRenderer.sortingLayerID;
        renderer.sortingOrder = sourceRenderer.sortingOrder + 6;

        Animator animator = effect.AddComponent<Animator>();
        animator.runtimeAnimatorController = sourceAnimator.runtimeAnimatorController;
        animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        animator.Rebind();
        animator.Update(0f);
        int stateHash = Animator.StringToHash("Base Layer." + stateName);
        if (!animator.HasState(0, stateHash))
        {
            Destroy(effect);
            return null;
        }

        SetStateBoolIfPresent(animator, stateName, true);
        animator.Play(stateHash, 0, 0f);
        animator.Update(0f);

        BossNoEffectAnimationVisual visual = effect.AddComponent<BossNoEffectAnimationVisual>();
        visual.destroyAt = Time.time + Mathf.Max(0.1f, lifetime);
        visual.spawnedAt = Time.time;
        visual.fadeInDuration = Mathf.Max(0f, fadeIn);
        visual.fadeOutDuration = Mathf.Max(0f, fadeOut);
        visual.effectRenderer = renderer;
        return visual;
    }

    public void EnableContactDamage(
        HealthSystem target,
        int damage,
        float armDelay,
        float horizontalRadius,
        float verticalTolerance,
        System.Action onContact = null)
    {
        damageTarget = target;
        contactDamage = Mathf.Max(1, damage);
        damageArmedAt = Time.time + Mathf.Max(0f, armDelay);
        damageRadius = Mathf.Max(0.1f, horizontalRadius);
        damageVerticalTolerance = Mathf.Max(0.1f, verticalTolerance);
        onDamageContact = onContact;
        damageEnabled = target != null;

        if (damageEnabled)
        {
            damageCollider = gameObject.GetComponent<BoxCollider2D>();
            if (damageCollider == null)
                damageCollider = gameObject.AddComponent<BoxCollider2D>();
            damageCollider.isTrigger = true;
            damageCollider.size = new Vector2(damageRadius * 2f, damageVerticalTolerance * 2f);
        }
    }

    private void Update()
    {
        float remaining = destroyAt - Time.time;
        if (remaining <= 0f)
        {
            Destroy(gameObject);
            return;
        }

        TryApplyContactDamage();

        if (effectRenderer == null)
            return;

        float elapsed = Time.time - spawnedAt;
        float alpha = fadeInDuration > 0f
            ? Mathf.Clamp01(elapsed / fadeInDuration)
            : 1f;
        if (fadeOutDuration > 0f)
            alpha = Mathf.Min(alpha, Mathf.Clamp01(remaining / fadeOutDuration));

        Color color = effectRenderer.color;
        color.a = alpha;
        effectRenderer.color = color;
    }

    private void TryApplyContactDamage()
    {
        if (!damageEnabled || damageDealt || damageTarget == null ||
            damageTarget.isDead || Time.time < damageArmedAt)
            return;

        Vector3 targetPosition = damageTarget.transform.position;
        if (Mathf.Abs(targetPosition.x - transform.position.x) > damageRadius ||
            Mathf.Abs(targetPosition.y - transform.position.y) > damageVerticalTolerance)
            return;

        // The spell remains dangerous for its visible lifetime. Entering the
        // animated darkness after it is armed applies damage on that frame.
        damageDealt = true;
        damageTarget.TakeDamage(contactDamage);
        onDamageContact?.Invoke();
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (damageTarget != null &&
            (other.transform == damageTarget.transform ||
             other.transform.IsChildOf(damageTarget.transform)))
        {
            TryApplyContactDamage();
        }
    }

    private static void SetStateBoolIfPresent(Animator animator, string stateName, bool value)
    {
        foreach (AnimatorControllerParameter parameter in animator.parameters)
        {
            if (parameter.type == AnimatorControllerParameterType.Bool &&
                parameter.name == stateName)
            {
                animator.SetBool(parameter.nameHash, value);
                return;
            }
        }
    }
}
