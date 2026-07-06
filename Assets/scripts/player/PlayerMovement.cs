using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using FMODUnity;


public class PlayerMovement : MonoBehaviour
{
    [System.NonSerialized] public Rigidbody2D rb;
    private PlayerInput input;
    public BoxCollider2D feet;
    public Transform hands;
    public CameraFollowPlayer cam;
    public LayerMask GroundLayer;
    private float direction;
    private bool turning;
    private StudioEventEmitter em;

    [Header("Ground Movement")]

    [Min(0f)]
    //[Tooltip("the speed of which cubon moves on the ground before begining to run")]
    public int baseSpeed;

    [Min(0f)]
    //[Tooltip("how fast cubon accelerates towards his base ground speed(linearly)")]
    public float baseAcceleration;

    [Min(0f)]
    [Tooltip("cubon's maximium speed; after running at a normal pace for a brief moment, he starts accelerating to this")]
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
    public float poundStrength;

    public Vector2 ScreenShake;

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

    [Header("Wall Jumping/Sliding")]
    private bool IsSliding;
    public float WallSlideSpeed;
    public bool IsWallJumping;
    private float WallJumpingDirection;
    public float WallJumpingTime;
    private float WallJumpingCounter;
    public float WallJumpingDuration;
    public Vector2 WallJumpingPower;


    [Header("SFX")] 
    public EventReference jump;
    public EventReference thud;
    public EventReference dash;
    public EventReference wallJump;

    private bool Grounded;
    private bool Walled;

    private bool jumping;
    private bool sloped;

    public playerState state;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        em =  GetComponent<StudioEventEmitter>();
        input = GetComponent<PlayerInput>();
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {

        Grounded = isGrounded();
        sloped = OnSlope();
        Walled = IsWalled();
        direction = input.actions["walk"].ReadValue<float>();
        turning = Grounded && direction != 0 && Mathf.Sign(direction) != Mathf.Sign(rb.linearVelocityX) && Mathf.Abs(rb.linearVelocityX) > 0.1f;


        WallSlide();
        WallJump();
        Dash();
        HorizontalMovement();
        VerticalMovement();
        GroundPound();

        if(!IsWallJumping){Flip();}
    }



    private void Flip()
    {
        if(input.actions["walk"].ReadValue<float>() < 0){
            transform.localScale = new Vector2(-1, transform.localScale.y);
        } else if(input.actions["walk"].ReadValue<float>() > 0){
            transform.localScale = new Vector2(1, transform.localScale.y);
        }
    }



