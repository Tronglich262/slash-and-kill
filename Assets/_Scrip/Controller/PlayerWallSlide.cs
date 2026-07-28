using UnityEngine;
using System.Collections;

public class PlayerWallSlide : MonoBehaviour
{
    public Transform groundCheck;
    public Transform wallCheckLeft;
    public Transform wallCheckRight;
    public Transform ledgeCheck;

    public LayerMask groundLayer;
    public LayerMask wallLayer;

    public float wallSlideSpeed = 2f;

    private Animator animator;
    private Rigidbody2D rb;
    private LadderClimb ladderClimb;

    private bool isGrounded;
    private bool isTouchingWall;
    private bool isClimbing;

    private float defaultGravity;

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        ladderClimb = GetComponent<LadderClimb>();
        defaultGravity = rb.gravityScale;
    }

    void Update()
    {
        if (isClimbing || (ladderClimb != null && ladderClimb.isClimbing))
            return;

        CheckGrounded();
        CheckWallInteraction();

        if (isTouchingWall && !isGrounded &&
            Input.GetKey(KeyCode.LeftShift) &&
            Input.GetKeyDown(KeyCode.Space))
        {
            TryClimbUp();
        }
    }

    void CheckGrounded()
    {
        isGrounded = Physics2D.Raycast(
            groundCheck.position,
            Vector2.down,
            0.1f,
            groundLayer
        );
    }

    void CheckWallInteraction()
    {
        bool leftTouchingWall = Physics2D.Raycast(
            wallCheckLeft.position,
            Vector2.left,
            0.1f,
            wallLayer
        );

        bool rightTouchingWall = Physics2D.Raycast(
            wallCheckRight.position,
            Vector2.right,
            0.1f,
            wallLayer
        );

        isTouchingWall = leftTouchingWall || rightTouchingWall;

        bool isHoldingShift = Input.GetKey(KeyCode.LeftShift);

        if (isTouchingWall && !isGrounded)
        {
            if (isHoldingShift)
            {
                animator.SetBool("iswallidle", true);
                animator.SetBool("isWallSliding", false);

                rb.linearVelocity = Vector2.zero;
                rb.gravityScale = 0;
            }
            else if (rb.linearVelocity.y < 0)
            {
                animator.SetBool("iswallidle", false);
                animator.SetBool("isWallSliding", true);

                rb.gravityScale = defaultGravity;
                rb.linearVelocity = new Vector2(
                    rb.linearVelocity.x,
                    -wallSlideSpeed
                );
            }
            else
            {
                ResetWallState();
            }
        }
        else
        {
            ResetWallState();
        }
    }

    void ResetWallState()
    {
        animator.SetBool("iswallidle", false);
        animator.SetBool("isWallSliding", false);
        rb.gravityScale = defaultGravity;
    }

    public void ResetStateForRevive()
    {
        StopAllCoroutines();
        isClimbing = false;
        isGrounded = false;
        isTouchingWall = false;

        if (animator != null)
        {
            animator.SetBool("iswallidle", false);
            animator.SetBool("isWallSliding", false);
            animator.SetBool("iswallgrab", false);
        }

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.gravityScale = defaultGravity;
        }
    }

    private void OnDisable()
    {
        ResetStateForRevive();
    }

    void TryClimbUp()
    {
        Vector2 dir = transform.localScale.x > 0
            ? new Vector2(1f, 1f)
            : new Vector2(-1f, 1f);

        bool wallAbove = Physics2D.Raycast(
            ledgeCheck.position,
            dir,
            0.3f,
            wallLayer
        );

        if (!wallAbove)
        {
            StartCoroutine(ClimbRoutine());
        }
    }

    IEnumerator ClimbRoutine()
    {
        isClimbing = true;

        animator.SetBool("iswallidle", false);
        animator.SetBool("iswallgrab", true);

        rb.gravityScale = 0;
        rb.linearVelocity = Vector2.zero;

        yield return new WaitForSeconds(0.15f);

        rb.gravityScale = defaultGravity;

        rb.linearVelocity = new Vector2(
            transform.localScale.x > 0 ? 8f : -8f,
            14f
        );

        yield return new WaitForSeconds(0.2f);

        animator.SetBool("iswallgrab", false);

        isClimbing = false;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        if (wallCheckLeft != null)
            Gizmos.DrawRay(wallCheckLeft.position, Vector2.left * 0.1f);

        if (wallCheckRight != null)
            Gizmos.DrawRay(wallCheckRight.position, Vector2.right * 0.1f);

        if (ledgeCheck != null)
        {
            Vector2 dir = transform.localScale.x > 0
                ? new Vector2(1f, 1f)
                : new Vector2(-1f, 1f);

            Gizmos.DrawRay(ledgeCheck.position, dir * 0.3f);
        }
    }
}
