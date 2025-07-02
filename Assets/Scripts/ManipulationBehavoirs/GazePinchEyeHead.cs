using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GazeNPinchEyeHead : ManipulationTechnique
{

    [Header("Fixation Filter")]
    public float FixationDuration = 0.25f; // in seconds
    public float FixationAngle = 1.5f; // in degrees

    [Header("Depth Range")]
    public float MinDistance = 1f; // in meters
    public float MaxDistance = 10f; // in meters

    bool _updateObjectPosToGazePoint;
    float _distanceOnGrab;
    float _distanceForward, _distanceBackward;
    float _depthGain_forward, _depthGain_backward;
    float _depthGain;
    public override void OnSingleHandGrabbed(Transform target)
    {
        HandPosition handData = HandPosition.GetInstance();
        EyeGaze gazeData = EyeGaze.GetInstance();
        HeadMovement headData = HeadMovement.GetInstance();

        Vector3 gazeOrigin = gazeData.GetGazeRay().origin;
        _distanceOnGrab = Vector3.Distance(target.position, gazeOrigin);

        if (_distanceOnGrab < MinDistance)
        {
            _distanceOnGrab = MinDistance;
        }
        else if (_distanceOnGrab > MaxDistance)
        {
            _distanceOnGrab = MaxDistance;
        }

        _distanceForward = MaxDistance - _distanceOnGrab;
        _distanceBackward = _distanceOnGrab - MinDistance;



        _depthGain_forward = _distanceForward / 20f;
        _depthGain_backward = _distanceBackward / 20f;

        _depthGain = Math.Max(_depthGain_backward, _depthGain_forward);

    }
    

    public override void ApplySingleHandGrabbedBehaviour(Transform target)
    {
        HandPosition handData = HandPosition.GetInstance();
        EyeGaze gazeData = EyeGaze.GetInstance();
        HeadMovement headData = HeadMovement.GetInstance();

        Vector3 gazeOrigin = gazeData.GetGazeRay().origin;
        Vector3 gazeDirection = gazeData.GetGazeRay().direction;


        Vector3 handPos_delta = handData.GetDeltaHandPosition(usePinchTip: true);
        Quaternion handRot_delta = handData.GetDeltaHandRotation(usePinchTip: true);

        Vector3 handPos = handData.GetHandPosition(usePinchTip: true);

        bool IsSaccading = gazeData.IsSaccading();
        bool IsFixating = GetIsFixating(gazeDirection);

        if (_updateObjectPosToGazePoint == false && IsSaccading)
        {
            _updateObjectPosToGazePoint = true;
        }
        else if (_updateObjectPosToGazePoint == true && IsFixating)
        {
            _updateObjectPosToGazePoint = false;
        }



        bool isBallisticHeadMovement = headData.HeadSpeed >= 0.2f || Math.Abs(headData.HeadAcc) >= 1f;
        bool handNotFastMoving = handData.GetHandSpeed() <= 0.2f;
        bool addDepthOffsetWithHead = handNotFastMoving;
        float deltaHeadY = headData.DeltaHeadY;

        target.position += handPos_delta * GetOriginGain(target);


        if (_updateObjectPosToGazePoint)
        {
            float distance = Vector3.Distance(gazeOrigin, target.position);
            target.position = gazeOrigin + gazeDirection * distance;
        }
        else
        {
            if (addDepthOffsetWithHead) target.position += _fixationCentroid * deltaHeadY * _depthGain * (isBallisticHeadMovement ? 1f : 0.5f);
        }
        
        float nextDistance = Vector3.Distance(target.position, gazeOrigin);
        Vector3 nextTargetDirection = (target.position - gazeOrigin).normalized;
        if(nextDistance < MinDistance)
        {
            target.position = gazeOrigin + nextTargetDirection * MinDistance;
        }
        else if (nextDistance > MaxDistance)
        {
            target.position = gazeOrigin + nextTargetDirection * MaxDistance;
        }

        target.rotation = handRot_delta * target.rotation;
    }

    
    float GetOriginGain(Transform target)
    {
        // return Vector3.Distance(target.position, Camera.main.transform.position) / Vector3.Distance(HandPosition.GetInstance().GetHandPosition(usePinchTip: true), Camera.main.transform.position);
        // return 1;
        return Vector3.Distance(target.position, Camera.main.transform.position);
    }

    private Vector3 _PreviousFP, _FP;
    private float _fixationWindowSize;
    private Vector3 _fixationCentroid;
    private Queue<Vector3> _fixationDirBuffer = new Queue<Vector3>();

    bool GetIsFixating(Vector3 gazeDir)
    {
        _fixationWindowSize = (int)(FixationDuration / Time.deltaTime);

        if (_fixationDirBuffer.Count <= _fixationWindowSize)
        {
            _fixationDirBuffer.Enqueue(gazeDir);
            return false;
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
            return false;
        }
        
        _fixationCentroid = tmpCentroid;
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