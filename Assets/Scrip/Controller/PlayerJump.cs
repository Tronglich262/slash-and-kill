using UnityEngine;

public class PlayerJump : MonoBehaviour
{
    public float jumpForce = 20f;
    public float fallMultiplier = 2.5f;
    public float lowJumpMultiplier = 2f;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    public float coyoteTime = 0.2f;
    private float coyoteTimeCounter;

    public float jumpBufferTime = 0.2f;
    private float jumpBufferCounter;

    private Rigidbody2D rb;
    private Animator animator;
    private bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // Kiểm tra chạm đất
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        float verticalSpeed = rb.linearVelocity.y;
        animator.SetFloat("VerticalSpeed", verticalSpeed);

        // Đặt trạng thái Fall nếu đang rơi
        if (!isGrounded && verticalSpeed < -0.1f)
        {
            animator.SetBool("Fall", true);
        }
        else
        {
            animator.SetBool("Fall", false);
        }

        // Coyote time
        if (isGrounded)
            coyoteTimeCounter = coyoteTime;
        else
            coyoteTimeCounter -= Time.deltaTime;

        // Jump buffer
        if (Input.GetButtonDown("Jump"))
            jumpBufferCounter = jumpBufferTime;
        else
            jumpBufferCounter -= Time.deltaTime;

        // Thực hiện nhảy
        if (jumpBufferCounter > 0 && coyoteTimeCounter > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            animator.SetTrigger("Jump");
            jumpBufferCounter = 0;
        }
    }

    void FixedUpdate()
    {
        if (rb.linearVelocity.y < 0)
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
        }
        else if (rb.linearVelocity.y > 0 && !Input.GetButton("Jump"))
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.fixedDeltaTime;
        }
    }
}
