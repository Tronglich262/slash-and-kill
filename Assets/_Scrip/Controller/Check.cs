using UnityEngine;
using UnityEngine.UI;


public class Check : MonoBehaviour
{
    public Slider healthSlider;
    [SerializeField] public float detectRadius = 2f;

    void Update()
    {
        CheckEnemyDistance();
    }

    void CheckEnemyDistance()
    {
        Collider2D enemy = Physics2D.OverlapCircle(transform.position, detectRadius, LayerMask.GetMask("Enemy"));
        if (healthSlider != null)
        {
            healthSlider.gameObject.SetActive(enemy != null); 
        }
    }

    void OnDrawGizmosSelected()
    {
        //check enemy
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectRadius);
    }
}