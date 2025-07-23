using UnityEngine;

public class GazeHand : ManipulationTechnique
{
    float _armLength = 0.75f;
    float _depth;
    Vector3 _handInitPosition;

    public override Vector3 GetNewVirtualHandPosition()
    {
        Vector3 nextVirtualHandPosition = VirtualHandPosition;

        Vector3 gazeOriginToHandOffest = WristPosition - GazeOrigin;

        Vector3 handOffsetVector = PinchPosition - _handInitPosition;
        float handOffsetDistance = Vector3.Project(handOffsetVector, GazeDirection).magnitude;

        if (handOffsetDistance >= 0.05f) _depth += (handOffsetDistance - 0.05f) * 0.144f * (Vector3.Dot(handOffsetVector, GazeDirection) > 0 ? 1 : -1);

        Vector3 gazePoint = GazeOrigin + GazeDirection * _depth;

        GrabbedObject.position = gazePoint + gazeOriginToHandOffest;
        GrabbedObject.rotation = PinchRotation_delta * GrabbedObject.rotation;

        return nextVirtualHandPosition;
    }    

}
