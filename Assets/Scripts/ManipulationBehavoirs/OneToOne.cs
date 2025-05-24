using UnityEngine;

public class OneToOne : ManipulationTechnique
{
    public override void Apply(Transform target)
    {
        Vector3 deltaPos = PinchDetector.GetInstance().IsLeftPinching ? HandPosition.GetInstance().LeftPinchTipPosition_delta : HandPosition.GetInstance().RightPinchTipPosition_delta;
        target.position += deltaPos;

        Quaternion deltaRot = PinchDetector.GetInstance().IsLeftPinching ? HandPosition.GetInstance().LeftHandRotation_delta : HandPosition.GetInstance().RightHandRotation_delta;
        target.rotation = deltaRot * target.rotation;
    }
}