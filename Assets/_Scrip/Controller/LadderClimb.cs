using UnityEngine;

public class LadderClimb : MonoBehaviour
{
    public float climbSpeed = 4f;

    public bool isClimbing = false;
    private float inputVertical;
    private Rigidbody2D rb;
    private Animator animator;
    private static readonly int LadderParameter = Animator.StringToHash("Ladder");
    private bool ladderAnimationState;
    private float defaultGravity;
    private readonly System.Collections.Generic.HashSet<Collider2D> ladderContacts =
        new System.Collections.Generic.HashSet<Collider2D>();
    public static LadderClimb instance;
    public void Awake()
    {
        if (instance != null && instance != this)
        {
            enabled = false;
            return;
        }

        instance = this;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        defaultGravity = rb.gravityScale;
    }

    void Update()
    {
        if (isClimbing)
        {
            inputVertical = Input.GetAxisRaw("Vertical");
            SetLadderAnimation(Mathf.Abs(inputVertical) > 0);
        }
        else
        {
            inputVertical = 0f;
            SetLadderAnimation(false);
        }
    }

    private void SetLadderAnimation(bool value)
    {
        if (ladderAnimationState == value)
            return;

        ladderAnimationState = value;
        animator.SetBool(LadderParameter, value);
    }

    void FixedUpdate()
    {
        if (isClimbing)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, inputVertical * climbSpeed);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Ladder"))
        {
            if (!ladderContacts.Add(collision))
                return;

            isClimbing = true;
            rb.gravityScale = 0f;
            animator.SetBool("iswallidle", false);
            animator.SetBool("isWallSliding", false);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Ladder"))
        {
            ladderContacts.Remove(collision);
            if (ladderContacts.Count == 0)
            {
                isClimbing = false;
                inputVertical = 0f;
                rb.gravityScale = defaultGravity;
            }
        }
    }

    private void OnDisable()
    {
        ladderContacts.Clear();
        isClimbing = false;
        inputVertical = 0f;
        if (animator != null)
            SetLadderAnimation(false);
        if (rb != null)
            rb.gravityScale = defaultGravity;
    }

    private void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }
}
