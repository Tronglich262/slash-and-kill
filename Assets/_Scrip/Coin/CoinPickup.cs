using System.Collections.Generic;
using UnityEngine;

public class CoinPickup : MonoBehaviour
{
    private static readonly Dictionary<GameObject, Stack<CoinPickup>> Pools =
        new Dictionary<GameObject, Stack<CoinPickup>>();

    public int coinValue = 10; 
    private GameObject sourcePrefab;
    private Rigidbody2D cachedRigidbody;
    private bool claimed;

    private void Awake()
    {
        cachedRigidbody = GetComponent<Rigidbody2D>();
    }

    public static CoinPickup Spawn(GameObject prefab, Vector3 position, int value)
    {
        if (prefab == null)
            return null;

        if (!Pools.TryGetValue(prefab, out Stack<CoinPickup> pool))
        {
            pool = new Stack<CoinPickup>();
            Pools.Add(prefab, pool);
        }

        CoinPickup pickup = null;
        while (pool.Count > 0 && pickup == null)
            pickup = pool.Pop();

        if (pickup == null)
        {
            GameObject instance = Instantiate(prefab, position, Quaternion.identity);
            pickup = instance.GetComponent<CoinPickup>();
            if (pickup == null)
            {
                Destroy(instance);
                Debug.LogWarning($"Coin prefab '{prefab.name}' has no CoinPickup component.");
                return null;
            }

            pickup.sourcePrefab = prefab;
        }
        else
        {
            pickup.transform.SetPositionAndRotation(position, Quaternion.identity);
            pickup.gameObject.SetActive(true);
        }

        pickup.coinValue = value;
        pickup.claimed = false;
        if (pickup.cachedRigidbody != null)
        {
            pickup.cachedRigidbody.linearVelocity = Vector2.zero;
            pickup.cachedRigidbody.angularVelocity = 0f;
        }

        return pickup;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!claimed && other.CompareTag("Player"))
        {
            claimed = true;

            if (FloatingTextManager.Instance != null)
            {
                FloatingTextManager.Instance.ShowGold(coinValue);
            }

            if (CoinManager.Instance != null)
                CoinManager.Instance.AddCoin(coinValue);

            Release();
        }
    }

    private void Release()
    {
        if (sourcePrefab == null)
        {
            Destroy(gameObject);
            return;
        }

        if (cachedRigidbody != null)
        {
            cachedRigidbody.linearVelocity = Vector2.zero;
            cachedRigidbody.angularVelocity = 0f;
        }

        gameObject.SetActive(false);
        Pools[sourcePrefab].Push(this);
    }
}
