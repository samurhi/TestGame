using UnityEngine;

public class ScrollText : MonoBehaviour
{
    public GameObject parent;

    private void Update()
    {
        GetComponent<RectTransform>().sizeDelta = new Vector2(parent.GetComponent<RectTransform>().sizeDelta.x,
            parent.GetComponent<RectTransform>().sizeDelta.y);
    }
}
