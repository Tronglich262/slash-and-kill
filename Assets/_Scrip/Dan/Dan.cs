using UnityEngine;
using System.Collections.Generic;

public class Dan : MonoBehaviour
{
    private static readonly Dictionary<GameObject, Stack<Dan>> Pools =
        new Dictionary<GameObject, Stack<Dan>>();

    private Rigidbody2D rb;
    private GameObject sourcePrefab;
    private bool isDestroying = false;
    public float speed = 5f;
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private int damage = 10;
    private float releaseTime;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        // Fallback for a projectile placed directly in a scene.
        if (releaseTime <= 0f)
            Activate(transform.position, Vector2.down * speed, damage);
    }

    private void Update()
    {
        if (!isDestroying && Time.time >= releaseTime)
            Release();
    }

    public static GameObject Spawn(GameObject prefab, Vector3 position, Vector2 velocity)
    {
        return Spawn(prefab, position, velocity, 10);
    }

    public static GameObject Spawn(
        GameObject prefab,
        Vector3 position,
        Vector2 velocity,
        int projectileDamage)
    {
        if (prefab == null)
            return null;

        if (!Pools.TryGetValue(prefab, out Stack<Dan> pool))
        {
            pool = new Stack<Dan>();
            Pools.Add(prefab, pool);
        }

        Dan projectile = null;
        while (pool.Count > 0 && projectile == null)
            projectile = pool.Pop();

        if (projectile == null)
        {
            GameObject obj = Instantiate(prefab, position, Quaternion.identity);
            projectile = obj.GetComponent<Dan>();
            if (projectile == null)
                return obj;
        }
        else
        {
            projectile.transform.SetPositionAndRotation(position, Quaternion.identity);
            projectile.gameObject.SetActive(true);
        }

        projectile.sourcePrefab = prefab;
        projectile.Activate(position, velocity, projectileDamage);
        return projectile.gameObject;
    }

    private void Activate(Vector3 position, Vector2 velocity, int projectileDamage)
    {
        transform.position = position;
        isDestroying = false;
        damage = Mathf.Max(0, projectileDamage);
        releaseTime = Time.time + Mathf.Max(0.1f, lifetime);

        if (rb != null)
        {
            rb.linearVelocity = velocity;
            rb.angularVelocity = 0f;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isDestroying) return;

        HealthSystem healthSystem = other.GetComponentInParent<HealthSystem>();
        if (healthSystem == null && other.CompareTag("Player"))
            healthSystem = HealthSystem.Instance;

        if (healthSystem != null)
        {
            healthSystem.TakeDamage(damage);
            Release();
        }
    }

    private void Release()
    {
        if (isDestroying)
            return;

        isDestroying = true;

        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        if (sourcePrefab == null)
        {
            Destroy(gameObject);
            return;
        }

        gameObject.SetActive(false);
        Pools[sourcePrefab].Push(this);
    }
}
