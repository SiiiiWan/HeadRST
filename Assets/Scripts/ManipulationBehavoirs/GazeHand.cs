using UnityEngine;

public class GazeHand : ManipulationTechnique
{
    float _armLength = 0.75f;
    float _depth;
    Vector3 _handInitPosition;

    public override void OnSingleHandGrabbed(Transform target)
    {
        EyeGaze gazeData = EyeGaze.GetInstance();

        Vector3 gazeOrigin = gazeData.GetGazeRay().origin;

        _depth = Vector3.Distance(target.position, gazeOrigin);
        _handInitPosition = HandPosition.GetInstance().GetHandPosition(usePinchTip: true);
    }

    public override void ApplySingleHandGrabbedBehaviour(Transform target)
    {
        HandPosition handData = HandPosition.GetInstance();
        EyeGaze gazeData = EyeGaze.GetInstance();
        HeadMovement headData = HeadMovement.GetInstance();

        Vector3 gazeDirection = gazeData.GetGazeRay().direction;
        Vector3 gazeOrigin = gazeData.GetGazeRay().origin;

        Vector3 handPosition = handData.GetHandPosition(usePinchTip: true);
        
        Vector3 gazeOriginToHandOffest = handPosition - gazeOrigin;

        Vector3 handOffsetVector = handPosition - _handInitPosition;
        float handOffsetDistance = Vector3.Project(handOffsetVector, gazeDirection).magnitude;

        if(handOffsetDistance >= 0.05f) _depth += (handOffsetDistance - 0.05f) * 0.144f * (Vector3.Dot(handOffsetVector, gazeDirection) > 0 ? 1 : -1);

        Vector3 gazePoint = gazeOrigin + gazeDirection * _depth;

        print(_depth);

        target.position = gazePoint + gazeOriginToHandOffest;

        Quaternion deltaRot = HandPosition.GetInstance().GetDeltaHandRotation(usePinchTip: true);
        target.rotation = deltaRot * target.rotation;
    }

}
