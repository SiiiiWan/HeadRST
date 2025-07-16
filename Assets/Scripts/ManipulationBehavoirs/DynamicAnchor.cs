using System;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using UnityEngine;

public class DynamicAnchor : ManipulationTechnique
{

    [Header("Gaze Filter")]

    public float FilterFrequency = 90f;
    public float FilterMinCutOff = 0.9f;
    public float FilterBeta = 15f;
    public float FilterDcutoff = 1f;

    [Header("Fixation Filter")]
    public float FixationDuration = 0.25f; // in seconds
    public float FixationAngle = 1.5f; // in degrees

    [Header("Depth")]
    public float MinDistance = 1f; // in meters
    public float MaxDistance = 10f; // in meters



    private Vector3 _filteredGazeDir;
    private Vector3 _filteredGazeOrigin;

    private OneEuroFilter<Vector3> _gazeDirFilter;
    private OneEuroFilter<Vector3> _gazePosFilter;

    void Awake()
    {
        _gazeDirFilter = new OneEuroFilter<Vector3>(FilterFrequency);
        _gazePosFilter = new OneEuroFilter<Vector3>(FilterFrequency);
    }


    public override void OnSingleHandGrabbed(Transform target)
    {
        _cumulatedHandOffset = Vector3.zero;
        _fixationWindowSize = (int)(FixationDuration / Time.deltaTime);

        EyeGaze gazeData = EyeGaze.GetInstance();
        _headDepth = Vector3.Distance(target.position, gazeData.GetGazeRay().origin);
    }

    private Vector3 _preTargetPosition;
    private float _headDepth;
    private Vector3 _cumulatedHandOffset;

    public override void ApplySingleHandGrabbedBehaviour(Transform target)
    {
        HandPosition handData = HandPosition.GetInstance();
        EyeGaze gazeData = EyeGaze.GetInstance();
        HeadMovement headData = HeadMovement.GetInstance();

        _gazeDirFilter.UpdateParams(FilterFrequency, FilterMinCutOff, FilterBeta, FilterDcutoff);
        _gazePosFilter.UpdateParams(FilterFrequency, FilterMinCutOff, FilterBeta, FilterDcutoff);


        _filteredGazeDir = _gazeDirFilter.Filter(gazeData.GetRawGazeDirection());
        _filteredGazeOrigin = _gazePosFilter.Filter(gazeData.GetRawGazeOrigin());

        Vector3 handPos_delta = handData.GetDeltaHandPosition(usePinchTip: false);
        Quaternion handRot_delta = handData.GetDeltaHandRotation(usePinchTip: true);

        bool IsSaccading = gazeData.IsSaccading();
        bool IsGazeFixating = GetIsFixating(_filteredGazeDir);

        bool gazeMoving = Vector3.Angle(_filteredGazeDir, _fixationCentroid) > 5f && !IsGazeFixating;

        _cumulatedHandOffset += handPos_delta * GetVisualGain(target);
        _headDepth += headData.DeltaHeadY * VitLerp(headData.HeadSpeed, 0.4f / 3f, 0.4f, 0.2f, 0.6f);

        if (gazeMoving) _cumulatedHandOffset = Vector3.zero;

        target.position = _filteredGazeOrigin + _filteredGazeDir * _headDepth + _cumulatedHandOffset;
        target.rotation = handRot_delta * target.rotation;

        float nextDistance = Vector3.Distance(target.position, _filteredGazeOrigin);
        if (nextDistance < MinDistance || nextDistance > MaxDistance) target.position = _preTargetPosition;

        _preTargetPosition = target.position;
    }


    float GetVisualGain(Transform target)
    {
        EyeGaze gazeData = EyeGaze.GetInstance();
        Vector3 gazeOrigin = gazeData.GetGazeRay().origin;
        Vector3 handPosition = HandPosition.GetInstance().GetHandPosition(usePinchTip: false);

        return Vector3.Distance(target.position, gazeOrigin) / Vector3.Distance(handPosition, gazeOrigin);
    }

    float VitLerp(float x, float k1 = 0.1f, float k2 = 0.4f, float v1 = 0.2f, float v2 = 0.8f)
    {
        if (x <= v1)
            return k1;

        if (x >= v2)
            return k2;

        return k1 + (k2 - k1) / (v2 - v1) * (x - v1);
    }

    private float _fixationWindowSize;
    private Vector3 _fixationCentroid;
    private Queue<Vector3> _fixationDirBuffer = new Queue<Vector3>();

    bool GetIsFixating(Vector3 gazeDir)
    {
        // adopted from spatial gaze marker, moved window size calculation to OnSingleHandGrabbed to avoid recalculating every frame (makes it more stable as the size changes every frame)
        // when buffer is filled, not return false immediately
        if (_fixationDirBuffer.Count < _fixationWindowSize)
        {
            // print("False by not filling the buffer: " + _fixationDirBuffer.Count + "/" + _fixationWindowSize);
            _fixationDirBuffer.Enqueue(gazeDir);
            return false;
        }

        if (_fixationDirBuffer.Count == _fixationWindowSize)
        {
            _fixationDirBuffer.Enqueue(gazeDir);
        }

        while (_fixationDirBuffer.Count > _fixationWindowSize)
        {
            _fixationDirBuffer.Dequeue();
        }

        Vector3 tmpCentroid = GetFixationDirCentroid();
        float gazeDispersion = 0f;
        foreach (Vector3 dir in _fixationDirBuffer)
        {
            float angle = Vector3.Angle(dir, tmpCentroid);
            if (angle > gazeDispersion) gazeDispersion = angle;
        }

        if (gazeDispersion > FixationAngle)
        {
            _fixationDirBuffer.Clear();
            // print("False by dispersion");
            return false;
        }

        _fixationCentroid = tmpCentroid;
        // print("True: " + gazeDispersion.ToString("F2") + "°");

        return true;
    }

    public Vector3 GetFixationDirCentroid()
    {
        if (_fixationDirBuffer == null || _fixationDirBuffer.Count == 0)
            return Vector3.zero;

        Vector3 sum = Vector3.zero;
        foreach (var dir in _fixationDirBuffer)
        {
            sum += dir.normalized;
        }
        Vector3 centroid = sum / _fixationDirBuffer.Count;
        return centroid.normalized;
    }


}