using UnityEngine;

public class ScrollTopBottom : MonoBehaviour
{
    public GameObject parent;

    private void Start()
    {
        GetComponent<RectTransform>().sizeDelta = new Vector2(parent.GetComponent<RectTransform>().sizeDelta.x + 30, 15);
    }
}