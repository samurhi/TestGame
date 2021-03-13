using System.Collections;
using UnityEngine;

//TODO: add second 2D collider to disable when crouching, fireballs, chi burn

public class PlayerMovement2 : MonoBehaviour
{
    //other components of the character
    private Swipe swipeListener;
    private FXScript fX;
    private Rigidbody2D rb2d;

    //player input variables
    private bool tap, swipeUp, swipeDown, swipeRight, swipeLeft, isHolding;
    private int queueNumber;
    private bool isListeningForInput;

    //variables to adjust how the character feels
    [Header("Assistance Scripts Used")]
    public bool alwaysListeningAssist;
    public float necTimeBetweenInputs;
    public bool stopWhenLanding;
    //assist which, when the character has wall grabbing and hits close to the ground, they auto fall
    //assist to jump when you jump a little late
    [Header("State Check Variables")]
    public float groundCheckRadius;
    public float groundCheckOffset;
    public LayerMask whatIsGround;
    public float wallCheckRadius;
    public float wallCheckOffset;
    public LayerMask whatIsWall;
    [Header("Static Movement Variables")]
    [Range(0, 90)] public int wallJumpAngle;
    [Range(1000, 3500)] public float groundPoundForce;
    [Range(0.5f, 1.5f)] public float groundPoundFreeze;
    [Range (0.01f, 0.1f)] public float acceleration;
    [Range(2, 4)] public float reducedGravity;
    [Range(3, 6)] public float normalGravity;
    [Range(0.5f, 3)] public float wallFallMaxSpeed;
    [Range(0.0f, 1.0f)] public float bigFallSlowDown;
    [Range(0.1f, 0.3f)] [Tooltip("How quickly the wall slows your character down")] public float wallDeceleraton;
    [Range(15, 40)] public float minSpeedForLanding;
    [Header("Dynamic Movement Variables")]
    [Range(400, 800)] public float jumpForce;
    [Range(0.01f, 0.5f)] public float airJumpFreeze;
    [Range(400, 800)] public float wallJumpForce;
    [Range(4, 9)] public float walkSpeed;
    [Range(20, 60)] public float dashSpeed;
    [Range(0.1f, 0.3f)] public float dashLength;
    [Header("Skills Learned")]
    public int airMovesUsable;
    public bool canGroundpound, canAirJump, canDash, canBackDash, canFireball, canChiBurn, canAirControl, canWallReset,
        canWallGrab;

    //governing variables to use internally
    private int xInput;
    private int airMovesUsed;
    private bool canDownMove;
    private int xInputRememberer;
    private float targetVelocity;
    private float xVelocityRememberer;
    private float lastYVelocity;
    //governing variables which have get/set fxns
    private bool impact;
    private string characterState;
    private bool canAirMove;
    private int facingness;
    private bool wallBound;
    private bool isDashing;

    //timers
    private float inputBufferTimer;

    //gatekeeper variables
    private bool doAWalk, doAStand, doAJump, doAWallJump, doADash, doAFireball, doACrouch, doAnAirJump,
        doAGroundpound, doABackDash, doAChiBurn;
    private bool isCrouching, isGroundpounding, isAirJumping, wantToWallGrab;

    //----------------------------------------------------------------------------------------------------

    //Initialize any variables that need it
    private void Start()
    {
        // All other player components are initialized
        rb2d = GetComponent<Rigidbody2D>();
        fX = GetComponent<FXScript>();
        swipeListener = GameObject.Find("Scripts").GetComponent<Swipe>();

        // Initialize general variables + timers
        facingness = 1; //start facing right
        xVelocityRememberer = 0.0f;
        inputBufferTimer = 0.0f;
        airMovesUsed = 0;
        wallBound = false;

        //Initalize inputs
        tap = swipeUp = swipeRight = swipeDown = swipeLeft = false;
        queueNumber = 0;
        isListeningForInput = true;

        //Initialize gatekeeper variables
        doABackDash = doAChiBurn = doACrouch = doADash = doAnAirJump = doAFireball = doAGroundpound = doAJump =
            doAStand = doAWalk = doAWallJump = false;
        isAirJumping = isGroundpounding = isDashing = false;
        wantToWallGrab = true;
    }

