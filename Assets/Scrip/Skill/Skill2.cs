using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Skill2 : MonoBehaviour
{
    public GameObject skillPrefab; // Kéo thả prefab skill vào đây
    public Transform player; // Kéo thả transform của nhân vật vào đây
    public Button skillButton; // Kéo thả button vào đây
    public Image cooldownBar; // Kéo thả hình ảnh thanh hồi chiêu vào đây
    public float cooldownTime = 5f; // Thời gian hồi chiêu
    private bool isOnCooldown = false; // Kiểm tra trạng thái hồi chiêu
    private Animator playerAnimator;

    void Start()
    {
        if (player != null)
        {
            playerAnimator = player.GetComponent<Animator>();
        }

        skillButton.onClick.AddListener(UseSkill);
        cooldownBar.fillAmount = 0; 
    }

    void Update()
    {

        if (Input.GetKeyDown(KeyCode.Alpha2) && !isOnCooldown)
        {
            UseSkill(); 
        }
    }

    void UseSkill()
    {
        if (skillPrefab != null && player != null && playerAnimator != null && !isOnCooldown)
        {
            isOnCooldown = true; 
            skillButton.interactable = false;
            cooldownBar.fillAmount = 1; // Bắt đầu hiện thanh hồi chiêu

            playerAnimator.SetBool("Attackskill", true);
            StartCoroutine(SkilltimeDeley());
            StartCoroutine(ResetAttackSkill());
            StartCoroutine(Cooldown()); // Bắt đầu hồi chiêu
        }
    }

    IEnumerator SkilltimeDeley()
    {
        float offsetX = 3f;
        float direction = Mathf.Sign(player.localScale.x);

        Collider2D playerCollider = player.GetComponent<Collider2D>();
        if (playerCollider == null)
        {
            Debug.LogError("Player không có Collider2D!");
            yield break;
        }

        float footY = playerCollider.bounds.min.y + 1f;
        Vector3 spawnPosition = new Vector3(player.position.x + (direction * offsetX), footY, player.position.z);

        GameObject spawnedSkill = Instantiate(skillPrefab, spawnPosition, Quaternion.identity);

        Vector3 skillScale = spawnedSkill.transform.localScale;
        skillScale.x = Mathf.Abs(skillScale.x) * direction;
        spawnedSkill.transform.localScale = skillScale;

        yield return new WaitForSecondsRealtime(2f);

        Destroy(spawnedSkill);
    }

    IEnumerator ResetAttackSkill()
    {
        yield return new WaitForSeconds(0.5f); 
        if (playerAnimator != null)
        {
            playerAnimator.SetBool("Attackskill", false);
        }
    }

    IEnumerator Cooldown()
    {
        float elapsedTime = 0f;

        while (elapsedTime < cooldownTime)
        {
            elapsedTime += Time.deltaTime;
            cooldownBar.fillAmount = 1 - (elapsedTime / cooldownTime); // Giảm dần thanh hồi chiêu
            yield return null;
        }

        isOnCooldown = false;
        skillButton.interactable = true;
        cooldownBar.fillAmount = 0; // Ẩn thanh hồi chiêu khi sẵn sàng
    }
}
