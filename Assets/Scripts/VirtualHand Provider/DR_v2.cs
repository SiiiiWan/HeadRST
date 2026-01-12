using System;
using UltimateProceduralPrimitivesFREE;
using Unity.Android.Gradle.Manifest;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class DR_v2 : VirtualHandProvider
{
    enum DR_States
    {
        Gaze,
        Head,
        Hand
    }
    private DR_States _currentMode = DR_States.Gaze;

    private Vector3 _handMidpointPosition_OnRedirection;
    private bool _isGazeFixation_prev;
    private float _headPitch_neutral;

    public override Vector3 UpdatePivot(Vector3 currentPosition)
    {
        UpdateDataSource();
        Vector3 gazeOrigin = GazeData.GetGazeOrigin();
        Vector3 gazeDirection = GazeData.GetGazeDirection();
        float headPitchAngle = HeadData.HeadAngle_WorldY;
        bool isHeadPitchInceasing = (headPitchAngle - HeadData.Pre_HeadAngle_WorldY) > 0.01f;
        bool isHeadPitchDecreasing = (headPitchAngle - HeadData.Pre_HeadAngle_WorldY) < -0.01f;
        bool isEyeInHeadAngleIncreasing = (Math.Abs(GazeData.EyeInHeadYAngle) - Math.Abs(GazeData.FilteredEyeInHeadAngle_Pre)) > 0.01f;
        bool isEyeInHeadAngleDecreasing = (GazeData.EyeInHeadYAngle - GazeData.FilteredEyeInHeadAngle_Pre) < -0.01f;

        if(GazeData.IsFixating_DT() && _currentMode == DR_States.Gaze)
        {
            _currentMode = DR_States.Hand;
        }

        if(Vector3.Angle(gazeDirection, (currentPosition - gazeOrigin).normalized) > 15f && GazeData.IsFixating_DT() && _isGazeFixation_prev == false)
        {
            _currentMode = DR_States.Gaze;
        }

        if(_currentMode != DR_States.Gaze)
        {
            if((headPitchAngle - _headPitch_neutral >= 5f && isHeadPitchInceasing) || (headPitchAngle - _headPitch_neutral <= -3f && isHeadPitchDecreasing))
            {
                _currentMode = DR_States.Head;
            }
            else
            {
                _currentMode = DR_States.Hand;
            }
        }

        // TODO: change 15 deg to hand distance

        if(_currentMode == DR_States.Gaze)
        {
            _headPitch_neutral = HeadData.HeadAngle_WorldY;
            _handMidpointPosition_OnRedirection = (HandData.RightHandPosition + HandData.LeftHandPosition) / 2f;
        } 
        if(GazeData.IsFixating_DT() == false) _headPitch_neutral = HeadData.HeadAngle_WorldY;

        // ShowCurrentMode();

        switch (_currentMode)
        {
            case DR_States.Gaze:
                currentPosition = gazeOrigin + gazeDirection * Vector3.Distance(GazeData.GetGazeHitPoint(out Vector3 hitPoint) ? hitPoint : currentPosition, gazeOrigin);
                break;
            case DR_States.Head:
                {
                    Vector3 directionFromGazeOrigin = (currentPosition - gazeOrigin).normalized;
                    
                    if (headPitchAngle - _headPitch_neutral >= 5f)
                    {
                        currentPosition += directionFromGazeOrigin * (headPitchAngle - _headPitch_neutral - 5f) * MathFunctions.Deg2Meter(Time.deltaTime, Vector3.Distance(currentPosition, gazeOrigin)) * 10f;
                    }

                    if (headPitchAngle - _headPitch_neutral <= -3f)
                    {
                        currentPosition -= directionFromGazeOrigin * (-headPitchAngle + _headPitch_neutral - 3f) * MathFunctions.Deg2Meter(Time.deltaTime, Vector3.Distance(currentPosition, gazeOrigin)) * 10f;
                    }

                    currentPosition = gazeOrigin + directionFromGazeOrigin * Mathf.Clamp(Vector3.Distance(currentPosition, gazeOrigin), 1, 5);
                    break;
                }
            case DR_States.Hand:
                // currentPosition remains unchanged
                break;
            default:
                // currentPosition remains unchanged
                break;
        }

        _isGazeFixation_prev = GazeData.IsFixating_DT();

        return currentPosition;
    }

    public override Pose GetVirtualHandPose(bool isRightHand)
    {
        _currentPivotPoint = UpdatePivot(_currentPivotPoint);
        Vector3 gazeOrigin = GazeData.GetGazeOrigin();
        Vector3 vec_gazeToPivot = _currentPivotPoint - gazeOrigin;

        Vector3 vec_gazeToRightHand = HandData.RightHandPosition - gazeOrigin;
        Vector3 vec_gazeToLeftHand = HandData.LeftHandPosition - gazeOrigin;
        Vector3 vec_gazeToHandMidpoint = _handMidpointPosition_OnRedirection - gazeOrigin;
        Vector3 vec_gazeToHandMidpoing_realTime = (HandData.RightHandPosition + HandData.LeftHandPosition) / 2f - gazeOrigin;

        Quaternion rightHandOffset_theta = Quaternion.LookRotation(vec_gazeToRightHand) * Quaternion.Inverse(Quaternion.LookRotation(vec_gazeToHandMidpoint));
        Quaternion leftHandOffset_theta = Quaternion.LookRotation(vec_gazeToLeftHand) * Quaternion.Inverse(Quaternion.LookRotation(vec_gazeToHandMidpoint));
        Vector3 rightVirtualHandForward = (rightHandOffset_theta * vec_gazeToPivot).normalized;
        Vector3 leftVirtualHandForward = (leftHandOffset_theta * vec_gazeToPivot).normalized;

        float rightHandOffset_r = vec_gazeToRightHand.magnitude - vec_gazeToHandMidpoint.magnitude;
        float leftHandOffset_r = vec_gazeToLeftHand.magnitude - vec_gazeToHandMidpoint.magnitude;
        float rightHandOffset_virtual_r = vec_gazeToPivot.magnitude + rightHandOffset_r;
        float leftHandOffset_virtual_r = vec_gazeToPivot.magnitude + leftHandOffset_r;

        // Vector3 rightVirtualHandPosition = gazeOrigin + rightVirtualHandForward * rightHandOffset_virtual_r;
        // Vector3 leftVirtualHandPosition = gazeOrigin + leftVirtualHandForward * leftHandOffset_virtual_r;

        Quaternion handRotationOffset = Quaternion.LookRotation(vec_gazeToPivot) * Quaternion.Inverse(Quaternion.LookRotation(MathFunctions.ProjectOntoXZPlane(vec_gazeToHandMidpoint)));
        Quaternion rightVirtualHandRotation = handRotationOffset * HandData.RightHandRotation;
        Quaternion leftVirtualHandRotation = handRotationOffset * HandData.LeftHandRotation;

        float handScaleFactor = vec_gazeToPivot.magnitude;
        Vector3 rightVirtualHandPosition = _currentPivotPoint + handRotationOffset * (HandData.RightHandPosition - _handMidpointPosition_OnRedirection);
        Vector3 leftVirtualHandPosition = _currentPivotPoint + handRotationOffset * (HandData.LeftHandPosition - _handMidpointPosition_OnRedirection);

        if(isRightHand)
        {
            return new Pose(rightVirtualHandPosition, rightVirtualHandRotation);
        }
        else
        {
            return new Pose(leftVirtualHandPosition, leftVirtualHandRotation);
        }
    }


    public override bool IsGazeRedirecting()
    {
        return _currentMode == DR_States.Gaze;
    }

    private TextMesh _distanceText;
    void ShowCurrentMode()
    {
        if (_distanceText == null)
        {
            GameObject textObject = new GameObject("DistanceText");
            textObject.transform.parent = transform;
            _distanceText = textObject.AddComponent<TextMesh>();
            _distanceText.fontSize = 50;
            _distanceText.anchor = TextAnchor.MiddleCenter;
            _distanceText.alignment = TextAlignment.Center;
            _distanceText.characterSize = 0.01f;
        }

        _distanceText.transform.position = _currentPivotPoint;
        _distanceText.text = $"{_currentMode}"; // Format to 2 decimal places
        _distanceText.color = _currentMode == DR_States.Gaze ? Color.red : (_currentMode == DR_States.Head ? Color.green : Color.blue);

        // Orient the text towards the camera
        if (Camera.main != null)
        {
            _distanceText.transform.LookAt(Camera.main.transform);
            _distanceText.transform.Rotate(0, 180, 0);
        }
    }

}