    // Determine what the player input is and what sorts of movements ought to be done. Done every update to not miss anything
    private void Update()
    {
        //update timers
        inputBufferTimer += Time.deltaTime;

        //check to see if we are listening for input
        if (inputBufferTimer > necTimeBetweenInputs) isListeningForInput = true;
        else isListeningForInput = false;

        tap = swipeListener.Tap;
        swipeUp = swipeListener.SwipeUp;
        swipeDown = swipeListener.SwipeDown;
        swipeRight = swipeListener.SwipeRight;
        swipeLeft = swipeListener.SwipeLeft;
        isHolding = swipeListener.IsHolding;

        // Get general player input
        if (isListeningForInput)
        {
            // If the character just started listening for input, check if input was given a few frames before
            if (alwaysListeningAssist)
            {
                if (queueNumber == 1) tap = true;
                else if (queueNumber == 2) swipeUp = true;
                else if (queueNumber == 3) swipeRight = true;
                else if (queueNumber == 4) swipeDown = true;
                else if (queueNumber == 5) swipeLeft = true;

                queueNumber = 0;
            }

            xInput = (swipeRight ? 1 : 0) - (swipeLeft ? 1 : 0);
            if (xInput != 0) xInputRememberer = xInput;
        }
        // Make sure the character doesn't recieve input when it doesn't want it, but still track user input
        else
        {
            QueueInput();
            tap = swipeUp = swipeRight = swipeDown = swipeLeft = false;
        }

        //run state fxns
        FlipCheck();
        StateCheck();
        
        // If there is some sort of input and it's not immediately after some other input, queue an action
        if (!(tap == swipeUp == swipeDown == swipeRight == swipeLeft == false))
        {
            wantToWallGrab = true;

            if (tap)
            {
                if (characterState == "wallbound") doAWallJump = true;
                else if (characterState != "airborne") doAJump = true;
                else if (canAirJump) doAnAirJump = true;
            }
            else if (swipeRight || swipeLeft)
            {
                if (characterState == "idle" || characterState == "crouching") doAWalk = true;
                else if (characterState == "walking" || characterState == "airborne")
                {
                    if (xInput == facingness && canDash) doADash = true;
                    else if (characterState == "airborne" && Mathf.Abs(rb2d.velocity.x) < 0.1f) doADash = true;
                    else if (characterState == "airborne" && canBackDash) doABackDash = true;
                    else if (characterState == "walking" || canAirControl) doAWalk = true;
                }
            }
            else if (swipeUp && characterState != "wallbound") doAFireball = true;
            else if (swipeDown)
            {
                if (characterState == "idle") doACrouch = true;
                else if (characterState == "walking" || characterState == "crouching") doAStand = true;
                else if (characterState == "wallbound" && canWallGrab) wantToWallGrab = false;
                else if (canGroundpound) doAGroundpound = true;
            }
        }
    }

