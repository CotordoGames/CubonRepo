using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D rb;
    private PlayerInput input;
    public BoxCollider2D feet;
    public LayerMask GroundLayer;

    [Header("Ground Movement")]

    [Min(0f)]
    [Tooltip("the speed of which cubon moves on the ground before begining to run")]
    public int baseSpeed;

    [Min(0f)]
    [Tooltip("how fast cubon accelerates towards his base ground speed(linearly)")]
    public float baseAcceleration;

    [Min(0f)]
    [Tooltip("cubon's maximium speed; after running at a normal pace for a breif moment, he starts accelerating to this")]
    public int runSpeed;

    [Min(0f)]
    [Tooltip("how fast cubon moves from base speed to running speed")]
    public float runAcceleration;

    [Min(0f)]
    [Tooltip("how fast cubon decelerates to 0 velocity")]
    public float deceleration;

    [Tooltip("cubon's possible states")]
    public enum playerState{ idle, walk, run, maxspeed, jump, fall, land, turning };

    [Header("Air Movement")]

    [Min(0f)]
    [Tooltip("how fast cubon can possibly move while airborne")]
    public float airSpeed;

    [Min(0f)]
    [Tooltip("how fast cubon accels to air speed when jumping")]
    public float jumpAcceleration;

    [Min(0f)]
    [Tooltip("how fast cubon accels to air speed when falling")]
    public float fallAcceleration;

    [Min(0f)]
    [Tooltip("how high cubon jumps by default")]
    public float baseJumpForce;

    [Min(0f)]
    [Tooltip("how fast cubon moves from jumping to falling")]
    public float baseJumpCutOff;

    [Min(0f)]
    [Tooltip("cubon's fall gravity and regular gravity")]
    public float baseGravity;
    public float fallGravity;

    public playerState state;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        input = GetComponent<PlayerInput>();
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        HorizontalMovement();
        VerticalMovement();
    }

    private void HorizontalMovement()
    {
        float direction = input.actions["walk"].ReadValue<float>();
        bool turning = isGrounded() && direction != 0 && Mathf.Sign(direction) != Mathf.Sign(rb.linearVelocityX) && Mathf.Abs(rb.linearVelocityX) > 0.1f;

        if (isGrounded())
        {
            if (turning)
            {
                state = playerState.turning;
            }
            else if (direction == 0)
            {
                state = playerState.idle;
            }
            else
            {
                if (Mathf.Abs(rb.linearVelocityX) < baseSpeed)
                {
                    state = playerState.walk;
                }
                else if (Mathf.Abs(rb.linearVelocityX) < runSpeed)
                {
                    state = playerState.run;
                }
                else
                {
                    state = playerState.maxspeed;
                }
            }
        }
        else
        {
            if(rb.linearVelocityY > 0)
            {
                state = playerState.jump;
            }
            else if(rb.linearVelocityY < 0)
            {
                state = playerState.fall;
            }
        }

        switch (state)
        {
            case playerState.idle:
                rb.linearVelocityX = Mathf.MoveTowards(rb.linearVelocityX, 0, deceleration * Time.deltaTime * 60); break;

            case playerState.walk:
                rb.linearVelocityX = Mathf.MoveTowards(rb.linearVelocityX, baseSpeed * direction, baseAcceleration * Time.deltaTime * 60); break;

            case playerState.run:
                rb.linearVelocityX = Mathf.MoveTowards(rb.linearVelocityX, runSpeed * direction, runAcceleration * Time.deltaTime * 60); break;

            case playerState.maxspeed:
                rb.linearVelocityX = Mathf.MoveTowards(rb.linearVelocityX, runSpeed * direction, runAcceleration * Time.deltaTime * 60); break; //placeholder i forgot why i added this state

            case playerState.turning:
                rb.linearVelocityX = Mathf.MoveTowards(rb.linearVelocityX, 0, deceleration * Time.deltaTime * 60); break;

            case playerState.jump:
                rb.linearVelocityX = Mathf.MoveTowards(rb.linearVelocityX, airSpeed * direction, jumpAcceleration * Time.deltaTime * 60); break;

            case playerState.fall:
                rb.linearVelocityX = Mathf.MoveTowards(rb.linearVelocityX, airSpeed * direction, fallAcceleration * Time.deltaTime * 60); break;
        }
        Debug.Log(state);
    }

    private void VerticalMovement()
    {
        if (isGrounded())
        {
            if (input.actions["jump"].WasPressedThisFrame())
            {
                rb.linearVelocityY = baseJumpForce;
            }
        }
        else{
            if(rb.linearVelocityY < 0)
                rb.gravityScale = fallGravity;
            else
                rb.gravityScale = baseGravity;
            if (input.actions["jump"].WasReleasedThisFrame() && rb.linearVelocityY > 0)
            {
                rb.linearVelocityY /= baseJumpCutOff;
            }
        }
    }

    private bool isGrounded()
    {
        return Physics2D.BoxCast(feet.bounds.center, feet.bounds.size, 0.0f, Vector2.down, 0.2f, GroundLayer);
    }
}
