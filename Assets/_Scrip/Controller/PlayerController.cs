using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    public float speed = 5f;
    private Rigidbody2D rb;
    private Animator animator;
    private bool facingRight = true;
    private bool canMove = true;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (!canMove) return;

        float moveX = Input.GetAxisRaw("Horizontal");

        animator.SetFloat("Speed", Mathf.Abs(moveX));
        if (moveX > 0 && !facingRight)
            Flip();
        else if (moveX < 0 && facingRight)
            Flip();
        rb.linearVelocity = new Vector2(moveX * speed, rb.linearVelocity.y);
    }

    void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    public void SetCanMove(bool value)
    {
        canMove = value;
        if (!canMove)
        {
            rb.linearVelocity = Vector2.zero;
            animator.SetFloat("Speed", 0);
        }
    }

    public bool GetCanMove()
    {
        return canMove;
    }

}