    // Move and execute character actions in physics space
    private void FixedUpdate()
    {
        if (isHolding) rb2d.gravityScale = reducedGravity;
        else rb2d.gravityScale = normalGravity;

        if (isGroundpounding && characterState != "airborne") isGroundpounding = false;

        if (doAJump) Jump(); //totally functional
        else if (doAWalk) Walk(); //totally functional
        else if (doAStand) Stand(); //totally functional
        else if (doABackDash && canAirMove) BackDash(); //totally functional but TODO: might want to add a freeze effect and/or a one time use thing
        else if (doAChiBurn) ChiBurn(); //TODO not started
        else if (doACrouch) Crouch(); //totally functional
        else if (doADash && canAirMove) StartCoroutine(Dash()); //done
        else if (doAnAirJump && canAirMove) //totally functional but TODO: maybe delete the freeze effect here and/or the one time use
            { StartCoroutine(Freeze(airJumpFreeze)); isAirJumping = true; xVelocityRememberer = rb2d.velocity.x; } //done
        else if (doAFireball) Fireball(); //TODO not started
        else if (doAGroundpound && canDownMove) { isGroundpounding = true; StartCoroutine(Freeze(groundPoundFreeze)); }
        else if (doAWallJump) WallJump(); //totally functional

        //actually move the character
        if (Mathf.Abs(targetVelocity) > 0.1f)
        {
            rb2d.velocity = Vector3.Lerp(rb2d.velocity, new Vector3(targetVelocity, rb2d.velocity.y, 0.0f), acceleration);
        }
        else rb2d.velocity = Vector3.Lerp(rb2d.velocity, new Vector3(targetVelocity, rb2d.velocity.y, 0.0f), 5 * acceleration);

        //allows the character to slow their falls on the wall
        if (canWallGrab && characterState == "wallbound" && rb2d.velocity.y < 0.1f && wantToWallGrab)
        {
            rb2d.velocity = Vector3.Lerp(rb2d.velocity, new Vector3(rb2d.velocity.x, 0.0f, 0.0f), 1);
            rb2d.gravityScale = 0.0f;
        }
        else if(characterState == "wallbound" && rb2d.velocity.y < -wallFallMaxSpeed)
        {
            rb2d.velocity = Vector3.Lerp(rb2d.velocity, new Vector3(rb2d.velocity.x, -wallFallMaxSpeed, 0.0f), wallDeceleraton);
        }

        //stops the character when they land briefly
        if (stopWhenLanding && ((lastYVelocity + minSpeedForLanding) < rb2d.velocity.y) && lastYVelocity < -minSpeedForLanding)
        {
            rb2d.velocity = new Vector3((rb2d.velocity.x * bigFallSlowDown), rb2d.velocity.y, 0.0f);
            impact = true;
        }

        //resets all movement variables
        if (doABackDash || doAChiBurn || doACrouch || doADash || doAnAirJump || doAFireball || doAGroundpound || doAJump ||
            doAStand || doAWalk || doAWallJump) //also keeps fxns which need to run many times from listening to input
        {
            inputBufferTimer = 0.0f;
            doABackDash = doAChiBurn = doACrouch = doADash = doAFireball = doAGroundpound = doAJump = doAWallJump =
                doAStand = doAWalk = doAnAirJump = false;
        }

        lastYVelocity = rb2d.velocity.y;
    }

    //----------------------------------------------------------------------------------------------------

    //governing functions which change everything else
    private void QueueInput() //this function allows one user input to be saved during a period of not listening to inputs and executed after inputs can be recieved
    {
        if (tap) queueNumber = 1;
        else if (swipeUp) queueNumber = 2;
        else if (swipeRight) queueNumber = 3;
        else if (swipeDown) queueNumber = 4;
        else if (swipeLeft) queueNumber = 5;
    }
    private void FlipCheck() //checks which direction the character is facing for asthetics and movement calculations
    {
        if (rb2d.velocity.x > 0.1f)
        {
            transform.localScale = new Vector3(1, 1, 1);
            facingness = 1;
        }
        else if (rb2d.velocity.x < -0.1f)
        {
            transform.localScale = new Vector3(-1, 1, 1);
            facingness = -1;
        }
    }
    private void StateCheck() //determines what state the character is in. For animations and movement calcs
    {
        bool isGrounded = false;
        wallBound = false;

        Collider2D[] groundColliders = Physics2D.OverlapCircleAll(rb2d.position + Vector2.down * groundCheckOffset,
            groundCheckRadius, whatIsGround);
        for (int i = 0; i < groundColliders.Length; i++)
        {
            if (groundColliders[i].gameObject != gameObject) isGrounded = true;
        }

        Collider2D[] rightColliders = Physics2D.OverlapCircleAll(rb2d.position +
                        Vector2.right * wallCheckOffset, wallCheckRadius, whatIsWall);
        for (int i = 0; i < rightColliders.Length; i++)
        {
            if (rightColliders[i].gameObject != gameObject) wallBound = true;
        }

        Collider2D[] leftColliders = Physics2D.OverlapCircleAll(rb2d.position +
            Vector2.left * wallCheckOffset, wallCheckRadius, whatIsWall);
        for (int i = 0; i < leftColliders.Length; i++)
        {
            if (leftColliders[i].gameObject != gameObject) wallBound = true;
        }

        if (wallBound && !isGrounded) characterState = "wallbound";
        else if (isGrounded == false) characterState = "airborne";
        else if (isCrouching == true) characterState = "crouching";
        else if (Mathf.Abs(rb2d.velocity.x) < 0.1f && Mathf.Abs(targetVelocity) < 0.1f) characterState = "idle";
        else characterState = "walking";

        if (isGrounded || (characterState == "wallbound" && canWallReset)) airMovesUsed = 0;
        if (airMovesUsed >= airMovesUsable * 2) canDownMove = false;
        else canDownMove = true;
        if (airMovesUsed >= airMovesUsable) canAirMove = false;
        else canAirMove = true;
    }

