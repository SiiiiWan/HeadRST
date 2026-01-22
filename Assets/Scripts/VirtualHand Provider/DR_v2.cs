using System;
using Oculus.Interaction.Body.Input;
using UltimateProceduralPrimitivesFREE;
using Unity.Android.Gradle.Manifest;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using static OVRPlugin;

public class DR_v2 : VirtualHandProvider
{
    enum DR_States
    {
        Gaze,
        Head,
        Hand
    }
    private DR_States _currentMode = DR_States.Gaze;

    private Vector3 _handMidpointPosition_OnRedirection, _torsoPosition_OnRedirection;
    private Quaternion _redirectionOffset_OnRedirection;
    private bool _isGazeFixation_prev;
    private float _headPitch_neutral;

    public bool TestMode;
    private float _scalingFactor = 1f;
    public float TorsoRotationOffset = 0f;
    public Transform TestOrigin;
    public bool UseTestOrigin = false;

    [Header("Visualizations")]
    public GameObject RedirectedPivotPoint;
    public GameObject MidPoint_OnRedirection, MidPoint_RealTime, LocalPivotPoint;
    public GameObject RealHand_Right_Visual1, RealHand_Left_Visual1, RealHand_Right_Visual2, RealHand_Left_Visual2;

    Linescript _rightRealHandLine, _leftRealHandLine, _rightVirtualHandLine, _leftVirtualHandLine, _rightVirtualElbowLine, _leftElbowLine, _rightElbowLine, _leftVirtualElbowLine;
    Linescript _virtualTorsoForwardLine, _torsoForwardLine, _torsoLine, _torsoLeftHandLine, _torsoRightHandLine;

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

            if(Input.GetKey(KeyCode.DownArrow))
            {
                Vector3 directionFromGazeOrigin = (currentPosition - gazeOrigin).normalized;
                currentPosition -= directionFromGazeOrigin * MathFunctions.Deg2Meter(Time.deltaTime, Vector3.Distance(currentPosition, gazeOrigin)) * 20f;
            }

            if(Input.GetKey(KeyCode.UpArrow))
            {
                Vector3 directionFromGazeOrigin = (currentPosition - gazeOrigin).normalized;
                currentPosition += directionFromGazeOrigin * MathFunctions.Deg2Meter(Time.deltaTime, Vector3.Distance(currentPosition, gazeOrigin)) * 20f;
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

        Posef torsoPose = BodyData.BodyState?.JointLocations[5].Pose ?? default;
        Vector3 torsoPosition = new Vector3(torsoPose.Position.x, torsoPose.Position.y, -torsoPose.Position.z);
        _torsoPosition_OnRedirection = torsoPosition;

        Quaternion torso_rotation =  new Quaternion(torsoPose.Orientation.x, torsoPose.Orientation.y, torsoPose.Orientation.z, torsoPose.Orientation.w);
        torso_rotation = Quaternion.Euler(
            torso_rotation.eulerAngles.x,
            -torso_rotation.eulerAngles.y + 90,
            torso_rotation.eulerAngles.z);
        Vector3 torso_forward = (torso_rotation * Vector3.forward).normalized;


        // TODO: change 15 deg to hand distance

        if(_currentMode == DR_States.Gaze)
        {
            _headPitch_neutral = HeadData.HeadAngle_WorldY;
        } 

        if(_currentMode != DR_States.Hand)
        {
            _handMidpointPosition_OnRedirection = HandData.HandMidPosition;

            Vector3 localRefForward = torso_forward;
            Vector3 reDirectedRefPosition = _pivot_redirected;
            Vector3 reDirectedRefForward = (reDirectedRefPosition - HeadData.HeadPosition).normalized;
            _redirectionOffset_OnRedirection = Quaternion.LookRotation(reDirectedRefForward) * Quaternion.Inverse(Quaternion.LookRotation(MathFunctions.ProjectOntoXZPlane(localRefForward)));
        }

        if(GazeData.IsFixating_DT() == false) _headPitch_neutral = HeadData.HeadAngle_WorldY;

        // ShowCurrentMode();

        switch (_currentMode)
        {
            case DR_States.Gaze:
                currentPosition = gazeOrigin + gazeDirection * Vector3.Distance(GazeData.GetGazeHitPoint_Sphere(out Vector3 hitPoint, 0.2f) ? hitPoint : currentPosition, gazeOrigin);
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
                {
                    currentPosition += torsoPosition -_torsoPosition_prev;
                    break;
                }

            default:
                // currentPosition remains unchanged
                break;
        }

        _isGazeFixation_prev = GazeData.IsFixating_DT();

        return currentPosition;
    }

    Vector3 _torsoPosition_prev;

    // public override float GetVirtualHandScalingFactor()
    // {
    //     return _scalingFactor;
    // }

