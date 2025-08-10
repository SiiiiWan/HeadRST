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
        VisualGainValue = Mathf.Max(1, GetVisualGain(GrabbedObject.transform.position));
        OffsetAddedByHand = PinchPosition_delta * VisualGainValue;
        GrabbedObject.transform.position += OffsetAddedByHand;

        AngleRotatedByHand = Quaternion.Angle(PinchRotation_delta * GrabbedObject.transform.rotation, GrabbedObject.transform.rotation);
        GrabbedObject.transform.rotation = PinchRotation_delta * GrabbedObject.transform.rotation;
    }

    public float GetVisualGain(Vector3 objectPosition)
    {
        return Mathf.Max(1f, Vector3.Distance(objectPosition, GazeOrigin) / Vector3.Distance(PinchPosition, GazeOrigin));
    }
}