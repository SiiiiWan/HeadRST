using System;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using UnityEngine;

public class GazeNPinchEyeHead : ManipulationTechnique
{

    public TextMeshPro text;
    [Header("Configuration")]
    public bool VisualGain;
    public bool UseGaze;
    public bool UseHead;
    public bool ForwardHeadDepthOnly;


    [Header("Fixation Filter")]
    public float FixationDuration = 0.25f; // in seconds
    public float FixationAngle = 1.5f; // in degrees

    [Header("Head Y Dispersion")]
    public float FixationDuration_HeadY = 0.25f; // in seconds
    public float FixationAngle_HeadY_High = 1f; // in degrees
    public float FixationAngle_HeadY_Low = 0.3f; // in degrees


    [Header("Depth")]
    public float MinDistance = 1f; // in meters
    public float MaxDistance = 10f; // in meters
    public float TimeInterval = 0.1f;

    bool _updateObjectPosToGazePoint;
    bool _addDepthOffsetWithHead;
    bool _isHeadYFixating;

    float _distanceOnGrab;
    float _distanceForward, _distanceBackward;
    float _depthGain_forward, _depthGain_backward;
    float _depthGain;

    float _timer = 0f;
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



        _depthGain_forward = _distanceForward / 10f;
        _depthGain_backward = _distanceBackward / 10f;

        _depthGain = Math.Max(_depthGain_backward, _depthGain_forward);

        _headY_OnFixation = headData.HeadAngle_WorldY;

