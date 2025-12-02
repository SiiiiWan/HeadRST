using UnityEngine;

public class ManipulatableCube : ManipulatableObject
{

    public Transform CubeVisualTransform;

    public override void SpecialBehaviour()
    {
        if (IsInGazeCone)
        {
            CubeManager.GetInstance().RegisterFocusedCube(this);
            UpdateCubeMaterial(CubeManager.GetInstance().CubeTransparentMaterial);
        }
        else
        {
            CubeManager.GetInstance().UnregisterFocusedCube(this);
            UpdateCubeMaterial(CubeManager.GetInstance().CubeSolidMaterial);
        }

        if (CubeManager.GetInstance().ClosestFocusedCube == this)
        {
            UpdateCubeMaterial(CubeManager.GetInstance().CubeHoverMaterial);
            GetComponent<Outline>().enabled = true;

            if (PinchDetector.GetInstance().IsOneHandPinching && PinchDetector.GetInstance().IsNoHandPinching_LastFrame)
            {
                IsPickedUp = true;
                SetCancelObjectGravity(true);
                GetComponent<Outline>().enabled = false;
                UseGravity = true;
            }
        }
        else
        {
            GetComponent<Outline>().enabled = false;
        }
    }

    public void UpdateCubeMaterial(Material mat)
    {
        if (CubeVisualTransform.GetComponent<Renderer>().material != mat)
            CubeVisualTransform.GetComponent<Renderer>().material = mat;
    }

}
