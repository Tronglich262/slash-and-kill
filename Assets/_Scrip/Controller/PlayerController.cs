using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    public float speed = 5f;
    [Header("Combat Movement")]
    [Range(0.1f, 1f)] public float attackMoveMultiplier = 0.45f;
    [Min(0.05f)] public float attackMoveSlowDuration = 0.55f;
    private Rigidbody2D rb;
    private Animator animator;
    private static readonly int SpeedParameter = Animator.StringToHash("Speed");
    private bool facingRight = true;
    private bool canMove = true;
    private float moveInput;
    private float lastAnimatorSpeed = -1f;
    private float attackSlowUntil;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (!canMove) return;

        moveInput = Input.GetAxisRaw("Horizontal");

        // Basic attack remains available while moving. Instead of stopping the
        // player abruptly, it briefly reduces travel speed for readable weight.
        if (Input.GetKeyDown(KeyCode.Q))
            BeginAttackMovementSlow();

        SetAnimatorSpeed(Mathf.Abs(moveInput) * GetCurrentMovementMultiplier());
        if (moveInput > 0 && !facingRight)
            Flip();
        else if (moveInput < 0 && facingRight)
            Flip();
    }

    void FixedUpdate()
    {
        if (!canMove)
            return;

        rb.linearVelocity = new Vector2(
            moveInput * speed * GetCurrentMovementMultiplier(),
            rb.linearVelocity.y);
    }

    void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    private void SetAnimatorSpeed(float value)
    {
        if (Mathf.Approximately(lastAnimatorSpeed, value))
            return;

        lastAnimatorSpeed = value;
        animator.SetFloat(SpeedParameter, value);
    }

    public void SetCanMove(bool value)
    {
        canMove = value;
        if (!canMove)
        {
            moveInput = 0f;
            rb.linearVelocity = Vector2.zero;
            SetAnimatorSpeed(0f);
        }
    }

    public bool GetCanMove()
    {
        return canMove;
    }

    public void BeginAttackMovementSlow()
    {
        attackSlowUntil = Mathf.Max(attackSlowUntil, Time.time + attackMoveSlowDuration);
    }

    private float GetCurrentMovementMultiplier()
    {
        return Time.time < attackSlowUntil ? attackMoveMultiplier : 1f;
    }

    public void ResetStateForRevive()
    {
        moveInput = 0f;
        lastAnimatorSpeed = -1f;
        attackSlowUntil = 0f;
        SetCanMove(true);
        SetAnimatorSpeed(0f);
    }

}
