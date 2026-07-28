using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class DamageText : MonoBehaviour
{
    private static readonly Stack<DamageText> Pool = new Stack<DamageText>();

    public TextMeshProUGUI damageText;
    private Transform enemyTransform;
    private Vector3 offset = new Vector3(0, 0f, 0); 
    private float moveSpeed = 1f; 
    private float fadeSpeed = 2f;
    private float lifetime = 1f;
    private float timer;
    private CanvasGroup canvasGroup;
    private Camera mainCamera;
    private bool isActive;

    private void Awake()
    {
        if (!TryGetComponent(out canvasGroup))
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public static DamageText Spawn(GameObject prefab, Transform parent)
    {
        DamageText instance = null;

        while (Pool.Count > 0 && instance == null)
            instance = Pool.Pop();

        if (instance == null)
        {
            GameObject obj = Instantiate(prefab, parent);
            instance = obj.GetComponent<DamageText>();
        }
        else
        {
            instance.transform.SetParent(parent, false);
            instance.gameObject.SetActive(true);
        }

        return instance;
    }

    public void Setup(int damage, Transform enemy, bool isCritical = false)
    {
        if (isCritical)
        {
            damageText.text = "-" + damage.ToString() + " CRIT! "; 
            damageText.color = Color.yellow; 
            damageText.fontSize = 70; 
        }
        else
        {
            damageText.text = "-" + damage.ToString();
            damageText.color = Color.red; 
            damageText.fontSize = 50; 
        }
        enemyTransform = enemy;
        offset = Vector3.zero;
        timer = 0f;
        canvasGroup.alpha = 1f;
        mainCamera = Camera.main;
        isActive = true;
    }

    void Update()
    {
        if (!isActive)
            return;

        if (enemyTransform != null)
        {
            if (mainCamera == null)
                mainCamera = Camera.main;

            if (mainCamera != null)
            {
                Vector3 screenPosition = mainCamera.WorldToScreenPoint(enemyTransform.position + offset);
                transform.position = screenPosition;
            }
        }

        timer += Time.deltaTime;
        offset.y += moveSpeed * Time.deltaTime;
        canvasGroup.alpha = Mathf.Clamp01(canvasGroup.alpha - fadeSpeed * Time.deltaTime);

        if (timer >= lifetime)
            Release();
    }

    private void Release()
    {
        isActive = false;
        enemyTransform = null;
        gameObject.SetActive(false);
        Pool.Push(this);
    }
}
