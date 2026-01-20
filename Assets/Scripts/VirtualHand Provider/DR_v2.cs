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

    public bool TestMode;
    public float TestGain = 2f;
    public Transform TestOrigin;
    public bool UseTestOrigin = false;

    [Header("Visualizations")]
    public GameObject RedirectedPivotPoint;
    public GameObject MidPoint_OnRedirection, MidPoint_RealTime, LocalPivotPoint;
    public GameObject RealHand_Right_Visual1, RealHand_Left_Visual1, RealHand_Right_Visual2, RealHand_Left_Visual2;

    Linescript _rightRealHandLine, _leftRealHandLine, _rightVirtualHandLine, _leftVirtualHandLine, _originToPivotLine, _originToHandMidRefLine, _originToHandMidRefLine_Proj, _originToHand_project;
    Linescript _twoHandConncectionLine, _twoVirutalHandConncectionLine;

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

        if(TestMode)
        {
            if(GazeData.IsFixating_DT() && _currentMode == DR_States.Gaze)
            {
                _currentMode = DR_States.Hand;
            }

            if(Input.GetKeyDown(KeyCode.Space))
            {
                _currentMode = DR_States.Gaze;
            }
        }
        else
        {
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
        _pivot_redirected = UpdatePivot(_pivot_redirected);
        Vector3 handMidpoint_realTime = (HandData.RightHandPosition + HandData.LeftHandPosition) / 2f;
        Vector3 pivot_local = handMidpoint_realTime;

        Vector3 viewPoint = UseTestOrigin && TestMode ? TestOrigin.position : HeadData.HeadPosition;
        
        Vector3 vec_gazeToLocalPivot = pivot_local - viewPoint;
        Vector3 vec_gazeToRedirectedPivot = _pivot_redirected - viewPoint;

        Quaternion redirectionOffset = Quaternion.LookRotation(vec_gazeToRedirectedPivot) * Quaternion.Inverse(Quaternion.LookRotation(MathFunctions.ProjectOntoXZPlane(vec_gazeToLocalPivot)));
        
        Quaternion rightVirtualHandRotation = redirectionOffset * HandData.RightHandRotation;
        Quaternion leftVirtualHandRotation = redirectionOffset * HandData.LeftHandRotation;

        float handScaleFactor = 1f;
        if(TestMode)
        {
            handScaleFactor = TestGain;
        }

        Vector3 rightPinchPosition = HandData.RightPinchTipPosition;
        Vector3 leftPinchPosition = HandData.LeftPinchTipPosition;
        float pinchDistance = Vector3.Distance(rightPinchPosition, leftPinchPosition);
        float t = Mathf.InverseLerp(0.01f, 0.15f, pinchDistance);
        float regulatedGainFactor = Mathf.Lerp(1f, handScaleFactor, t);

        Vector3 rightVirtualHandPosition = _pivot_redirected + redirectionOffset * (HandData.RightHandPosition - pivot_local) * regulatedGainFactor;
        Vector3 leftVirtualHandPosition = _pivot_redirected + redirectionOffset * (HandData.LeftHandPosition - pivot_local) * regulatedGainFactor;

        if(TestMode)
        {
            RealHand_Left_Visual1.SetActive(true);
            RealHand_Left_Visual2.SetActive(true);
            RealHand_Right_Visual1.SetActive(true);
            RealHand_Right_Visual2.SetActive(true);

            // Visulaizations
            RedirectedPivotPoint.transform.position = _pivot_redirected;
            LocalPivotPoint.transform.position = pivot_local;
            MidPoint_OnRedirection.transform.position = _handMidpointPosition_OnRedirection;
            MidPoint_RealTime.transform.position = handMidpoint_realTime;

            if (_rightRealHandLine == null) _rightRealHandLine = new Linescript(0.01f, transform);
            _rightRealHandLine.SetPosition(pivot_local, HandData.RightHandPosition);

            if (_leftRealHandLine == null) _leftRealHandLine = new Linescript(0.01f, transform);
            _leftRealHandLine.SetPosition(pivot_local, HandData.LeftHandPosition);

            if (_rightVirtualHandLine == null) _rightVirtualHandLine = new Linescript(0.01f, transform);
            _rightVirtualHandLine.SetPosition(_pivot_redirected, rightVirtualHandPosition);

            if (_leftVirtualHandLine == null) _leftVirtualHandLine = new Linescript(0.01f, transform);
            _leftVirtualHandLine.SetPosition(_pivot_redirected, leftVirtualHandPosition);

            if (_originToPivotLine == null) _originToPivotLine = new Linescript(0.01f, transform, Color.blue);
            _originToPivotLine.SetPosition(viewPoint, _pivot_redirected);

            if (_originToHandMidRefLine == null) _originToHandMidRefLine = new Linescript(0.01f, transform);
            _originToHandMidRefLine.SetPosition(viewPoint, pivot_local);

            if (_originToHandMidRefLine_Proj == null) _originToHandMidRefLine_Proj = new Linescript(0.01f, transform, Color.blue);
            _originToHandMidRefLine_Proj.SetPosition(viewPoint, viewPoint + MathFunctions.ProjectOntoXZPlane(vec_gazeToLocalPivot));

            if (_originToHand_project == null) _originToHand_project = new Linescript(0.01f, transform);
            _originToHand_project.SetPosition(pivot_local, viewPoint + MathFunctions.ProjectOntoXZPlane(vec_gazeToLocalPivot));

            if (_twoHandConncectionLine == null) _twoHandConncectionLine = new Linescript(0.01f, transform, Color.yellow);
            _twoHandConncectionLine.SetPosition(HandData.RightHandPosition, HandData.LeftHandPosition);

            if (_twoVirutalHandConncectionLine == null) _twoVirutalHandConncectionLine = new Linescript(0.01f, transform);
            _twoVirutalHandConncectionLine.SetPosition(rightVirtualHandPosition, leftVirtualHandPosition); 

            ShowText(ref _pinchDistance, (rightPinchPosition + leftPinchPosition) / 2f, Vector3.Distance(rightPinchPosition, leftPinchPosition).ToString("F2") + " m");
        }


        if(isRightHand)
        {
            return new Pose(rightVirtualHandPosition, rightVirtualHandRotation);
        }
        else
        {
            return new Pose(leftVirtualHandPosition, leftVirtualHandRotation);
        }

        // Vector3 vec_RightHand_To_RealTimeMid = handMidpoint_realTime - HandData.RightHandPosition;
        // Vector3 vec_LeftHand_To_RealTimeMid = handMidpoint_realTime - HandData.LeftHandPosition;
        // Vector3 vec_RightHand_To_OnRedirMid = _handMidpointPosition_OnRedirection - HandData.RightHandPosition;
        // Vector3 vec_LeftHand_To_OnRedirMid = _handMidpointPosition_OnRedirection - HandData.LeftHandPosition;
        // Vector3 vec_OnRedirMid_To_LeftHand = HandData.LeftHandPosition - _handMidpointPosition_OnRedirection;
        // Vector3 vec_OnRedirMid_To_RightHand = HandData.RightHandPosition - _handMidpointPosition_OnRedirection;

        // float angleRightHand = Vector3.Angle(vec_RightHand_To_OnRedirMid, vec_RightHand_To_RealTimeMid);
        // float angleLeftHand = Vector3.Angle(vec_LeftHand_To_OnRedirMid, vec_LeftHand_To_RealTimeMid);
        // float angleOnRedirMid = Vector3.Angle(vec_OnRedirMid_To_RightHand, vec_OnRedirMid_To_LeftHand);
        // if(angleRightHand > angleOnRedirMid || angleLeftHand > angleOnRedirMid)
        // {
        //     _handMidpointPosition_OnRedirection = handMidpoint_realTime;
        // }

        // Quaternion rightHandOffset_theta = Quaternion.LookRotation(vec_gazeToRightHand) * Quaternion.Inverse(Quaternion.LookRotation(vec_gazeToHandMidpoint));
        // Quaternion leftHandOffset_theta = Quaternion.LookRotation(vec_gazeToLeftHand) * Quaternion.Inverse(Quaternion.LookRotation(vec_gazeToHandMidpoint));
        // Vector3 rightVirtualHandForward = (rightHandOffset_theta * vec_headToPivot).normalized;
        // Vector3 leftVirtualHandForward = (leftHandOffset_theta * vec_headToPivot).normalized;

        // float rightHandOffset_r = vec_gazeToRightHand.magnitude - vec_gazeToHandMidpoint.magnitude;
        // float leftHandOffset_r = vec_gazeToLeftHand.magnitude - vec_gazeToHandMidpoint.magnitude;
        // float rightHandOffset_virtual_r = vec_headToPivot.magnitude + rightHandOffset_r;
        // float leftHandOffset_virtual_r = vec_headToPivot.magnitude + leftHandOffset_r;

        // Vector3 rightVirtualHandPosition = gazeOrigin + rightVirtualHandForward * rightHandOffset_virtual_r;
        // Vector3 leftVirtualHandPosition = gazeOrigin + leftVirtualHandForward * leftHandOffset_virtual_r;

        // float handScaleFactor = vec_headToPivot.magnitude;
    }


    public override bool IsGazeRedirecting()
    {
        return _currentMode == DR_States.Gaze;
    }

    private TextMesh _distanceText;
    private TextMesh _pinchDistance;
    void ShowText(ref TextMesh textMesh, Vector3 position, string content)
    {
        if (textMesh == null)
        {
            GameObject textObject = new GameObject();
            textObject.transform.parent = transform;
            textMesh = textObject.AddComponent<TextMesh>();
            textMesh.fontSize = 50;
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;
            textMesh.characterSize = 0.01f;
        }

        textMesh.transform.position = position;
        textMesh.text = content;

        // Orient the text towards the camera
        if (Camera.main != null)
        {
            textMesh.transform.LookAt(Camera.main.transform);
            textMesh.transform.Rotate(0, 180, 0);
        }
    }


}
