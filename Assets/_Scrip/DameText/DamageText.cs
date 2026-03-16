using UnityEngine;
using TMPro;

public class DamageText : MonoBehaviour
{
    public TextMeshProUGUI damageText;
    private Transform enemyTransform;
    private Vector3 offset = new Vector3(0, 0f, 0); 
    private float moveSpeed = 1f; 
    private float fadeSpeed = 2f;
    private CanvasGroup canvasGroup; 

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

        canvasGroup = gameObject.AddComponent<CanvasGroup>();//làm mờ
        Destroy(gameObject, 1f); 
    }

    void Update()
    {
        if (enemyTransform != null)
        {
            Vector3 screenPosition = Camera.main.WorldToScreenPoint(enemyTransform.position + offset);
            transform.position = screenPosition;
        }
        offset.y += moveSpeed * Time.deltaTime;
        if (canvasGroup != null)
        {
            canvasGroup.alpha -= fadeSpeed * Time.deltaTime;
        }
    }
}
