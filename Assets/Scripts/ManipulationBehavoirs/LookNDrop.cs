using System;
using UnityEngine;

public class LookNDrop : ManipulationTechnique
{
    public float Multiplier = 100f;
    [Header("Gaze Filter")]

    public float FilterFrequency = 90f;
    public float FilterMinCutOff = 0.9f;
    public float FilterBeta = 15f;
    public float FitlerDcutoff = 1f;

    private OneEuroFilter<Vector3> _gazeDirFilter;
    private OneEuroFilter<Vector3> _gazePosFilter;

    private Vector3 _filteredGazeDir;
    private Vector3 _filteredGazeOrigin;

    private Vector3 _cumulatedHandOffset;
    private float _distanceOnGazeRay;

    void Awake()
    {
        _gazeDirFilter = new OneEuroFilter<Vector3>(FilterFrequency);
        _gazePosFilter = new OneEuroFilter<Vector3>(FilterFrequency);
    }

    public override void ApplySingleHandGrabbedBehaviour(Transform target)
    {
        EyeGaze gazeData = EyeGaze.GetInstance();
        HandPosition handData = HandPosition.GetInstance();

        Vector3 deltaHandPos = handData.GetDeltaHandPosition(usePinchTip: true);



        _gazeDirFilter.UpdateParams(FilterFrequency, FilterMinCutOff, FilterBeta, FitlerDcutoff);
        _gazePosFilter.UpdateParams(FilterFrequency, FilterMinCutOff, FilterBeta, FitlerDcutoff);


        _filteredGazeDir = _gazeDirFilter.Filter(gazeData.GetRawGazeDirection());
        _filteredGazeOrigin = _gazePosFilter.Filter(gazeData.GetRawGazeOrigin());

        if (gazeData.IsSaccading())
        {
            _distanceOnGazeRay = Vector3.Distance(target.position, _filteredGazeOrigin);
            _cumulatedHandOffset = Vector3.zero;
        }

        _cumulatedHandOffset += deltaHandPos;

        Vector3 zHand_delta = Vector3.Project(deltaHandPos, _filteredGazeDir.normalized);
        Vector3 z_delta = zHand_delta.normalized * zHand_delta.magnitude * zHand_delta.magnitude * Multiplier;

        _cumulatedHandOffset += z_delta;

        target.position = _filteredGazeOrigin + _filteredGazeDir *_distanceOnGazeRay + _cumulatedHandOffset;

        Quaternion deltaRot = HandPosition.GetInstance().GetDeltaHandRotation(usePinchTip: true);
        target.rotation = deltaRot * target.rotation;
    }
}