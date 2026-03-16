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
        // check chân bằng raycast
        bool leftGrounded = Physics2D.Raycast(groundCheckLeft.position, Vector2.down, 0.1f, groundLayer);
        bool rightGrounded = Physics2D.Raycast(groundCheckRight.position, Vector2.down, 0.1f, groundLayer);

        isEdgeIdle = (leftGrounded ^ rightGrounded); 

        animator.SetBool("isEdgeIdle", isEdgeIdle);

        if (isEdgeIdle)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }

        Debug.DrawRay(groundCheckLeft.position, Vector2.down * 0.1f, Color.red);
        Debug.DrawRay(groundCheckRight.position, Vector2.down * 0.1f, Color.blue);
    }
}