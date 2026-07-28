using UnityEngine;

public class PlayerEdgeIdle : MonoBehaviour
{
    public Transform groundCheckLeft;
    public Transform groundCheckRight;
    public LayerMask groundLayer;

    private Animator animator;
    private static readonly int EdgeIdleParameter = Animator.StringToHash("isEdgeIdle");

    private Rigidbody2D rb;
    [SerializeField] private float checkInterval = 0.05f;
    private float nextCheckTime;
    private bool isEdgeIdle;

    private void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (Time.time < nextCheckTime)
            return;

        nextCheckTime = Time.time + checkInterval;
        CheckEdgeIdle();
    }

    void CheckEdgeIdle()
    {
        // check chân bằng raycast
        bool leftGrounded = Physics2D.Raycast(groundCheckLeft.position, Vector2.down, 0.1f, groundLayer);
        bool rightGrounded = Physics2D.Raycast(groundCheckRight.position, Vector2.down, 0.1f, groundLayer);

        bool newEdgeIdle = leftGrounded ^ rightGrounded;
        if (newEdgeIdle != isEdgeIdle)
        {
            isEdgeIdle = newEdgeIdle;
            animator.SetBool(EdgeIdleParameter, isEdgeIdle);
        }

        if (isEdgeIdle)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }

#if UNITY_EDITOR
        Debug.DrawRay(groundCheckLeft.position, Vector2.down * 0.1f, Color.red);
        Debug.DrawRay(groundCheckRight.position, Vector2.down * 0.1f, Color.blue);
#endif
    }

    public void ResetStateForRevive()
    {
        isEdgeIdle = false;
        nextCheckTime = 0f;
        if (animator != null)
            animator.SetBool(EdgeIdleParameter, false);
    }

    private void OnDisable()
    {
        ResetStateForRevive();
    }
}
