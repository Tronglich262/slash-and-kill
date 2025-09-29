using UnityEngine;

public class PlayerWallSlide : MonoBehaviour
{
    public Transform groundCheck;
    public Transform wallCheckLeft;
    public Transform wallCheckRight;
    public LayerMask groundLayer;
    public LayerMask wallLayer;

    private Animator animator;
    private Rigidbody2D rb;
    private bool isGrounded;
    private bool isTouchingWall;
    private bool isWallSliding;
    public float wallSlideSpeed = 2f; 

    private void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        CheckGrounded();
        CheckWallSliding();
    }

    void CheckGrounded()
    {
      
        isGrounded = Physics2D.Raycast(groundCheck.position, Vector2.down, 0.1f, groundLayer);
    }

    void CheckWallSliding()
    {
        
        bool leftTouchingWall = Physics2D.Raycast(wallCheckLeft.position, Vector2.left, 0.1f, wallLayer);
        bool rightTouchingWall = Physics2D.Raycast(wallCheckRight.position, Vector2.right, 0.1f, wallLayer);
        isTouchingWall = leftTouchingWall || rightTouchingWall;

       
        if (isTouchingWall && !isGrounded && rb.linearVelocity.y < 0)
        {
            isWallSliding = true;
            animator.SetBool("isWallSliding", true);
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, -wallSlideSpeed); // Điều chỉnh tốc độ trượt
        }
        else
        {
            isWallSliding = false;
            animator.SetBool("isWallSliding", false);
        }
    }

    // Debug Ray
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(wallCheckLeft.position, Vector2.left * 0.1f);
        Gizmos.DrawRay(wallCheckRight.position, Vector2.right * 0.1f);
    }
}