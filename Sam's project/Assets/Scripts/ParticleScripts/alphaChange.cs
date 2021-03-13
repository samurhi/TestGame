using UnityEngine;

public class alphaChange : MonoBehaviour
{
    private SpriteRenderer sp;
    private float alpha;

    public float alphaDecreaser;

    void Start()
    {
        sp = GetComponent<SpriteRenderer>();
        alpha = 0.8f;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        alpha -= alphaDecreaser;
        sp.color = new Color(1, 1, 1, alpha);
    }
}
