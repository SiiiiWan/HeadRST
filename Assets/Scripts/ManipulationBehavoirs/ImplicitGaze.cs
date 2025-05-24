using UnityEngine;

public class ImplicitGaze : ManipulationTechnique
{
    public override void Apply(Transform target)
    {
        Vector3 gazeOrigin = EyeGaze.GetInstance().GetGazeRay().origin;
        Vector3 gazeDirection = EyeGaze.GetInstance().GetGazeRay().direction;

        float distance = Vector3.Distance(gazeOrigin, target.position);
        target.position = gazeOrigin + gazeDirection * distance;

        Quaternion deltaRot = PinchDetector.GetInstance().IsLeftPinching ? HandPosition.GetInstance().LeftHandRotation_delta : HandPosition.GetInstance().RightHandRotation_delta;
        target.rotation = deltaRot * target.rotation;
    }
}