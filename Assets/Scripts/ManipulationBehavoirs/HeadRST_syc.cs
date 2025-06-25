using System;
using UnityEngine;
using UnityEngine.Assertions.Comparers;

public class HeadRST_sync : ManipulationTechnique
{

    [Header("One Euro Filter")]

    public float FilterFrequency = 90f;
    public float FilterMinCutOff = 0.05f;
    public float FilterBeta = 10f;
    public float FitlerDcutoff = 1f;

    private OneEuroFilter<Vector3> _gazeDirFilter;
    private OneEuroFilter<Vector3> _gazePosFilter;

    private float _depthGain;
    private float _depthOffset;

    private Vector3 _accumulatedHandOffset;

    void Awake()
    {
        _gazeDirFilter = new OneEuroFilter<Vector3>(FilterFrequency);
        _gazePosFilter = new OneEuroFilter<Vector3>(FilterFrequency);
    }

    public override void OnSingleHandGrabbed(Transform target)
    {
        _depthOffset = Vector3.Distance(EyeGaze.GetInstance().GetGazeRay().origin, target.position);
        // _accumulatedHandOffset = Vector3.zero;
    }

    public override void ApplySingleHandGrabbedBehaviour(Transform target)
    {
        EyeGaze gazeData = EyeGaze.GetInstance();
        HeadMovement headData = HeadMovement.GetInstance();
        HandPosition handData = HandPosition.GetInstance();

        Vector3 rawGazeOrigin = gazeData.GetRawGazeOrigin();
        Vector3 rawGazeDirection = gazeData.GetRawGazeDirection();

        _gazeDirFilter.UpdateParams(FilterFrequency, FilterMinCutOff, FilterBeta, FitlerDcutoff);
        _gazePosFilter.UpdateParams(FilterFrequency, FilterMinCutOff, FilterBeta, FitlerDcutoff);

        Vector3 gazeOrigin = _gazePosFilter.Filter(rawGazeOrigin);
        Vector3 gazeDirection = _gazeDirFilter.Filter(rawGazeDirection);

        Quaternion deltaHandRot = handData.GetDeltaHandRotation(usePinchTip: true);
        Vector3 deltaHandPos = handData.GetDeltaHandPosition(usePinchTip: true);


        bool isBallisticHeadMovement = headData.HeadSpeed >= 0.2f || Math.Abs(headData.HeadAcc) >= 1f;
        bool HandNotFastMoving = handData.GetHandSpeed() <= 0.5f;
        bool addDepthOffsetWithHead = isBallisticHeadMovement && HandNotFastMoving;

        if(addDepthOffsetWithHead) _depthOffset += headData.DeltaHeadY * 0.2f;

        _accumulatedHandOffset += deltaHandPos;

        if (gazeData.IsSaccading())
        {
            _accumulatedHandOffset = Vector3.zero;
        }

        target.position = gazeOrigin + gazeDirection * Mathf.Clamp(_depthOffset, 1f, 10f) + _accumulatedHandOffset;
        target.rotation = deltaHandRot * target.rotation;

    }
    


}