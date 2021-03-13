using UnityEngine;

public class FXScript : MonoBehaviour
{
    //game objects to effect
    private PlayerMovement2 player;

    //particle effects to play
    public GameObject exImplosion;
    public GameObject trailFreeze;
    public GameObject impactExplosion;
    public GameObject trailDash;

    //general functionality variables
    private Vector3 pos;
    private int facingness;

    //ground pound variables
    private bool isGroundPounding;
    private bool gPTripper;

    //dash functionality variables
    private bool isDashing;

    void Start()
    {
        player = GetComponent<PlayerMovement2>();
        isGroundPounding = false;
        isDashing = false;
        gPTripper = false;
        facingness = 1;
    }

    void FixedUpdate()
    {
        pos = player.transform.position;
        isDashing = player.IsDashing;
        facingness = player.Facingness;
        isGroundPounding = player.IsGroundPounding;

        if (gPTripper && !isGroundPounding) { gPTripper = false; ImpactExplosion(); }

        if (isGroundPounding) TrailPound();
        else if (isDashing) PlayDashEffect();
    }

    //functions called for ground pounding
    public void ChargeChi()
    {
        GameObject instance = Instantiate(exImplosion, pos, Quaternion.identity);
        Destroy(instance, player.groundPoundFreeze);
        gPTripper = true;
    }
    private void ImpactExplosion()
    {
        GameObject instance = Instantiate(impactExplosion, pos + Vector3.down * 1.85f, Quaternion.identity);
        Destroy(instance, 1.0f);
    }

    //functions to be called many times in this script to show fast movement
    private void TrailPound()
    {
        GameObject instance = Instantiate(trailFreeze, (pos + Vector3.forward * 0.01f), Quaternion.identity);
        Destroy(instance, 0.2f);
    }
    private void PlayDashEffect()
    {
        GameObject instance = Instantiate(trailDash, (pos + Vector3.forward * 0.01f),
            Quaternion.Euler(0,90 - facingness * 90, -10));
        Destroy(instance, 0.2f);
    }
}
