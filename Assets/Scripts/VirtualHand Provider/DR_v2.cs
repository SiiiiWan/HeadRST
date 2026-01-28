using System;
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

    enum HeadPitchControlState
    {
        Neutral,
        PitchUp,
        PitchDown
    }

    private DR_States _currentMode = DR_States.Gaze;
    private HeadPitchControlState _headPitchControlState;

    private bool _isGazeFixation_prev;
    private Vector3 _torsoPosition_prev;
    private float _headPitchAngle_prev;
    private Vector3 _headForward_OnRedirection;
    private float _headPitchAngle_OnRedirection;

    Vector3 _redirectedCentroid;
    Vector3 _handToTorsoOffset;

    Vector3 _leftVirtualHandPosition;
    Vector3 _rightVirtualHandPosition;

    public float PitchOffsetThr_Up = 10f;
    public float IndicatorOffset_Up = 2f;
    public float PitchOffsetThr_Down = 10f;
    public float IndicatorOffset_Down = 2f;


    public float TestGain = 1f;
    private float _torsoAmpGain;

    // Visualizations
    public GameObject RedirectedPivotPoint, HeadPitchThr_Upper, HeadPitchThr_Lower, HeadPitchPoint_Current_Local, HeadPitch_Current_Indicator;
    public DepthControlIndicator UpArrow, DownArrow;
    private Linescript _rightVirtualHandLine1, _leftVirtualHandLine1, _rightVirtualHandLine2, _leftVirtualHandLine2;
    public Material ArrowActivationMaterial, ArrowDeactivationMaterial;

    public override Pose GetVirtualHandPose(bool isRightHand)
    {
        // Get Tracking Data
        UpdateDataSource();
        
        //// Get Gaze Data
        Vector3 gazeOrigin = GazeData.GetGazeOrigin();
        Vector3 gazeDirection = GazeData.GetGazeDirection();
        
        // // Get Hand Data
        Vector3 rightHandPosition = HandData.RightHandPosition;
        Vector3 leftHandPosition = HandData.LeftHandPosition;
        Vector3 leftPalmPosition = HandData.LeftPalmPosition;
        Vector3 rightPalmPosition = HandData.RightPalmPosition;
        Vector3 midPalmPosition = (leftPalmPosition + rightPalmPosition) / 2f;

        // // Get Head Data
        float headPitchAngle = HeadData.HeadAngle_WorldY;

        // // Get Torso Data
        Posef torsoPose = BodyData.BodyState?.JointLocations[5].Pose ?? default;
        Vector3 torsoPosition = new Vector3(torsoPose.Position.x, torsoPose.Position.y, -torsoPose.Position.z);
        Quaternion torso_rotation =  new Quaternion(torsoPose.Orientation.x, torsoPose.Orientation.y, torsoPose.Orientation.z, torsoPose.Orientation.w);
        torso_rotation = Quaternion.Euler(
            torso_rotation.eulerAngles.x,
            -torso_rotation.eulerAngles.y + 90,
            torso_rotation.eulerAngles.z);
        Vector3 torso_forward = (torso_rotation * Vector3.forward).normalized;

        // Mode Switching Logic
        // // Gaze -> Hand
        if(GazeData.IsFixating_DT() && _currentMode == DR_States.Gaze)
        {
            _currentMode = DR_States.Hand;
        }

        // // Hand -> Gaze
        Vector3 virtualHandMidpoint = (_leftVirtualHandPosition + _rightVirtualHandPosition) / 2f;
        float distanceBewteenVirualHands = Vector3.Distance(_leftVirtualHandPosition, _rightVirtualHandPosition);
        float distanceToVirtualHandMidpoint = Vector3.Distance(gazeOrigin, virtualHandMidpoint);
        // if(Input.GetKeyDown(KeyCode.Space))
        if(Vector3.Angle(gazeDirection, (virtualHandMidpoint - gazeOrigin).normalized) > MathFunctions.Meter2Deg(distanceBewteenVirualHands, distanceToVirtualHandMidpoint) / 2 * 1.5f && GazeData.IsFixating_DT() && _isGazeFixation_prev == false)
        {
            _currentMode = DR_States.Gaze;
        }


        // Mode Effects
        // // Gaze mode
        if(_currentMode == DR_States.Gaze)
        {
            // Redirect centriod to gaze point during 
            if(GazeData.GetGazeHitPoint_Sphere(out RaycastHit hit, Vector3.Distance(leftPalmPosition, rightPalmPosition)/2))
            {
                _redirectedCentroid = gazeOrigin + (hit.point - gazeOrigin).normalized * Mathf.Clamp(Vector3.Distance(hit.point, gazeOrigin), 1, 5);          
            }
            else
            {
                _redirectedCentroid = gazeOrigin + gazeDirection * Mathf.Clamp(Vector3.Distance(_redirectedCentroid, gazeOrigin), 1, 5);   
            }       
            
            // Update During Gaze Mode
            _headForward_OnRedirection = HeadData.HeadForward;
            _headPitchAngle_OnRedirection = HeadData.HeadAngle_WorldY;
            _handToTorsoOffset = HandData.HandMidPosition - torsoPosition;
            _torsoAmpGain = Vector3.Distance(torsoPosition, _redirectedCentroid);
        }

        // Calculate the depth axis
        Vector3 directionFromGazeOrigin = (_redirectedCentroid - gazeOrigin).normalized;

        // Head Pitch Depth Adjustment
        _headPitchControlState = HeadPitchControlState.Neutral;
        float headPitchOffset = headPitchAngle - _headPitchAngle_OnRedirection;
        if(headPitchOffset > PitchOffsetThr_Up && headPitchAngle > _headPitchAngle_prev)
        {
            _headPitchControlState = HeadPitchControlState.PitchUp;
            _redirectedCentroid += directionFromGazeOrigin * MathFunctions.Deg2Meter(Time.deltaTime, Vector3.Distance(_redirectedCentroid, gazeOrigin)) * 50f;
        }
        if(headPitchOffset < -PitchOffsetThr_Down && headPitchAngle < _headPitchAngle_prev)
        {
            _headPitchControlState = HeadPitchControlState.PitchDown;
            _redirectedCentroid -= directionFromGazeOrigin * MathFunctions.Deg2Meter(Time.deltaTime, Vector3.Distance(_redirectedCentroid, gazeOrigin)) * 50f;
        }

        // // Hand mode
        if(_currentMode == DR_States.Hand)
        {
            _redirectedCentroid += (torsoPosition - _torsoPosition_prev) * _torsoAmpGain;
        }

        // Calculate Virtual Hand Rotation
        Quaternion redirectionRotationOffset = Quaternion.LookRotation(directionFromGazeOrigin) * Quaternion.Inverse(Quaternion.LookRotation(MathFunctions.ProjectOntoXZPlane(torso_forward)));
        Quaternion rightVirtualHandRotation = redirectionRotationOffset * HandData.RightHandRotation;
        Quaternion leftVirtualHandRotation = redirectionRotationOffset * HandData.LeftHandRotation;

        // Calculate Virtual Hand Position
        _leftVirtualHandPosition = _redirectedCentroid + redirectionRotationOffset * (leftHandPosition - torsoPosition)  - redirectionRotationOffset *  _handToTorsoOffset;
        _rightVirtualHandPosition = _redirectedCentroid + redirectionRotationOffset * (rightHandPosition - torsoPosition)  - redirectionRotationOffset *  _handToTorsoOffset;

        // Update Visualizations
        float depth = Vector3.Distance(_redirectedCentroid, HeadData.HeadPosition);
        Vector3 vec_redirected_dir = (_redirectedCentroid - HeadData.HeadPosition).normalized;
        // Project HeadData.HeadForward onto plane with normal perpendicular to dir (use "right" as plane normal)
        Vector3 planeNormal = Vector3.Cross(Vector3.up, vec_redirected_dir).normalized;
        Vector3 vec_headPitch = (HeadData.HeadForward - Vector3.Dot(HeadData.HeadForward, planeNormal) * planeNormal).normalized;
        Vector3 vec_headPitch_OnRedirection = (_headForward_OnRedirection - Vector3.Dot(_headForward_OnRedirection, planeNormal) * planeNormal).normalized;
        Quaternion pitchRotationOffset = Quaternion.LookRotation(vec_redirected_dir) * Quaternion.Inverse(Quaternion.LookRotation(vec_headPitch_OnRedirection));
        Vector3 vec_headPitch_local = (pitchRotationOffset * vec_headPitch).normalized;
        Vector3 vec_headPitch_localIndicator = Vector3.Slerp(vec_redirected_dir, vec_headPitch_local, headPitchOffset > 0 ? IndicatorOffset_Up/PitchOffsetThr_Up: IndicatorOffset_Down/PitchOffsetThr_Down).normalized;

        // RedirectedPivotPoint.transform.position = _redirectedCentroid;
        // HeadPitchPoint_Current_Local.transform.position = HeadData.HeadPosition + vec_headPitch_local * depth;

        // float headPitchOffset = Vector3.Angle(vec_redirected_dir, vec_headPitch_local);
        // ShowText(ref _debugText1, HeadPitchPoint_Current_Local.transform.position, headPitchOffset.ToString("F2") + " deg / " + (HeadData.HeadAngle_WorldY - _headPitchAngle_OnRedirection).ToString("F2") + " deg");

        Vector3 vec_visualizationBar_upper = Vector3.RotateTowards(vec_redirected_dir, Vector3.up, Mathf.Deg2Rad * IndicatorOffset_Up, 0f).normalized;
        Vector3 vec_visualizationBar_lower = Vector3.RotateTowards(vec_redirected_dir, Vector3.down, Mathf.Deg2Rad * IndicatorOffset_Down, 0f).normalized;

        UpArrow.transform.position = HeadData.HeadPosition + vec_visualizationBar_upper * depth;
        DownArrow.transform.position = HeadData.HeadPosition + vec_visualizationBar_lower * depth;
        UpArrow.transform.LookAt(Camera.main.transform);
        DownArrow.transform.LookAt(Camera.main.transform);
        HeadPitch_Current_Indicator.transform.position = HeadData.HeadPosition + vec_headPitch_localIndicator * depth;


        if(headPitchOffset > PitchOffsetThr_Up)
        {
            // UpArrow.transform.position = HeadPitch_Current_Indicator.transform.position;
        }
        else if(headPitchOffset < -PitchOffsetThr_Down)
        {
            // DownArrow.transform.position = HeadPitch_Current_Indicator.transform.position;
        }

        if(_headPitchControlState == HeadPitchControlState.PitchUp)
        {
            UpArrow.SetColor_All(ArrowActivationMaterial);
        }
        else if(_headPitchControlState == HeadPitchControlState.PitchDown)
        {
            DownArrow.SetColor_All(ArrowActivationMaterial);
        }
        else
        {
            UpArrow.SetColor_All(ArrowDeactivationMaterial);
            DownArrow.SetColor_All(ArrowDeactivationMaterial);
        }

        // Update previous frame data
        _isGazeFixation_prev = GazeData.IsFixating_DT();
        _torsoPosition_prev = torsoPosition;
        _headPitchAngle_prev = headPitchAngle;

        // Return Virtual Hand Pose
        if(isRightHand)
        {
            return new Pose(_rightVirtualHandPosition, rightVirtualHandRotation);
        }
        else
        {
            return new Pose(_leftVirtualHandPosition, leftVirtualHandRotation);
        }     
    }

    // // Visualization
    void UpdateVisuals(HeadPitchControlState headPitchControlState)
    {


        // HeadPitchThr_Upper.transform.position = HeadData.HeadPosition + vec_visualizationBar_upper * depth;
        // HeadPitchThr_Lower.transform.position = HeadData.HeadPosition + vec_visualizationBar_lower * depth;

        // if (_rightVirtualHandLine1 == null) _rightVirtualHandLine1 = new Linescript(0.01f, transform);
        // _rightVirtualHandLine1.SetPosition(HeadData.HeadPosition, _redirectedCentroid);

        // if (_leftVirtualHandLine1 == null) _leftVirtualHandLine1 = new Linescript(0.01f, transform);
        // _leftVirtualHandLine1.SetPosition(HeadData.HeadPosition, HeadNeutralPoint.transform.position);

        // if (_rightVirtualHandLine2 == null) _rightVirtualHandLine2 = new Linescript(0.01f, transform, Color.blue);
        // _rightVirtualHandLine2.SetPosition(HeadData.HeadPosition, HeadData.HeadPosition + HeadData.HeadForward * depth);

        // if (_leftVirtualHandLine2 == null) _leftVirtualHandLine2 = new Linescript(0.01f, transform, Color.blue);
        // _leftVirtualHandLine2.SetPosition(_leftVirtualHandPosition, _redirectedCentroid);



        // if(TestMode)
        // {
            // RealHand_Left_Visual1.SetActive(true);
            // RealHand_Left_Visual2.SetActive(true);
            // RealHand_Right_Visual1.SetActive(true);
            // RealHand_Right_Visual2.SetActive(true);

            // // Visulaizations


            // LocalPivotPoint.transform.position = _pivot_redirected;
            // // MidPoint_OnRedirection.transform.position = _handMidpointPosition_OnRedirection;
            // // MidPoint_RealTime.transform.position = HandData.HandMidPosition;

            // if (_leftRealHandLine == null) _leftRealHandLine = new Linescript(0.01f, transform);
            // _leftRealHandLine.SetPosition(leftElbowPosition, leftHandPosition);

            // if (_leftElbowLine == null) _leftElbowLine = new Linescript(0.01f, transform);
            // _leftElbowLine.SetPosition(leftElbowPosition, torsoPosition);

            // if (_rightElbowLine == null) _rightElbowLine = new Linescript(0.01f, transform);
            // _rightElbowLine.SetPosition(rightElbowPosition, torsoPosition);

            // if (_torsoForwardLine == null) _torsoForwardLine = new Linescript(0.01f, transform);
            // _torsoForwardLine.SetPosition(torsoPosition, torsoPosition + torso_forward); 



            // if (_rightVirtualHandLine == null) _rightVirtualHandLine = new Linescript(0.01f, transform);
            // _rightVirtualHandLine.SetPosition(rightVirtualElbowPosition, rightVirtualHandPosition);

            // if (_leftVirtualHandLine == null) _leftVirtualHandLine = new Linescript(0.01f, transform);
            // _leftVirtualHandLine.SetPosition(leftVirtualElbowPosition, leftVirtualHandPosition);

            // if (_rightVirtualElbowLine == null) _rightVirtualElbowLine = new Linescript(0.01f, transform);
            // _rightVirtualElbowLine.SetPosition(rightVirtualElbowPosition, reDirectedRefPosition);

            // if (_leftVirtualElbowLine == null) _leftVirtualElbowLine = new Linescript(0.01f, transform);
            // _leftVirtualElbowLine.SetPosition(leftVirtualElbowPosition, reDirectedRefPosition);

            // // if (_virtualTorsoForwardLine == null) _virtualTorsoForwardLine = new Linescript(0.01f, transform);
            // // _virtualTorsoForwardLine.SetPosition(reDirectedRefPosition, reDirectedRefPosition + torso_forward);

            // // if (_twoVirutalHandConncectionLine == null) _twoVirutalHandConncectionLine = new Linescript(0.01f, transform);
            // // _twoVirutalHandConncectionLine.SetPosition(rightVirtualHandPosition, leftVirtualHandPosition); 

            // // if (_torsoLine == null) _torsoLine = new Linescript(0.01f, transform, Color.red);
            // // _torsoLine.SetPosition(torsoPosition, torsoPosition + torso_forward*radialDistance); 

            // // if (_torsoLeftHandLine == null) _torsoLeftHandLine = new Linescript(0.01f, transform, Color.red);
            // // _torsoLeftHandLine.SetPosition(torsoPosition, HandData.LeftHandPosition);

            // // if (_torsoRightHandLine == null) _torsoRightHandLine = new Linescript(0.01f, transform, Color.red);
            // // _torsoRightHandLine.SetPosition(torsoPosition, HandData.RightHandPosition); 

            // MidPoint_RealTime.transform.position = HandData.HandMidPosition;
            // MidPoint_RealTime.transform.localScale = Vector3.one * Vector3.Distance(HandData.RightHandPosition, HandData.LeftHandPosition) * 0.5f;

        // }
    }

    public override bool IsGazeRedirecting()
    {
        return _currentMode == DR_States.Gaze;
    }

    private TextMesh _distanceText;
    private TextMesh _pinchDistance;
    private TextMesh _debugText1;
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
