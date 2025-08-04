using UnityEngine;

public class GazeHand : ManipulationTechnique
{
    public float TorsoOffset = 0.35f; // Offset from the camera to the torso
    float _offset = 0.5f;
    float _depth = 2f;
    Vector3 _handInitPosition;
    Vector3 _pivotPoint;

    // [Header("One Euro Filter")]

    // public bool FilteringGaze = true;


    // public float FilterFrequency = 90f;
    // public float FilterMinCutOff = 0.9f;
    // public float FilterBeta = 15f;
    // public float FilterDcutoff = 1f;

    // private OneEuroFilter<Vector3> _gazeDirFilter = new OneEuroFilter<Vector3>(90f);
    // private OneEuroFilter<Vector3> _gazePosFilter = new OneEuroFilter<Vector3>(90f);

    public Vector3 ExtraFilteredGazeDirection { get; private set; }

    public override void ExtraUpdateTrackingData()
    {

        // if (FilteringGaze)
        // {
        //     _gazeDirFilter.UpdateParams(FilterFrequency, FilterMinCutOff, FilterBeta, FilterDcutoff);
        //     _gazePosFilter.UpdateParams(FilterFrequency, FilterMinCutOff, FilterBeta, FilterDcutoff);

        //     ExtraFilteredGazeDirection = _gazeDirFilter.Filter(GazeData.GetRawGazeDirection());
        //     ExtraFilteredGazeOrigin = _gazePosFilter.Filter(GazeData.GetRawGazeOrigin());
        // }

        ExtraFilteredGazeDirection = Vector3.Lerp(ExtraFilteredGazeDirection, GazeData.GetRawGazeDirection(), 0.1f);

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
        _depth = Mathf.Clamp(_depth, MinDepth, MaxDepth);

        _pivotPoint = GazeOrigin + gazeToObjectVector.normalized * _depth;

        VirtualHandPosition = _pivotPoint + manubriumToHandOffset;
    }

    public override void ApplyGazingButNotGrabbingBehaviour()
    {
        VirtualHandPosition += WristPosition_delta;
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

        _depth = Mathf.Clamp(_depth, MinDepth, MaxDepth);

        Vector3 manubriumPoint = HeadPosition + Vector3.down * TorsoOffset;
        Vector3 manubriumToHandOffset = WristPosition - manubriumPoint;

        _pivotPoint = GazeData.GetRawGazeOrigin() + ExtraFilteredGazeDirection * _depth;

        VirtualHandPosition = _pivotPoint + manubriumToHandOffset;
    }

    public override void ApplyIndirectGrabbedBehaviour()
    {
        VirtualHandPosition += WristPosition_delta;
    }

}
