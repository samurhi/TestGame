using UnityEngine;

public class PlayerAnimator3 : MonoBehaviour
{
    private PlayerMovement2 pm;
    private Rigidbody2D rb2d;
    private Animator animator;
    private SpriteRenderer spR;

    private float speedX;
    private float speedY;

    private float flashTimer;
    private bool flashState;
    private float getUpTimer;

    public float flashRate;
    public float getUpTime;

    void Start()
    {
        pm = GetComponentInParent<PlayerMovement2>();
        rb2d = GetComponentInParent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spR = GetComponent<SpriteRenderer>();

        flashTimer = 0.0f;
        getUpTimer = 0.0f;
    }

    void Update()
    {
        transform.rotation = Quaternion.Euler(0, 0, 0);

        speedX = rb2d.velocity.x;
        speedY = rb2d.velocity.y;
        
        animator.SetFloat("speedX", Mathf.Abs(speedX));

        //check character state in PlayerMovement2 script and play correct animation based on that
        if (!pm.IsDashing && !pm.Impact)
        {
            switch (pm.CharacterState)
            {
                case "crouching":
                    animator.Play("monk_crouch");
                    break;
                case "airborne":
                    if (speedY < 0.1) animator.Play("monk_fall");
                    else animator.Play("monk_jump");
                    break;
                case "wallbound":
                    if (speedY < 0.1)
                    {
                        animator.Play("monk_wallGrab");
                        transform.rotation = Quaternion.Euler(0, 0, pm.Facingness * 25);
                    }
                    else animator.Play("monk_jump");
                    break;
                case "walking":
                    if (pm.WallBound) animator.Play("monk_idle");
                    else animator.Play("monk_walk");
                    break;
                case "idle":
                    animator.Play("monk_idle");
                    break;
            }
        }
        //asthetically rotate the sprite when he dashes
        else if (pm.IsDashing)
        {
            animator.Play("monk_dash");
            transform.rotation = Quaternion.Euler(0, 0, -pm.Facingness * 15);
        }
        //asthetically play the get up animation when he hits the ground hard
        else if (pm.Impact)
        {
            animator.Play("monk_getUp");
            getUpTimer += Time.deltaTime;

            if (getUpTimer > getUpTime)
            {
                pm.Impact = false;
                getUpTimer = 0.0f;
            }
        }

        //cause the character to flash when out of air moves
        if (!pm.CanAirMove)
        {
            flashTimer += Time.deltaTime;

            if (flashTimer > flashRate && flashState)
            {
                spR.color = new Color(0.5f, 0.5f, 0.5f, 0.7f);
                flashTimer = 0.0f;
                flashState = false;
            }
            else if (flashTimer > flashRate && !flashState)
            {
                spR.color = new Color(1, 1, 1, 1);
                flashTimer = 0.0f;
                flashState = true;
            }
        }
        else { spR.color = new Color(1, 1, 1, 1); flashState = true; }
    }
}