    //action functions to be executed when doA_____ = true
    private void Jump()
    {
        isCrouching = false;
        rb2d.AddForce(new Vector2(0f, jumpForce));
    }
    private void Walk()
    {
       isCrouching = false;
       targetVelocity  = xInputRememberer * walkSpeed;
    }
    private void Stand()
    {
        isCrouching = false;
        targetVelocity = 0.0f;
    }
    private void AirJump()
    {
        isAirJumping = false;
        rb2d.velocity =  new Vector2 (xVelocityRememberer, 0.0f);
        xVelocityRememberer = 0.0f; 
        Jump();
        airMovesUsed++;
        //fx here for double jump
        //detract some amount of chi
    }
    private void WallJump()
    {
        targetVelocity = walkSpeed * -facingness;
        rb2d.AddForce(Vector3.Normalize(new Vector3(Mathf.Cos(wallJumpAngle) * -facingness, Mathf.Sin(wallJumpAngle),
            0.0f)) * wallJumpForce);
    }
    private void BackDash()
    {
        WallJump();
        airMovesUsed++;
        //fx here for back dash
        //detract some amount of chi
    }
    private void ChiBurn()
    {
        isCrouching = false;
    }
    private void Crouch()
    {
        isCrouching = true;
    }
    IEnumerator Dash()
    {
        float dashTimer = 0.0f;
        isDashing = true;

        while (dashTimer < dashLength)
        {
            dashTimer += Time.fixedDeltaTime;
            rb2d.velocity = new Vector3(dashSpeed * facingness, 0, 0);
            inputBufferTimer = 0.0f;
            yield return new WaitForFixedUpdate();
        }
        rb2d.velocity = new Vector3(targetVelocity, 0, 0);
        airMovesUsed++;
        isDashing = false;
    }
    private void Fireball()
    {
        
    }
    private void Groundpound()
    {
        rb2d.AddForce(new Vector2(0f, -groundPoundForce));
        targetVelocity = 0.0f;
        airMovesUsed = airMovesUsable * 2 + 1;
        //detract some amount of chi
    }

    IEnumerator Freeze(float duration)
    {
        float freezeTimer = 0.0f;

        rb2d.constraints = RigidbodyConstraints2D.FreezeAll;
        if (isGroundpounding) fX.ChargeChi();
        
        while (freezeTimer < duration)
        {
            freezeTimer += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        rb2d.constraints = RigidbodyConstraints2D.None;
        rb2d.constraints = RigidbodyConstraints2D.FreezeRotation;

        if (isAirJumping) AirJump();
        else if (isGroundpounding) Groundpound();
    }

    //----------------------------------------------------------------------------------------------------

    public bool Impact { get { return impact; } set { impact = value; } }
    public bool CanAirMove { get { return canAirMove; } }
    public bool WallBound { get { return wallBound; } }
    public int Facingness { get { return facingness; } }
    public string CharacterState { get { return characterState; } }
    public bool IsDashing { get { return isDashing; } }
    public bool IsGroundPounding { get { return isGroundpounding; } }
}