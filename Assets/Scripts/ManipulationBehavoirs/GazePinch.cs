using UnityEngine;

public class GazePinch : ManipulationTechnique
{
    public override string TechniqueName => "GAZE+PINCH";

    public override void ApplyGrabbedBehaviour()
    {
        ApplyGazePinchTransform();
    }

    protected void ApplyGazePinchTransform()
    {
        VisualGainValue = GetVisualGain(GrabbedObject.transform.position);
        OffsetAddedByHand = PinchPosition_delta * VisualGainValue;
        GrabbedObject.transform.position += OffsetAddedByHand;

        AngleRotatedByHand = Quaternion.Angle(PinchRotation_delta * GrabbedObject.transform.rotation, GrabbedObject.transform.rotation);
        GrabbedObject.transform.rotation = PinchRotation_delta * GrabbedObject.transform.rotation;
    }

    protected float GetVisualGain(Vector3 objectPosition)
    {
        return Mathf.Max(1f, Vector3.Distance(objectPosition, GazeOrigin) / Vector3.Distance(PinchPosition, GazeOrigin));
    }
}