    private void HorizontalMovement()
    {
        if (IsDashing)
        {
            state = playerState.dashing;
            return;
        }


        if (Grounded)
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

        float speed = 0f;
        switch (state)
        {
            case playerState.idle:
                rb.linearVelocityX = Mathf.MoveTowards(rb.linearVelocityX, 0, deceleration * Time.deltaTime * 60); break;

            case playerState.walk:
                speed = Mathf.MoveTowards(OnSlope() ? rb.linearVelocity.magnitude : Mathf.Abs(rb.linearVelocityX), baseSpeed, baseAcceleration * Time.deltaTime * 60);
                if (!jumping) rb.linearVelocity = GetGroundNormal() * speed; break;

            case playerState.run:
                speed = Mathf.MoveTowards(OnSlope() ? rb.linearVelocity.magnitude : Mathf.Abs(rb.linearVelocityX), runSpeed, runAcceleration * Time.deltaTime * 60);
                if (!jumping) rb.linearVelocity = GetGroundNormal() * speed; break;

            case playerState.maxspeed:
                speed = Mathf.MoveTowards(OnSlope() ? rb.linearVelocity.magnitude : Mathf.Abs(rb.linearVelocityX), runSpeed, runAcceleration * Time.deltaTime * 60); //placeholder i forgot why i added this state
                if (!jumping) rb.linearVelocity = GetGroundNormal() * speed; break;

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

        if (Grounded && !jumping)
        {
            if (sloped)
            {
                rb.gravityScale = 0;
            }
            else
            {
                rb.gravityScale = baseGravity;
            }
            if (input.actions["jump"].WasPressedThisFrame())
            {
                PlayNoise(jump);
                jumping = true;
                rb.linearVelocityY = baseJumpForce;
            }
        }
        else if (!Grounded) {
            jumping = false;
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


    void PlayNoise(EventReference e)
    {
        RuntimeManager.PlayOneShot(e,  transform.position);
    }
    

    private void GroundPound()
    {
        if (!Grounded)
        {
            if (input.actions["pound"].WasPressedThisFrame() && !Pounding)
            {
                rb.linearVelocityY = poundStrength;
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
                PlayNoise(thud);
                cam.ShakeCamera(ScreenShake.x, ScreenShake.y);
                jumping = true;
                rb.linearVelocityY = BounceStrength;
                rb.linearVelocityX += GroundPoundSpeedBoost * direction;
                Pounding = false;
                state = playerState.jump;
            }
        }
    }


    private void StopWallJumping()
    {
        IsWallJumping = false;
    }


    private IEnumerator DashCoroutine()
    {
        PlayNoise(dash);
        // initialize and prepare for dash; store the gravity and then set it to 0 so our dash is straight
        CanDash = false;
        IsDashing = true;
        float OldGravity = rb.gravityScale;
        rb.gravityScale = 0;

        // this is our "dash". simply sets velocity X to dash speed.
        cam.ShakeCamera(ScreenShake.x / 1.5f, ScreenShake.y);
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




    private Vector2 GetGroundNormal()
    {
        RaycastHit2D hit = Physics2D.Raycast(
            new Vector2(feet.bounds.center.x, feet.bounds.min.y), Vector2.down,
            feet.bounds.extents.y + 0.3f, GroundLayer
        );

        if(hit && Mathf.Abs(hit.normal.x) > 0.01f)
        {
            return new Vector2(hit.normal.y, -hit.normal.x) * Mathf.Sign(direction);
        }
        return Vector2.right * Mathf.Sign(direction);
    }




    private bool IsWalled()
    {
        return Physics2D.OverlapCircle(hands.position, 0.2f, GroundLayer);
    }




    private void WallSlide()
    {
        if(Walled && !isGrounded() && direction != 0)
        {
            IsSliding = true;
            rb.linearVelocityY = Mathf.Clamp(rb.linearVelocityY, -WallSlideSpeed, float.MaxValue);
        }
        else
        {
            IsSliding = false;
        }
    }




    private bool OnSlope()
    {
        RaycastHit2D hit = Physics2D.Raycast(
            new Vector2(feet.bounds.center.x, feet.bounds.min.y), Vector2.down,
            feet.bounds.extents.y + 0.5f, GroundLayer
        );
        return hit && Mathf.Abs(hit.normal.x) > 0.01f;
    }



    private void WallJump()
    {
        if (IsSliding)
        {
            IsWallJumping = false;
            WallJumpingDirection = -transform.localScale.x;
            WallJumpingCounter = WallJumpingTime;

            CancelInvoke(nameof(StopWallJumping));
        }
        else
        {
            WallJumpingCounter -= Time.deltaTime;
        }

        if(input.actions["jump"].WasPressedThisFrame() && WallJumpingCounter > 0)
        {
            PlayNoise(wallJump);
            IsWallJumping = true;
            rb.linearVelocity = new Vector2(WallJumpingDirection * WallJumpingPower.x, WallJumpingPower.y);
            WallJumpingCounter = 0f;

            Invoke(nameof(StopWallJumping), WallJumpingDuration);
        }

        if(IsWallJumping && transform.localScale.x != WallJumpingDirection)
        {
            Vector3 localScale = transform.localScale;
            localScale.x *= -1;
            transform.localScale = localScale;
        }

    }
}
