using UnityEngine;

public class Slime : Monster
{
    public override void Move()
    {
        transform.LookAt(new Vector3(Camera.main.transform.position.x, 0, Camera.main.transform.position.z));
        transform.Translate(Vector3.forward * Speed * Time.deltaTime);
    }
}
