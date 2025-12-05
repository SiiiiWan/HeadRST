using UnityEngine;

public class ManipulatableCube : ManipulatableObject
{

    public Transform CubeVisualTransform;

    public override void OnGazeConeEnter()
    {
        base.OnGazeConeEnter();
        CubeManager.GetInstance().RegisterFocusedCube(this);
        UpdateCubeMaterial(CubeManager.GetInstance().CubeTransparentMaterial);
    }

    public override void OnGazeConeExit()
    {
        base.OnGazeConeExit();
        CubeManager.GetInstance().UnregisterFocusedCube(this);
        UpdateCubeMaterial(CubeManager.GetInstance().CubeSolidMaterial);
    }

    public override void OnHoverEnter()
    {
        base.OnHoverEnter();

        UpdateCubeMaterial(CubeManager.GetInstance().CubeHoverMaterial);
        GetComponent<Outline>().enabled = true;
    }

    public override void OnHoverExit()
    {
        base.OnHoverExit();
        GetComponent<Outline>().enabled = false;
        RefreshInGazeConeState();
    }

    public override void OnPickup()
    {
        base.OnPickup();
    }

    public override void OnDrop()
    {
        UseGravity = Vector3.Distance(transform.position, Camera.main.transform.position) > 1f;
        base.OnDrop();
    }

    public void UpdateCubeMaterial(Material mat)
    {
        if (CubeVisualTransform.GetComponent<Renderer>().material != mat)
            CubeVisualTransform.GetComponent<Renderer>().material = mat;
    }

}
