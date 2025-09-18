using UnityEngine;

public class UserHeadMovement : MonoBehaviour
{
    public Transform body;
    void Update()
    {
        transform.position = Camera.main.transform.position;
        transform.rotation = Camera.main.transform.rotation; 

        body.position = new Vector3(transform.position.x, transform.position.y - body.localScale.y - transform.localScale.y / 2, transform.position.z);
    }
}
