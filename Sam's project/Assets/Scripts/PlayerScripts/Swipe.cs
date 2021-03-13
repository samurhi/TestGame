using UnityEngine;

public class Swipe : MonoBehaviour
{
    private bool tap, swipeUp, swipeDown, swipeRight, swipeLeft, isHolding;
    private Vector2 startTouch, swipeDelta;
    private bool isTouching;
    private bool inputTrigger;
    [HideInInspector] public int inputIndex;

    private float touchStopwatch;

    public int swipeDeadzonePixels;
    public int tapDeadzonePixels;
    public float touchTimer;
    public bool keyErrorCheck;

    private void Update()
    {
        tap = swipeDown = swipeLeft = swipeRight = swipeUp = false;

        //check to see if there is a touch on the screen. If not, reset
        if (Input.touches.Length != 0)
        {
            if (Input.touches[0].phase == TouchPhase.Began)
            {
                isTouching = true;
                inputTrigger = true;
                startTouch = Input.touches[0].position;
            }
            else if (Input.touches[0].phase == TouchPhase.Ended || Input.touches[0].phase == TouchPhase.Canceled)
            {
                Reset();
            }
        }
        else isHolding = false;

        //calculate the distance between start and end of touch
        swipeDelta = Vector2.zero;
        if (isTouching)
        {
            touchStopwatch += Time.deltaTime;

            if (Input.touches.Length != 0) swipeDelta = Input.touches[0].position - startTouch;
        }

        //calculate initially if it's a tap or a swipe
        if ((swipeDelta.magnitude < tapDeadzonePixels && touchStopwatch > touchTimer) ||
            (inputTrigger && Input.touches.Length == 0))
        {
            tap = true;
            inputTrigger = false;
            isHolding = true;
            Reset();
        }

        //is the distance great enough to be considered a swipe?
        if (swipeDelta.magnitude > swipeDeadzonePixels)
        {
            float x = swipeDelta.x;
            float y = swipeDelta.y;

            tap = false;

            if (Mathf.Abs(x) >= Mathf.Abs(y))
            {
                //left or right swipe
                if (x >= 0) swipeRight = true;
                else swipeLeft = true;
            }
            else
            {
                //up or down
                if (y >= 0) swipeUp = true;
                else swipeDown = true;
            }

            Reset();
            inputTrigger = false;
        }

        //not used in actual game, just for testing on a keyboard
        if (keyErrorCheck)
        {
            if (Input.GetKeyDown(KeyCode.D)) swipeRight = true;
            else if (Input.GetKeyDown(KeyCode.A)) swipeLeft = true;
            else if (Input.GetKeyDown(KeyCode.W)) swipeUp = true;
            else if (Input.GetKeyDown(KeyCode.S)) swipeDown = true;
            else if (Input.GetKeyDown(KeyCode.Space)) tap = true;
            else if (Input.GetKey(KeyCode.Space)) isHolding = true;
        }
    }

    private void Reset()
    {
        startTouch = swipeDelta = Vector2.zero;
        isTouching = false;
        touchStopwatch = 0.0f;
    }

    public Vector2 SwipeDelta { get { return swipeDelta; } }
    public bool Tap { get { return tap; } }
    public bool SwipeUp { get { return swipeUp; } }
    public bool SwipeDown { get { return swipeDown; } }
    public bool SwipeRight { get { return swipeRight; } }
    public bool SwipeLeft { get { return swipeLeft; } }
    public bool IsHolding { get { return isHolding; } }
}
