using UnityEngine;

public class PlayerEdgeIdle : MonoBehaviour
{
    public Transform groundCheckLeft;
    public Transform groundCheckRight;
    public LayerMask groundLayer;

    private Animator animator;

    private Rigidbody2D rb;
    private bool isEdgeIdle;

    private void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        CheckEdgeIdle();
    }

    void CheckEdgeIdle()
    {
        // Raycast từ mỗi chân để kiểm tra có đứng trên nền không
        bool leftGrounded = Physics2D.Raycast(groundCheckLeft.position, Vector2.down, 0.1f, groundLayer);
        bool rightGrounded = Physics2D.Raycast(groundCheckRight.position, Vector2.down, 0.1f, groundLayer);

        // Nếu chỉ 1 bên chạm đất → đang đứng ở mép
        isEdgeIdle = (leftGrounded ^ rightGrounded); // XOR

        // Cập nhật Animator
        animator.SetBool("isEdgeIdle", isEdgeIdle);

        // (Tùy chọn) Ngăn trượt mép
        if (isEdgeIdle)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }

        // Debug Ray
        Debug.DrawRay(groundCheckLeft.position, Vector2.down * 0.1f, Color.red);
        Debug.DrawRay(groundCheckRight.position, Vector2.down * 0.1f, Color.blue);
    }
}