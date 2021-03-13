using UnityEngine;

public class Teleport : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag.Equals("Player"))
        {
            collision.transform.position = new Vector3(gameObject.transform.GetChild(0).position.x,
                gameObject.transform.GetChild(0).position.y, collision.transform.position.z);
        }
    }
}
