using UnityEngine;

public class LadderClimb : MonoBehaviour
{
    public float climbSpeed = 4f;

    private bool isClimbing = false;
    private float inputVertical;
    private Rigidbody2D rb;
    private Animator animator;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (isClimbing)
        {
            inputVertical = Input.GetAxisRaw("Vertical");
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, inputVertical * climbSpeed);
            rb.gravityScale = 0;

            animator.SetBool("Ladder", Mathf.Abs(inputVertical) > 0);
        }
        else
        {
            rb.gravityScale = 1;
            animator.SetBool("Ladder", false);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Ladder"))
        {
            isClimbing = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Ladder"))
        {
            isClimbing = false;
        }
    }
}