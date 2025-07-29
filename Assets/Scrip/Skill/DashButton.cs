using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DashButton : MonoBehaviour
{
    public GameObject player; // Gán Player vào Inspector
    public float dashDistance = 4f; // Khoảng cách Dash
    public float dashDuration = 0.2f; // Thời gian Dash
    public float cooldownTime = 1.5f; // Thời gian hồi chiêu
    public Button dashButton; // Button UI để Dash
    public Image cooldownPanel; // Hình ảnh hiển thị cooldown (Panel mờ dần)

    private Animator animator;
    private Rigidbody2D rb;
    private bool isDashing = false;
    private bool isCooldown = false;

    void Start()
    {
        if (player != null)
        {
            animator = player.GetComponent<Animator>();
            rb = player.GetComponent<Rigidbody2D>();
        }

        if (dashButton != null)
        {
            dashButton.onClick.AddListener(StartDash);
        }

        if (cooldownPanel != null)
        {
            cooldownPanel.fillAmount = 0; // Bắt đầu với cooldown ẩn
        }
    }

    private void Update()
    {
        if (!isDashing && !isCooldown)
        {
            if (Input.GetKeyDown(KeyCode.E)) // Kiểm tra phím E
            {
                StartDash();
            }
        }
    }

    public void StartDash()
    {
        if (!isDashing && !isCooldown && player != null)
        {
            StartCoroutine(Dash());
        }
    }

    IEnumerator Dash()
    {
        isDashing = true;
        isCooldown = true;
        dashButton.interactable = false; // Vô hiệu hóa nút Dash
        animator.SetBool("Dash", true); // Bật animation Dash

        float dashDirection = player.transform.localScale.x >= 0 ? 1f : -1f;
        Vector2 startPosition = rb.position;
        Vector2 targetPosition = startPosition + new Vector2(dashDistance * dashDirection, 0);

        float elapsedTime = 0f;
        while (elapsedTime < dashDuration)
        {
            rb.MovePosition(Vector2.Lerp(startPosition, targetPosition, elapsedTime / dashDuration));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        rb.MovePosition(targetPosition);
        animator.SetBool("Dash", false);
        isDashing = false;

        // Bắt đầu hồi chiêu
        StartCoroutine(DashCooldown());
    }

    IEnumerator DashCooldown()
    {
        float cooldownProgress = 0f;

        while (cooldownProgress < cooldownTime)
        {
            cooldownProgress += Time.deltaTime;
            if (cooldownPanel != null)
            {
                cooldownPanel.fillAmount = 1 - (cooldownProgress / cooldownTime); // Hiển thị cooldown giảm dần
            }
            yield return null;
        }

        if (cooldownPanel != null)
        {
            cooldownPanel.fillAmount = 0; // Ẩn cooldown khi kết thúc
        }

        isCooldown = false;
        dashButton.interactable = true; // Kích hoạt lại nút Dash
    }
}
