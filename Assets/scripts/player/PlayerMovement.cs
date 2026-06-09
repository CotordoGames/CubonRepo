using UnityEngine;
using UnityEditor;
using UnityEngine.InputSystem;
using System.Threading;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D rb;
    private PlayerInput input;
    public BoxCollider2D feet;
    public CameraFollowPlayer cam;
    public LayerMask GroundLayer;
    private float direction;
    private bool turning;

    [Header("Ground Movement")]

    [Min(0f)]
    //[Tooltip("the speed of which cubon moves on the ground before begining to run")]
    public int baseSpeed;

    [Min(0f)]
    //[Tooltip("how fast cubon accelerates towards his base ground speed(linearly)")]
    public float baseAcceleration;

    [Min(0f)]
    [Tooltip("cubon's maximium speed; after running at a normal pace for a breif moment, he starts accelerating to this")]
    public int runSpeed;

    [Min(0f)]
    //[Tooltip("how fast cubon moves from base speed to running speed")]
    public float runAcceleration;

    [Min(0f)]
    //[Tooltip("how fast cubon decelerates to 0 velocity")]
    public float deceleration;

    //[Tooltip("cubon's possible states")]
    public enum playerState{ idle, walk, run, maxspeed, jump, fall, land, turning, groundpounding, dashing };

    [Header("Air Movement")]

    [Min(0f)]
    //[Tooltip("how fast cubon can possibly move while airborne")]
    public float airSpeed;

    [Min(0f)]
    //[Tooltip("how fast cubon accels to air speed when jumping")]
    public float jumpAcceleration;

    [Min(0f)]
    [Tooltip("how fast cubon accels to air speed when falling")]
    public float fallAcceleration;

    [Min(0f)]
    //[Tooltip("how high cubon jumps by default")]
    public float baseJumpForce;

    [Min(0f)]
    //[Tooltip("how fast cubon moves from jumping to falling")]
    public float baseJumpCutOff;

    [Min(0f)]
    //[Tooltip("cubon's fall gravity and regular gravity")]
    public float baseGravity;
    public float fallGravity;

    [Header("GroundPound")]
    //[ToolTip("how fast cubon slams")]
    public float PoundStrength;

    //[ToolTip("how high cubon bounces")]
    public float BounceStrength;
    public bool Pounding;
    public float GroundPoundSpeedBoost;

    [Header("Dash")]

    //[ToolTip("how fast cubon moves when dashing")]
    public float DashSpeed;

    //[ToolTip("how long the dash last")]
    public float DashTime;

    //[Tooltip("the cooldown rate between dashes")]
    public float DashCoolDown;

    public bool IsDashing;
    public bool CanDash = true;

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
        direction = input.actions["walk"].ReadValue<float>();
        turning = isGrounded() && direction != 0 && Mathf.Sign(direction) != Mathf.Sign(rb.linearVelocityX) && Mathf.Abs(rb.linearVelocityX) > 0.1f;


        Dash();
        HorizontalMovement();
        VerticalMovement();
        GroundPound();
    }

    private void HorizontalMovement()
    {
        if (IsDashing)
        {
            state = playerState.dashing;
            return;
        }


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
            else if(rb.linearVelocityY < 0 && state != playerState.groundpounding)
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

            case playerState.groundpounding:
                rb.linearVelocityX = Mathf.MoveTowards(rb.linearVelocityX, airSpeed * direction, fallAcceleration * Time.deltaTime * 60); break;

            case playerState.dashing:
                break;
        }
        Debug.Log(state);
    }

    private void Dash()
    {
        if (IsDashing)
        {
            return;
        }

        if (input.actions["dash"].WasPressedThisFrame() && CanDash)
        {
            StartCoroutine(DashCoroutine());
        }
    }

    private void VerticalMovement()
    {
        if (IsDashing)
        {
            return;
        }

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

    private void GroundPound()
    {
        if (!isGrounded())
        {
            if (input.actions["pound"].WasPressedThisFrame() && !Pounding)
            {
                rb.linearVelocityY = PoundStrength;
                state = playerState.groundpounding;
                Pounding = true;
            }
            if (!input.actions["pound"].IsPressed())
            {
                Pounding = false;
            }
        }
        else
        {
            if (input.actions["pound"].IsPressed() && Pounding)
            {
                cam.ShakeCamera(1.0f, 0.2f);
                rb.linearVelocityY = BounceStrength;
                rb.linearVelocityX += GroundPoundSpeedBoost * direction;
                Pounding = false;
                state = playerState.jump;
            }
        }
    }

    private IEnumerator DashCoroutine()
    {
        // initialize and prepare for dash; store the gravity and then set it to 0 so our dash is straight
        CanDash = false;
        IsDashing = true;
        float OldGravity = rb.gravityScale;
        rb.gravityScale = 0;

        // this is our "dash". simply sets velocity X to dash speed.
        rb.linearVelocity = new Vector2(transform.localScale.x * DashSpeed, 0f);

        // end our dash, set gravity back to normal
        yield return new WaitForSeconds(DashTime);
        rb.linearVelocityX = transform.localScale.x * baseSpeed * 1.125f;
        rb.gravityScale = OldGravity;
        IsDashing = false;

        // wait until the cooldown ends to dash again.
        yield return new WaitForSeconds(DashCoolDown);
        CanDash = true;
    }

    private bool isGrounded()
    {
        return Physics2D.BoxCast(feet.bounds.center, feet.bounds.size, 0.0f, Vector2.down, 0.2f, GroundLayer);
    }
}
