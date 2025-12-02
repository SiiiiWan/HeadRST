using UnityEngine;

public class ManipulatableCube : MonoBehaviour
{
    public bool IsInGazeCone { get; private set; }
    public float AngleToGaze { get; private set; }

    public Transform CubeVisualTransform;

    // Update is called once per frame
    void Update()
    {
        AngleToGaze = Vector3.Angle(EyeGaze.GetInstance().GetGazeRay().direction, transform.position - EyeGaze.GetInstance().GetGazeRay().origin);
        IsInGazeCone = AngleToGaze <= 10f || EyeGaze.GetInstance().GetGazeHitTrans() == transform;

        if (IsInGazeCone)
        {
            CubeManager.GetInstance().RegisterFocusedCube(this);
            UpdateMaterial(CubeManager.GetInstance().CubeTransparentMaterial);
        }
        else
        {
            CubeManager.GetInstance().UnregisterFocusedCube(this);
            UpdateMaterial(CubeManager.GetInstance().CubeSolidMaterial);
        }

        if(CubeManager.GetInstance().ClosestFocusedCube == this)
        {
            UpdateMaterial(CubeManager.GetInstance().CubeHoverMaterial);
        }
    }

    public void UpdateMaterial(Material mat)
    {
        if(CubeVisualTransform.GetComponent<Renderer>().material != mat)
            CubeVisualTransform.GetComponent<Renderer>().material = mat;
    }
}
