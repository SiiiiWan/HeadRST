using UnityEngine;

public class GazeHand : ManipulationTechnique
{
    public float TorsoOffset = 0.35f; // Offset from the camera to the torso
    float _offset = 0.5f;
    float _depth = 2f;
    Vector3 _handInitPosition;
    Vector3 _pivotPoint;

    public Vector3 VirtualHandOffsetFromObject {get; private set; } = Vector3.zero;

    public Vector3 ExtraFilteredGazeDirection { get; private set; }

    public override void ExtraUpdateTrackingData()
    {
        ExtraFilteredGazeDirection = Vector3.Lerp(ExtraFilteredGazeDirection, GazeData.GetRawGazeDirection(), 0.1f).normalized;
    }

    public override void ApplyObjectFreeBehaviour()
    {
        VirtualHandPosition = WristPosition;
    }

    public override void TriggerOnLookAtNewObjectBehavior()
    {
        Vector3 gazeToObjectVector = GazingObject.transform.position - GazeOrigin;
        Vector3 manubriumPoint = HeadPosition + Vector3.down * TorsoOffset;
        Vector3 manubriumToHandOffset = WristPosition - manubriumPoint;

        _depth = gazeToObjectVector.magnitude - _offset;
        _depth = Mathf.Clamp(_depth, 0.05f, 15f);

        // _depth = Mathf.Clamp(_depth, MinDepth, MaxDepth);

        _pivotPoint = GazeOrigin + gazeToObjectVector.normalized * _depth;

        VirtualHandPosition = _pivotPoint + manubriumToHandOffset;
    }

    public override void ApplyGazingButNotGrabbingBehaviour()
    {
        VirtualHandPosition += WristPosition_delta;

        VirtualHandOffsetFromObject = VirtualHandPosition - GazingObject.transform.position;
    }

    public override void TriggerOnSingleHandGrabbed(ManipulatableObject obj, GrabbedState grabbedState)
    {
        base.TriggerOnSingleHandGrabbed(obj, grabbedState);

        _handInitPosition = WristPosition;
    }

    public override void ApplyDirectGrabbedBehaviour()
    {
        Vector3 handOffsetFromGrabInit = WristPosition - _handInitPosition;
        float handOffsetDistanceOnDepthAxis = Vector3.Project(handOffsetFromGrabInit, GazeDirection).magnitude;

        if (handOffsetDistanceOnDepthAxis >= 0.05f) _depth += (handOffsetDistanceOnDepthAxis - 0.05f) * 100f * 0.144f * 100f * (Vector3.Dot(handOffsetFromGrabInit, GazeDirection) > 0 ? 1 : -1) * Time.deltaTime / 100f;

        _depth = Mathf.Clamp(_depth, 0.05f, 15f);

        Vector3 manubriumPoint = HeadPosition + Vector3.down * TorsoOffset;
        Vector3 manubriumToHandOffset = WristPosition - manubriumPoint;

        _pivotPoint = GazeData.GetRawGazeOrigin() + ExtraFilteredGazeDirection * _depth;

        GrabbedObject.transform.position = _pivotPoint + manubriumToHandOffset;

        VirtualHandPosition = GrabbedObject.transform.position + VirtualHandOffsetFromObject;
    }

    public override void ApplyIndirectGrabbedBehaviour()
    {
        VirtualHandPosition += WristPosition_delta;
    }

}
