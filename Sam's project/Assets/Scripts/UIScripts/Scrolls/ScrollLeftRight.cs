using UnityEngine;

public class ScrollLeftRight : MonoBehaviour
{
    public GameObject parent;

    private void Update()
    {
        GetComponent<RectTransform>().sizeDelta = new Vector2(10, parent.GetComponent<RectTransform>().sizeDelta.y);
    }
}
