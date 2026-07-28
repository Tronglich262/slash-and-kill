using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator))]
public class PlayerJump : MonoBehaviour
{
    public float jumpForce = 20f;

   [SerializeField] public float fallMultiplier = 2.5f;
    [SerializeField] public float lowJumpMultiplier = 2f;

    [SerializeField] public Transform groundCheck;
    [SerializeField] public float groundCheckRadius = 0.2f;
    [Header("Robust Ground Check")]
    [SerializeField] private bool drawGroundRay = true;
    [SerializeField] public LayerMask groundLayer;
    [SerializeField, Range(0.1f, 1f)] private float minimumGroundNormalY = 0.45f;
    [SerializeField, Min(0.01f)] private float groundSkinDistance = 0.08f;
    [SerializeField, Min(0.02f)] private float groundProbeDistance = 0.18f;
    [SerializeField, Range(0f, 0.45f)] private float groundedAnimationGrace = 0.1f;

    [SerializeField] public float coyoteTime = 0.15f;
    [SerializeField] private float coyoteTimeCounter;

    [SerializeField] public float jumpBufferTime = 0.15f;
    private float jumpBufferCounter;

    private Rigidbody2D rb;
    private Collider2D playerCollider;
    private readonly ContactPoint2D[] groundContacts = new ContactPoint2D[12];
    private readonly RaycastHit2D[] groundCastHits = new RaycastHit2D[8];
    private ContactFilter2D groundContactFilter;
    private Animator animator;
    private LadderClimb ladderClimb;
    private static readonly int VerticalSpeedParameter = Animator.StringToHash("VerticalSpeed");
    private static readonly int IsJumpingParameter = Animator.StringToHash("IsJumping");
    private static readonly int FallParameter = Animator.StringToHash("Fall");
    private bool jumpingAnimationState;
    private bool fallAnimationState;

    private bool isGrounded;
    private bool inputLocked;
    private float lastRawGroundedTime = float.NegativeInfinity;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<Collider2D>();
        groundContactFilter = new ContactFilter2D();
        groundContactFilter.SetLayerMask(groundLayer);
        groundContactFilter.useTriggers = false;
        animator = GetComponent<Animator>();
        ladderClimb = GetComponent<LadderClimb>();
    }

    void Update()
    {
        if (inputLocked)
            jumpBufferCounter = 0f;

        if (ladderClimb != null && ladderClimb.isClimbing)
        {
            coyoteTimeCounter = 0f;
            jumpBufferCounter = 0f;
            SetJumpingAnimation(false);
            SetFallAnimation(false);
            return;
        }

        // Probe the real feet, not just a single contact frame. This never moves the player.
        bool rawGrounded = CheckGrounded();
        float verticalSpeed = rb.linearVelocity.y;
        if (rawGrounded)
            lastRawGroundedTime = Time.time;

        // Brief grace prevents an Idle/Fall flicker over seams between map colliders.
        isGrounded = rawGrounded || (verticalSpeed <= 0.05f && Time.time - lastRawGroundedTime <= groundedAnimationGrace);

        animator.SetFloat(VerticalSpeedParameter, verticalSpeed);
        SetJumpingAnimation(!isGrounded);
        SetFallAnimation(!isGrounded && verticalSpeed < -0.1f);

        if (isGrounded)
        {
            animator.ResetTrigger("Jump");
            animator.SetFloat(VerticalSpeedParameter, 0f);
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

        // Jump logic
        if (jumpBufferCounter > 0 && coyoteTimeCounter > 0)
        {
            Jump();
            jumpBufferCounter = 0;
        }
    }

    private bool CheckGrounded()
    {
        if (playerCollider == null || groundLayer.value == 0)
            return false;

        groundContactFilter.SetLayerMask(groundLayer);
        int contactCount = playerCollider.GetContacts(groundContactFilter, groundContacts);
        for (int i = 0; i < contactCount; i++)
        {
            if (groundContacts[i].normal.y >= minimumGroundNormalY)
                return true;
        }

        float probeLength = groundSkinDistance + groundProbeDistance;
        int hitCount = playerCollider.Cast(Vector2.down, groundContactFilter, groundCastHits, probeLength);
        for (int i = 0; i < hitCount; i++)
        {
            if (groundCastHits[i].collider != null && groundCastHits[i].normal.y >= minimumGroundNormalY)
                return true;
        }

        // Tilemap seams can fall between contact frames. Sample left, centre and right feet.
        Bounds bounds = playerCollider.bounds;
        float inset = Mathf.Min(bounds.extents.x * 0.65f, 0.08f);
        Vector2[] origins =
        {
            new Vector2(bounds.center.x - inset, bounds.min.y + 0.025f),
            new Vector2(bounds.center.x, bounds.min.y + 0.025f),
            new Vector2(bounds.center.x + inset, bounds.min.y + 0.025f)
        };

        for (int i = 0; i < origins.Length; i++)
        {
            RaycastHit2D hit = Physics2D.Raycast(origins[i], Vector2.down, probeLength, groundLayer);
            if (hit.collider != null && hit.normal.y >= minimumGroundNormalY)
                return true;
        }

        // Existing GroundCheck child is now a real fallback sensor as well.
        if (groundCheck != null)
        {
            RaycastHit2D hit = Physics2D.Raycast(groundCheck.position, Vector2.down, probeLength, groundLayer);
            if (hit.collider != null && hit.normal.y >= minimumGroundNormalY)
                return true;
        }

        if (drawGroundRay)
        {
            for (int i = 0; i < origins.Length; i++)
                Debug.DrawRay(origins[i], Vector2.down * probeLength, Color.red);
        }
        return false;
    }

    void FixedUpdate()
    {
        if (inputLocked)
            return;

        if (ladderClimb != null && ladderClimb.isClimbing)
            return;

        // Fall faster
        if (rb.linearVelocity.y < 0)
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y *
                (fallMultiplier - 1) * Time.fixedDeltaTime;
        }
        // Low jump when release early
        else if (rb.linearVelocity.y > 0 && !Input.GetButton("Jump"))
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y *
                (lowJumpMultiplier - 1) * Time.fixedDeltaTime;
        }
    }

