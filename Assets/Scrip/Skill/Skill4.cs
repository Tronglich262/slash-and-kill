using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Skill4 : MonoBehaviour
{
    public GameObject skillPrefab; // Prefab kỹ năng
    public Transform player; // Nhân vật
    public Button skillButton; // Nút kích hoạt chiêu
    public Image cooldownPanel; // Panel hiển thị hồi chiêu
    public float cooldownTime = 5f; // Thời gian hồi chiêu

    private bool isCooldown = false; // Kiểm tra xem chiêu đang hồi hay không

    void Start()
    {
        skillButton.onClick.AddListener(UseSkill); // Lắng nghe sự kiện bấm button
        cooldownPanel.fillAmount = 0; // Ban đầu không hiển thị
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha4) && !isCooldown) // Kiểm tra phím số 4
        {
            UseSkill();
        }
    }

    void UseSkill()
    {
        if (skillPrefab != null && player != null && !isCooldown)
        {
            StartCoroutine(SkilltimeDeley());
            StartCoroutine(StartCooldown());
        }
    }

    IEnumerator SkilltimeDeley()
    {
        float offset = 0f;
        float direction = player.localScale.x >= 0 ? 1f : -1f;

        GameObject spawnedSkill = Instantiate(skillPrefab, player.position + new Vector3(direction * offset, 0.4f, 0), Quaternion.identity);

        float duration = 10f;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            // Cập nhật vị trí của skill theo nhân vật
            spawnedSkill.transform.position = player.position + new Vector3(direction * offset, 0, 0);

            elapsedTime += Time.deltaTime;
            yield return null; // Đợi frame tiếp theo
        }

        Destroy(spawnedSkill);
    }

    IEnumerator StartCooldown()
    {
        isCooldown = true;
        skillButton.interactable = false; // Tắt nút khi hồi chiêu
        cooldownPanel.fillAmount = 1; // Bắt đầu hiển thị panel

        float elapsed = 0f;
        while (elapsed < cooldownTime)
        {
            elapsed += Time.deltaTime;
            cooldownPanel.fillAmount = 1 - (elapsed / cooldownTime); // Giảm dần panel
            yield return null;
        }

        cooldownPanel.fillAmount = 0; // Ẩn panel khi hồi xong
        skillButton.interactable = true; // Bật lại nút
        isCooldown = false;
    }
}