    public override Pose GetVirtualHandPose(bool isRightHand)
    {
        _pivot_redirected = UpdatePivot(_pivot_redirected);

        Posef rightElbowPose = BodyData.BodyState?.JointLocations[16].Pose ?? default;
        Vector3 rightElbowPosition = new Vector3(rightElbowPose.Position.x, rightElbowPose.Position.y, -rightElbowPose.Position.z);
        Posef leftElbowPose = BodyData.BodyState?.JointLocations[11].Pose ?? default;
        Vector3 leftElbowPosition = new Vector3(leftElbowPose.Position.x, leftElbowPose.Position.y, -leftElbowPose.Position.z);
        Posef torsoPose = BodyData.BodyState?.JointLocations[5].Pose ?? default;
        Vector3 torsoPosition = new Vector3(torsoPose.Position.x, torsoPose.Position.y, -torsoPose.Position.z);
        
        Quaternion torso_rotation =  new Quaternion(torsoPose.Orientation.x, torsoPose.Orientation.y, torsoPose.Orientation.z, torsoPose.Orientation.w);
        torso_rotation = Quaternion.Euler(
            torso_rotation.eulerAngles.x,
            -torso_rotation.eulerAngles.y + 90,
            torso_rotation.eulerAngles.z);
        Vector3 torso_forward = (torso_rotation * Vector3.forward).normalized;

        Vector3 localRefPosition = torsoPosition;
        Vector3 localRefForward = torso_forward;
        // Vector3 reDirectedRefPosition = (_pivot_redirected - HeadData.HeadPosition) * (1 - (_handMidpointPosition_OnRedirection - torsoPosition).magnitude / (_pivot_redirected - HeadData.HeadPosition).magnitude);
        Vector3 reDirectedRefPosition = _pivot_redirected;
        Vector3 reDirectedRefForward = (reDirectedRefPosition - HeadData.HeadPosition).normalized;

        // _scalingFactor = Vector3.Distance(reDirectedRefPosition, HeadData.HeadPosition);
        _scalingFactor = 1;


        Quaternion redirectionRotationOffset = Quaternion.LookRotation(reDirectedRefForward) * Quaternion.Inverse(Quaternion.LookRotation(MathFunctions.ProjectOntoXZPlane(localRefForward)));
        reDirectedRefPosition = reDirectedRefPosition + _redirectionOffset_OnRedirection * (_torsoPosition_OnRedirection - _handMidpointPosition_OnRedirection);
        // torso_forward = new Vector3(
        //     torso_forward.x,
        //     -torso_forward.y + 90,
        //     torso_forward.z);

        // if(_currentMode == DR_States.Hand)
        // {
        //     _pivot_redirected += torsoPosition - _torsoPosition_prev;
        // }

        Vector3 rightHandPosition = HandData.RightHandPosition;
        Vector3 leftHandPosition = HandData.LeftHandPosition;
        Vector3 leftVirtualElbowPosition = reDirectedRefPosition + redirectionRotationOffset * ((leftElbowPosition - torsoPosition) * _scalingFactor);
        Vector3 rightVirtualElbowPosition = reDirectedRefPosition + redirectionRotationOffset * ((rightElbowPosition - torsoPosition) * _scalingFactor);
        Vector3 leftVirtualHandPosition = leftVirtualElbowPosition + redirectionRotationOffset * ((leftHandPosition - leftElbowPosition) * _scalingFactor);
        Vector3 rightVirtualHandPosition = rightVirtualElbowPosition + redirectionRotationOffset * ((rightHandPosition - rightElbowPosition) * _scalingFactor);

        // Vector3 vec_gazeToLocalPivot = pivot_local - viewPoint;
        // Vector3 vec_gazeToRedirectedPivot = pivot_world - viewPoint;
        // Quaternion redirectionOffset = Quaternion.LookRotation(vec_gazeToRedirectedPivot) * Quaternion.Inverse(Quaternion.LookRotation(MathFunctions.ProjectOntoXZPlane(vec_gazeToLocalPivot)));
        // Quaternion redirectionPositionOffset = Quaternion.LookRotation(vec_gazeToRedirectedPivot) * Quaternion.Inverse(Quaternion.LookRotation(HandData.RightHandPosition - rightElbowPosition));    


        // if(_currentMode == DR_States.Hand)
        // {
        //     _pivot_redirected += redirectionPositionOffset * HandData.RightHandPosition_delta;
        // }        

        // detect the condition during gaze saccade
        // if(Vector3.Angle(GazeData.GetGazeDirection(), (HandData.HandMidPosition - GazeData.GetGazeOrigin()).normalized) <= MathFunctions.Meter2Deg(Vector3.Distance(HandData.RightHandPosition, HandData.LeftHandPosition), Vector3.Distance(HandData.HandMidPosition, GazeData.GetGazeOrigin())) / 2 * 1.2f)
        // {
        //     redirectionOffset = Quaternion.identity;
        //     pivot_world = pivot_local;
        // }

        // Vector3 rightVirtualHandPosition = _pivot_redirected;
        // Vector3 leftVirtualHandPosition = pivot_world + _leftHandLocalOffset;



        Quaternion rightVirtualHandRotation = redirectionRotationOffset * HandData.RightHandRotation;
        Quaternion leftVirtualHandRotation = redirectionRotationOffset * HandData.LeftHandRotation;


        if(TestMode)
        {
            RealHand_Left_Visual1.SetActive(true);
            RealHand_Left_Visual2.SetActive(true);
            RealHand_Right_Visual1.SetActive(true);
            RealHand_Right_Visual2.SetActive(true);

            // Visulaizations
            RedirectedPivotPoint.transform.position = reDirectedRefPosition;
            LocalPivotPoint.transform.position = _pivot_redirected;
            // MidPoint_OnRedirection.transform.position = _handMidpointPosition_OnRedirection;
            // MidPoint_RealTime.transform.position = HandData.HandMidPosition;

            if (_rightRealHandLine == null) _rightRealHandLine = new Linescript(0.01f, transform);
            _rightRealHandLine.SetPosition(rightElbowPosition, rightHandPosition);

            if (_leftRealHandLine == null) _leftRealHandLine = new Linescript(0.01f, transform);
            _leftRealHandLine.SetPosition(leftElbowPosition, leftHandPosition);

            if (_leftElbowLine == null) _leftElbowLine = new Linescript(0.01f, transform);
            _leftElbowLine.SetPosition(leftElbowPosition, torsoPosition);

            if (_rightElbowLine == null) _rightElbowLine = new Linescript(0.01f, transform);
            _rightElbowLine.SetPosition(rightElbowPosition, torsoPosition);

            if (_torsoForwardLine == null) _torsoForwardLine = new Linescript(0.01f, transform);
            _torsoForwardLine.SetPosition(torsoPosition, torsoPosition + torso_forward); 



            if (_rightVirtualHandLine == null) _rightVirtualHandLine = new Linescript(0.01f, transform);
            _rightVirtualHandLine.SetPosition(rightVirtualElbowPosition, rightVirtualHandPosition);

            if (_leftVirtualHandLine == null) _leftVirtualHandLine = new Linescript(0.01f, transform);
            _leftVirtualHandLine.SetPosition(leftVirtualElbowPosition, leftVirtualHandPosition);

            if (_rightVirtualElbowLine == null) _rightVirtualElbowLine = new Linescript(0.01f, transform);
            _rightVirtualElbowLine.SetPosition(rightVirtualElbowPosition, reDirectedRefPosition);

            if (_leftVirtualElbowLine == null) _leftVirtualElbowLine = new Linescript(0.01f, transform);
            _leftVirtualElbowLine.SetPosition(leftVirtualElbowPosition, reDirectedRefPosition);

            if (_virtualTorsoForwardLine == null) _virtualTorsoForwardLine = new Linescript(0.01f, transform);
            _virtualTorsoForwardLine.SetPosition(reDirectedRefPosition, reDirectedRefPosition + torso_forward);

            // if (_twoVirutalHandConncectionLine == null) _twoVirutalHandConncectionLine = new Linescript(0.01f, transform);
            // _twoVirutalHandConncectionLine.SetPosition(rightVirtualHandPosition, leftVirtualHandPosition); 

            // if (_torsoLine == null) _torsoLine = new Linescript(0.01f, transform, Color.red);
            // _torsoLine.SetPosition(torsoPosition, torsoPosition + torso_forward*radialDistance); 

            // if (_torsoLeftHandLine == null) _torsoLeftHandLine = new Linescript(0.01f, transform, Color.red);
            // _torsoLeftHandLine.SetPosition(torsoPosition, HandData.LeftHandPosition);

            // if (_torsoRightHandLine == null) _torsoRightHandLine = new Linescript(0.01f, transform, Color.red);
            // _torsoRightHandLine.SetPosition(torsoPosition, HandData.RightHandPosition); 

            // ShowText(ref _handDistance, HandData.HandMidPosition, Vector3.Distance(HandData.RightHandPosition, HandData.LeftHandPosition).ToString("F2") + " m");
        }

        _torsoPosition_prev = torsoPosition;

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
    private TextMesh _handDistance;
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
        // Quaternion torsoRotation = new Quaternion(torsoPose.Orientation.x, torsoPose.Orientation.y, torsoPose.Orientation.z, torsoPose.Orientation.w);
        // torsoRotation = Quaternion.Euler(
        //     torsoRotation.eulerAngles.x,
        //     -torsoRotation.eulerAngles.y + 90,
        //     torsoRotation.eulerAngles.z);
        // Vector3 torso_forward = (torsoRotation * Vector3.forward).normalized;

}
