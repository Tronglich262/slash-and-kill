using UnityEngine;

/// <summary>
/// Lightweight, code-created telegraph for Bringer of Death's void attacks and portals.
/// It owns its temporary renderers and never needs a prefab in the scene.
/// </summary>
internal sealed class BossVoidRift2D : MonoBehaviour
{
    private static Sprite circleSprite;

    private float radius;
    private float warningTime;
    private float activeTime;
    private int damage;
    private HealthSystem target;
    private bool trackTargetDuringWarning;
    private float trackingDuration;
    private float startedAt;
    private bool damageApplied;
    private SpriteRenderer core;
    private SpriteRenderer ring;

    public static void SpawnAttack(
        Vector2 position,
        float radius,
        float warningTime,
        float activeTime,
        int damage,
        HealthSystem target)
    {
        Create(position, radius, warningTime, activeTime, damage, target, "Void Rift");
    }

    public static void SpawnTrackingAttack(
        Vector2 position,
        float radius,
        float warningTime,
        float activeTime,
        int damage,
        HealthSystem target)
    {
        Create(position, radius, warningTime, activeTime, damage, target, "Void Rift Tracking", true);
    }

    public static void SpawnPortal(Vector2 position, float radius)
    {
        Create(position, radius, 0.12f, 0.32f, 0, null, "Void Portal");
    }

    private static void Create(
        Vector2 position,
        float radius,
        float warningTime,
        float activeTime,
        int damage,
        HealthSystem target,
        string objectName,
        bool trackTarget = false)
    {
        GameObject riftObject = new GameObject(objectName);
        riftObject.transform.position = new Vector3(position.x, position.y, 0f);
        BossVoidRift2D rift = riftObject.AddComponent<BossVoidRift2D>();
        rift.Initialize(radius, warningTime, activeTime, damage, target, trackTarget);
    }

    private void Initialize(
        float requestedRadius,
        float requestedWarningTime,
        float requestedActiveTime,
        int requestedDamage,
        HealthSystem requestedTarget,
        bool requestedTrackTarget)
    {
        radius = Mathf.Max(0.25f, requestedRadius);
        warningTime = Mathf.Max(0.02f, requestedWarningTime);
        activeTime = Mathf.Max(0.05f, requestedActiveTime);
        damage = Mathf.Max(0, requestedDamage);
        target = requestedTarget;
        trackTargetDuringWarning = requestedTrackTarget && target != null;
        trackingDuration = Mathf.Min(0.42f, warningTime * 0.35f);
        startedAt = Time.time;

        Sprite sprite = GetCircleSprite();
        core = CreateRenderer("Core", sprite, new Color(0.16f, 0.01f, 0.28f, 0.16f), 45);
        ring = CreateRenderer("Ring", sprite, new Color(0.82f, 0.2f, 1f, 0.75f), 46);
        core.transform.localScale = Vector3.one * radius * 2f;
        ring.transform.localScale = Vector3.one * radius * 2.15f;
    }

    private SpriteRenderer CreateRenderer(string childName, Sprite sprite, Color color, int order)
    {
        GameObject child = new GameObject(childName);
        child.transform.SetParent(transform, false);
        SpriteRenderer renderer = child.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.color = color;
        renderer.sortingOrder = order;
        return renderer;
    }

    private void Update()
    {
        float elapsed = Time.time - startedAt;
        float totalTime = warningTime + activeTime;
        if (elapsed >= totalTime)
        {
            Destroy(gameObject);
            return;
        }

        if (trackTargetDuringWarning && elapsed < trackingDuration && target != null)
        {
            Vector3 targetPosition = target.transform.position;
            transform.position = new Vector3(targetPosition.x, targetPosition.y, 0f);
        }

        float warningProgress = Mathf.Clamp01(elapsed / warningTime);
        float pulse = 1f + Mathf.Sin(elapsed * 18f) * 0.07f;
        core.transform.localScale = Vector3.one * radius * 2f * Mathf.Lerp(0.2f, pulse, warningProgress);
        ring.transform.localScale = Vector3.one * radius * 2.15f * Mathf.Lerp(0.35f, pulse, warningProgress);
        ring.transform.Rotate(0f, 0f, 150f * Time.deltaTime);

        if (elapsed < warningTime)
            return;

        float fade = 1f - Mathf.InverseLerp(warningTime, totalTime, elapsed);
        SetAlpha(core, 0.34f * fade);
        SetAlpha(ring, 0.9f * fade);

        if (!damageApplied && damage > 0)
        {
            damageApplied = true;
            if (target != null &&
                Vector2.Distance(target.transform.position, transform.position) <= radius)
            {
                target.TakeDamage(damage);
                BossCameraShake2D.Shake(0.16f, 0.055f);
            }
        }
    }

    private static void SetAlpha(SpriteRenderer renderer, float alpha)
    {
        Color color = renderer.color;
        color.a = alpha;
        renderer.color = color;
    }

    private static Sprite GetCircleSprite()
    {
        if (circleSprite != null)
            return circleSprite;

        const int size = 64;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp
        };
        Color[] pixels = new Color[size * size];
        float center = (size - 1) * 0.5f;
        float maxDistance = center;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float normalizedDistance = Vector2.Distance(new Vector2(x, y), new Vector2(center, center)) / maxDistance;
                float alpha = Mathf.Clamp01(1f - normalizedDistance);
                pixels[y * size + x] = new Color(1f, 1f, 1f, alpha * alpha);
            }
        }

        texture.SetPixels(pixels);
        texture.Apply(false, true);
        circleSprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        return circleSprite;
    }
}