        _fixationWindowSize = (int)(FixationDuration / Time.deltaTime);
        _fixationWindowSize_HeadY = (int)(FixationDuration_HeadY / Time.deltaTime);

    }

    private Vector3 _preTargetPosition;

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
        bool IsGazeFixating = GetIsFixating(gazeDirection);
        float headY = headData.HeadAngle_WorldY;
        _isHeadYFixating = GetIsHeadYFixating(headY);


        float headY_diviation = headY - _headY_OnFixation;
        bool isDivationIncreasing = headY_diviation > Math.Abs(_headY_diviation_pre);

        bool isBallisticHeadMovement = headData.HeadSpeed >= 0.2f || Math.Abs(headData.HeadAcc) >= 1f;

        bool handNotFastMoving = handData.GetHandSpeed() <= 0.1f;
        _addDepthOffsetWithHead = handNotFastMoving;
        // bool addDepthOffsetWithHead = (headY_diviation > 0 && headY_diviation > _headY_diviation_pre) || (headY_diviation < 0 && headY_diviation < _headY_diviation_pre);

        // text.text = IsHeadYFixating.ToString();
        // text.text = IsGazeFixating.ToString();


        float deltaHeadY = headData.DeltaHeadY;

        if (_updateObjectPosToGazePoint == false && (IsSaccading || Vector3.Angle(gazeDirection, _fixationCentroid) > 5f) && !IsGazeFixating) // fixation detection is frequenly swtiching between true and false, so need state switching
        {
            _updateObjectPosToGazePoint = true; // swithc to gaze state

            _addDepthOffsetWithHead = false;
        }
        else if (_updateObjectPosToGazePoint == true && IsGazeFixating)
        {
            _updateObjectPosToGazePoint = false; // switch to head hand state

            _headY_OnFixation = headY;
            _addDepthOffsetWithHead = true;
        }

        // if (_updateObjectPosToGazePoint == false)
        // {
        //     if (_isHeadYFixating) // can further add ballistic / corrective
        //     {
        //         _addDepthOffsetWithHead = false;
        //     }
        //     else
        //     {
        //         _addDepthOffsetWithHead = true;
        //     }
        // }

        target.position += handPos_delta * GetOriginGain(target);


        if (_updateObjectPosToGazePoint)
        {
            if (UseGaze)
            {
                float distance = Vector3.Distance(gazeOrigin, target.position);
                target.position = gazeOrigin + gazeDirection * distance;
                // text.text = "Gaze Point";
            }

        }
        else
        {
            // if(addDepthOffsetWithHead) target.position += (target.position - gazeOrigin).normalized * deltaHeadY * _depthGain * (isBallisticHeadMovement ? 1f : 0.25f);
            // text.text = "Not adding Depth Offset";

            // target.position += (target.position - gazeOrigin).normalized * deltaHeadY * _depthGain;

            if (_addDepthOffsetWithHead && UseHead)
            {
                // text.text = "Depth Offset";
                target.position += (target.position - gazeOrigin).normalized * deltaHeadY * _depthGain;
            }



            // if (UseHead)
            // {
            //     if (ForwardHeadDepthOnly)
            //     {
            //         if (deltaHeadY > 0) target.position += (target.position - gazeOrigin).normalized * deltaHeadY * _depthGain * (isBallisticHeadMovement ? 1f : 0.25f);
            //     }
            //     else
            //     {
            //         target.position += (target.position - gazeOrigin).normalized * deltaHeadY * _depthGain * (isBallisticHeadMovement ? 1f : 0.25f);
            //     }
            // }

            // if (Math.Abs(headY_diviation) > 2)
            // {
            //     _timer += Time.deltaTime;
            //     if (_timer >= TimeInterval)
            //     {
            //         _timer = 0f;
            //         target.position += (target.position - gazeOrigin).normalized * GetOriginGain(target) * 0.1f * (headY_diviation > 0 ? 1 : -1);
            //     }                
            // }


            // text.text = headY_diviation.ToString("F2") + "°";
        }

        float nextDistance = Vector3.Distance(target.position, gazeOrigin);
        if (nextDistance < MinDistance || nextDistance > MaxDistance) target.position = _preTargetPosition;

        target.rotation = handRot_delta * target.rotation;


        _preTargetPosition = target.position;
        _headY_diviation_pre = headY_diviation;

        text.transform.LookAt(Camera.main.transform);
        text.transform.Rotate(0, 180f, 0); // Optional: flip to face the camera properly

    }


    float GetOriginGain(Transform target)
    {
        if (VisualGain)
            return Vector3.Distance(target.position, Camera.main.transform.position) / Vector3.Distance(HandPosition.GetInstance().GetHandPosition(usePinchTip: true), Camera.main.transform.position);
        // return 1;
        return Vector3.Distance(target.position, Camera.main.transform.position);
    }

    float VitLerp(float x, float k1 = 0.1f, float k2 = 0.4f, float v1 = 0.2f, float v2 = 0.8f)
    {
        if (x <= v1)
            return k1;

        if (x >= v2)
            return k2;

        return k1 + (k2 - k1) / (v2 - v1) * (x - v1);
    }

    private Vector3 _PreviousFP, _FP;
    private float _fixationWindowSize;
    private Vector3 _fixationCentroid;
    private float _eyeInHeadY_OnFixation;
    private float _headY_OnFixation;
    private float _headY_diviation_pre;
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


    private float _fixationWindowSize_HeadY;
    private float _fixationCentroid_HeadY;
    private Queue<float> _fixationBuffer_HeadY = new Queue<float>();

    bool GetIsHeadYFixating(float headYAngle)
    {

        if (_fixationBuffer_HeadY.Count < _fixationWindowSize_HeadY)
        {
            _fixationBuffer_HeadY.Enqueue(headYAngle);
            return false;
        }

        if (_fixationBuffer_HeadY.Count == _fixationWindowSize_HeadY)
        {
            _fixationBuffer_HeadY.Enqueue(headYAngle);
        }

        while (_fixationBuffer_HeadY.Count > _fixationWindowSize_HeadY)
        {
            _fixationBuffer_HeadY.Dequeue();
        }

        float tmpCentroid = GetMeanFixationBufferHeadY();
        float dispersion = 0f;
        foreach (float angle in _fixationBuffer_HeadY)
        {
            float angle_diff = Math.Abs(angle - tmpCentroid);
            if (angle_diff > dispersion) dispersion = angle_diff;
        }

        text.text = dispersion.ToString("F2") + "°";

        if (_isHeadYFixating)
        {
            if (dispersion > FixationAngle_HeadY_High)
            {
                _fixationBuffer_HeadY.Clear();
                return false;                
            }

        }
        else
        {
            if (dispersion > FixationAngle_HeadY_Low)
            {
                _fixationBuffer_HeadY.Clear();
                return false;                
            }            
        }

        _fixationCentroid_HeadY = tmpCentroid;
        return true;
    }
    
    private float GetMeanFixationBufferHeadY()
    {
        if (_fixationBuffer_HeadY == null || _fixationBuffer_HeadY.Count == 0)
            return 0f;

        float sum = 0f;
        foreach (float val in _fixationBuffer_HeadY)
        {
            sum += val;
        }
        return sum / _fixationBuffer_HeadY.Count;
    }

}