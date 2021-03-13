using UnityEngine;

public class Background : MonoBehaviour
{
    public GameObject clouds;
    public GameObject background;
    public GameObject middleGround;
    public GameObject foreground;

    public float cloudSpeedAbs;
    public float backgroundSpeedRel;
    public float middleGroundSpeedRel;
    public float foregroundSpeedRel;

    public bool endlessPanCloud;
    public float cloudSizeX;
    public bool endlessPanBack;
    public float backgroundSizeX;
    public bool endlessPanMiddle;
    public float middleGroundSizeX;
    public bool endlessPanFore;
    public float foregroundSizeX;

    private Transform cam;
    private Vector3 previousPos;

    void Start()
    {
        cam = GetComponent<Transform>();
        previousPos = cam.position;
    }

    void Update()
    {
        //move various backgrounds with parallax
        Vector3 difference = cam.position - previousPos;

        background.transform.position -= difference * backgroundSpeedRel;
        middleGround.transform.position -= difference * middleGroundSpeedRel;
        foreground.transform.position -= difference * foregroundSpeedRel;

        previousPos = cam.position;

        //move the clouds regardless of player movement
        clouds.transform.position += new Vector3(cloudSpeedAbs, 0.0f, 0.0f);

        if (endlessPanCloud)
        {
            if (clouds.transform.position.x > cloudSizeX) clouds.transform.position += new Vector3(-cloudSizeX, 0.0f, 0.0f);
            else if (clouds.transform.position.x < -cloudSizeX) clouds.transform.position += new Vector3(cloudSizeX, 0.0f, 0.0f);
        }
        if (endlessPanBack)
        {
            if (background.transform.position.x > backgroundSizeX) background.transform.position += new Vector3(-backgroundSizeX, 0.0f, 0.0f);
            else if (background.transform.position.x < -backgroundSizeX) background.transform.position += new Vector3(backgroundSizeX, 0.0f, 0.0f);
        }
        if (endlessPanMiddle)
        {
            if (middleGround.transform.position.x > middleGroundSizeX) middleGround.transform.position += new Vector3(-middleGroundSizeX, 0.0f, 0.0f);
            else if (middleGround.transform.position.x < -middleGroundSizeX) middleGround.transform.position += new Vector3(middleGroundSizeX, 0.0f, 0.0f);
        }
        if (endlessPanFore)
        {
            if (foreground.transform.position.x > foregroundSizeX) foreground.transform.position += new Vector3(-foregroundSizeX, 0.0f, 0.0f);
            else if (foreground.transform.position.x < -foregroundSizeX) foreground.transform.position += new Vector3(foregroundSizeX, 0.0f, 0.0f);
        }
    }
}
