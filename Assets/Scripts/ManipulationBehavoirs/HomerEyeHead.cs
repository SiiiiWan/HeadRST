using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class HomerEyeHead : ManipulationTechnique
{

    [Header("Fixation Filter")]
    public float FixationDuration = 0.25f; // in seconds
    public float FixationAngle = 1.5f; // in degrees

    [Header("Depth Range")]
    public float MinDistance = 1f; // in meters
    public float MaxDistance = 10f; // in meters


    [Header("PRISM Parameters")]
    public float scalingConstant = 0.15f; //TODO: fine parameters from papers
    public float minVelocityThreshold = 0.01f;


    [Header("HOMER Parameters")]
    public float TorsoOffset = 0.35f;

    bool _updateObjectPosToGazePoint;
    float _distanceOnGrab;
    float _distanceForward, _distanceBackward;
    float _depthGain_forward, _depthGain_backward;

    private Vector3 lastHandPosition;

    public override void OnSingleHandGrabbed(Transform target)
    {
        HandPosition handData = HandPosition.GetInstance();
        EyeGaze gazeData = EyeGaze.GetInstance();
        HeadMovement headData = HeadMovement.GetInstance();
        Vector3 handPos = handData.GetHandPosition(usePinchTip: true);

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


        StartManipulation_HOMER(GetCurrentTorsoPosition(), handPos, target.position);
        lastHandPosition = handPos;
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

        float distance = Vector3.Distance(gazeOrigin, target.position);
        float depthGain = Math.Max(_depthGain_backward, _depthGain_forward);

        if (_updateObjectPosToGazePoint)
        {
            target.position = gazeOrigin + gazeDirection.normalized * distance;
            StartManipulation_HOMER(GetCurrentTorsoPosition(), handPos, target.position);
        }
        else
        {

            if (addDepthOffsetWithHead) _offsetVector += _fixationCentroid * deltaHeadY * depthGain * (isBallisticHeadMovement ? 1f : 0.5f);


            Vector3 handVelocity = handPos_delta / Time.deltaTime;
            float handSpeed = handVelocity.magnitude;

            float scaleFactor = Mathf.Min(1.2f, handSpeed / scalingConstant);
            if (handSpeed < minVelocityThreshold) scaleFactor = 0f;

            Vector3 scaledHandMovement = handPos_delta * scaleFactor;
            Vector3 scaledHandPosition = lastHandPosition + scaledHandMovement;

            Vector3 nextObjectPosition = UpdateObjectPosition_HOMER(scaledHandPosition);

            float nextDistance = Vector3.Distance(nextObjectPosition, gazeOrigin);
            if (nextDistance <= MaxDistance && nextDistance >= MinDistance)
            {
                target.position = nextObjectPosition;
            }

        }

        lastHandPosition = handPos;
        target.rotation = handRot_delta * target.rotation;
    }


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

    private Vector3 _torsoPosition_init;
    private Vector3 _handPosition_init;
    private Vector3 _objectPosition_init;
    private float _distanceHandToTorso_init;
    private float _distanceObjectToTorso_init;
    private Vector3 _offsetVector;

    public void StartManipulation_HOMER(Vector3 torso, Vector3 hand, Vector3 objectPosition)
    {
        // Difference between HOMER and visual gain is that HOMER handle the depth translation
        _torsoPosition_init = torso;
        _handPosition_init = hand;
        _objectPosition_init = objectPosition;

        _distanceHandToTorso_init = Vector3.Distance(_torsoPosition_init, _handPosition_init);
        _distanceObjectToTorso_init = Vector3.Distance(_torsoPosition_init, _objectPosition_init);

        Vector3 torso_to_hand_Direction_init = (_handPosition_init - _torsoPosition_init).normalized;
        Vector3 expectedObjectPosition = _torsoPosition_init + torso_to_hand_Direction_init * _distanceObjectToTorso_init;
        _offsetVector = _objectPosition_init - expectedObjectPosition;
    }

    Vector3 GetCurrentTorsoPosition()
    {
        return Camera.main.transform.position + Vector3.down * TorsoOffset;
    }

    public Vector3 UpdateObjectPosition_HOMER(Vector3 currentHandPosition)
    {
        Vector3 currentTorsoPosition = GetCurrentTorsoPosition();
        float distanceHandToTorso_current = Vector3.Distance(currentTorsoPosition, currentHandPosition);
        float scaledDistance = _distanceObjectToTorso_init / _distanceHandToTorso_init * distanceHandToTorso_current;

        Vector3 torso_to_hand_Direction_current = (currentHandPosition - currentTorsoPosition).normalized;
        Vector3 objectPosition = currentTorsoPosition + torso_to_hand_Direction_current * scaledDistance + _offsetVector;
        return objectPosition;
    }

}