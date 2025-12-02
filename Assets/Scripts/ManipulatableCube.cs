using UnityEngine;

public class ManipulatableCube : MonoBehaviour
{
    public bool IsInGazeCone { get; private set; }
    public float AngleToGaze { get; private set; }

    public bool IsPickedUp { get; private set; }
    public Transform CubeVisualTransform;

    // Update is called once per frame
    void Update()
    {
        if (IsPickedUp)
        {
            UpdatePosition(CubeManager.GetInstance().CubeStackingCursor.transform.position);

            if (PinchDetector.GetInstance().IsNoHandPinching)
            {
                IsPickedUp = false;
            }
            return;
        }

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

        if (CubeManager.GetInstance().ClosestFocusedCube == this)
        {
            UpdateMaterial(CubeManager.GetInstance().CubeHoverMaterial);
            if (PinchDetector.GetInstance().IsOneHandPinching && PinchDetector.GetInstance().IsNoHandPinching_LastFrame)
            {
                IsPickedUp = true;
            }
        }
    }

    public void UpdateMaterial(Material mat)
    {
        if (CubeVisualTransform.GetComponent<Renderer>().material != mat)
            CubeVisualTransform.GetComponent<Renderer>().material = mat;
    }
    
    public void UpdatePosition(Vector3 newPosition)
    {
        transform.position = newPosition;
    }
}
