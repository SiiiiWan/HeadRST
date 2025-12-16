using Unity.VisualScripting;
using UnityEngine;

public enum CubeColor
{
    Red,
    Green,
    Blue
}

public class ManipulatableCube : ManipulatableObject
{

    public CubeColor CubeColor;
    protected override void Update()
    {
        base.Update();
        
        if(transform.position.y < transform.localScale.y * 0.5f)
        {
            transform.position = new Vector3(transform.position.x, transform.localScale.y * 0.5f, transform.position.z);
        }

    }
    public void UpdateCubeMaterial(Material mat)
    {
        // if (CubeVisualTransform.GetComponent<Renderer>().material != mat)
        //     CubeVisualTransform.GetComponent<Renderer>().material = mat;
    }

}
