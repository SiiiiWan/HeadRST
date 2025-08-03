using UnityEngine;

public class GazeNPinchOrigin : ManipulationTechnique
{

    public override void Update()
    {
        base.Update();
        VirtualHandPosition = WristPosition;
    }

    public override void ApplyIndirectGrabbedBehaviour()
    {

        GrabbedObject.transform.position += GetVisualGain(GrabbedObject.transform.position) * PinchPosition_delta;

        GrabbedObject.transform.rotation = PinchRotation_delta * GrabbedObject.transform.rotation;
    }

    public float GetVisualGain(Vector3 objectPosition)
    {
        return Mathf.Max(1f, Vector3.Distance(objectPosition, GazeOrigin) / Vector3.Distance(PinchPosition, GazeOrigin));
    }
}