using System.Collections;
using UnityEngine;

public class TownsFolk : MonoBehaviour
{
    public GameObject scripts;
    private Animator animator;
    private BoxCollider2D triggerCollider;

    public TextBoxDialogue[] dialogueBoxes;
    public Transform leftBound;
    public Transform rightBound;
    [Range(1, 15)] public float speed;
    [Range(0.01f,0.99f)] public float walkTime;
    public float minWaitTime;
    public float maxWaitTime;

    private float leftEnd;
    private float rightEnd;
    private float goalPos;
    private float timer;
    private bool isTriggered;

    void Start()
    {
        animator = GetComponent<Animator>();
        triggerCollider = GetComponent<BoxCollider2D>();
        animator.SetFloat("speed", speed);
        leftEnd = leftBound.position.x;
        rightEnd = rightBound.position.x;
        goalPos = (Random.value * (rightEnd - leftEnd)) + leftEnd;
        StartCoroutine(Walk());
    }

    //a period of waiting based on their variables
    IEnumerator Wait(float time)
    {
        animator.Play("idle");

        for (float t = 0; t < Mathf.Min(Mathf.Max(minWaitTime, time * (1 - walkTime)), maxWaitTime); t += Time.deltaTime)
        {
            yield return null;
        }

        goalPos = (Random.value * (rightEnd - leftEnd)) + leftEnd;
        StartCoroutine(Walk());
    }

    //after waiting, NPC walks to a random point between their left and right bounds
    IEnumerator Walk()
    {
        animator.Play("walk");

        timer = 0.0f;

        for (float d = Mathf.Abs(transform.position.x - goalPos); d > 0; d -= speed/100)
        {
            if ((transform.position.x - goalPos) > 0)
            {
                transform.position -= new Vector3(speed / 100, 0, 0);
                transform.localScale = new Vector3(-1, 1, 1);
            }
            else
            {
                transform.position += new Vector3(speed / 100, 0, 0);
                transform.localScale = new Vector3(1, 1, 1);
            }
            timer += Time.deltaTime;
            yield return null;
        }
        StartCoroutine(Wait(timer));
    }

    //makes sure to give the player time to walk away from NPC after convo
    IEnumerator ConvoCooldown()
    {
        triggerCollider.enabled = false;

        yield return new WaitForSeconds(2);

        triggerCollider.enabled = true;
    }

    //convo starts
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isTriggered) return;
        isTriggered = true;

        if (collision.tag.Equals("Player"))
        {
            scripts.GetComponent<DialogueManager>().StartDialogue(dialogueBoxes);
            triggerCollider.size *= 2;
            StopAllCoroutines();
            animator.Play("idle");
        }
    }

    //convo ends
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!isTriggered) return;
        isTriggered = false;

        if (collision.tag.Equals("Player"))
        {
            scripts.GetComponent<DialogueManager>().EndDialogue();
            triggerCollider.size *= 0.5f;
            StartCoroutine(Walk());
            StartCoroutine(ConvoCooldown());
        }
    }
}