private void OnDrawGizmosSelected()
    {
        if (!drawGroundRay) return;
        Collider2D col = playerCollider != null ? playerCollider : GetComponent<Collider2D>();
        if (col == null) return;
        Bounds bounds = col.bounds;
        Vector3 origin = new Vector3(bounds.center.x, bounds.min.y + 0.03f, transform.position.z);
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawLine(origin, origin + Vector3.down * (groundSkinDistance));
        Gizmos.DrawWireSphere(origin + Vector3.down * (groundSkinDistance), 0.035f);
    }

    void Jump()
    {
        coyoteTimeCounter = 0;

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        animator.SetTrigger("Jump");
    }

    public void SetInputLocked(bool locked)
    {
        inputLocked = locked;
        if (locked)
        {
            jumpBufferCounter = 0f;
            coyoteTimeCounter = 0f;
        }
    }

    private void SetJumpingAnimation(bool value)
    {
        jumpingAnimationState = value;
        if (animator != null) animator.SetBool(IsJumpingParameter, value);
    }

    private void SetFallAnimation(bool value)
    {
        fallAnimationState = value;
        if (animator != null) animator.SetBool(FallParameter, value);
    }

    public void ForceIdleAnimation()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        if (animator == null)
            return;

        animator.ResetTrigger("Jump");
        animator.SetBool(IsJumpingParameter, false);
        animator.SetBool(FallParameter, false);
        animator.SetFloat(VerticalSpeedParameter, 0f);
        jumpingAnimationState = false;
        fallAnimationState = false;
    }
    public void ResetStateForRevive()
    {
        coyoteTimeCounter = 0f;
        jumpBufferCounter = 0f;
        isGrounded = false;

        if (animator == null)
            return;

        jumpingAnimationState = true;
        fallAnimationState = true;
        SetJumpingAnimation(false);
        SetFallAnimation(false);
        animator.SetFloat(VerticalSpeedParameter, 0f);
        animator.ResetTrigger("Jump");
    }
}
