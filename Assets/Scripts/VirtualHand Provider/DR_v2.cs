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

    enum InteractionArea
    {
        Close,
        Distance
    }

    private DR_States _currentMode = DR_States.Gaze;
    private InteractionArea _currentArea = InteractionArea.Distance;
    private bool _isGazeFixation_prev;
    private float _headPitch_neutral;

    Vector3 _redirectedCentroid;
    Vector3 _handToTorsoOffset;

    Vector3 _leftVirtualHandPosition;
    Vector3 _rightVirtualHandPosition;

    public override Pose GetVirtualHandPose(bool isRightHand)
    {
        // Get Tracking Data
        UpdateDataSource();
        
        //// Gaze
        Vector3 gazeOrigin = GazeData.GetGazeOrigin();
        Vector3 gazeDirection = GazeData.GetGazeDirection();
        
        //// Hand
        Vector3 rightHandPosition = HandData.RightHandPosition;
        Vector3 leftHandPosition = HandData.LeftHandPosition;

        //// Head
        float headPitchAngle = HeadData.HeadAngle_WorldY;

        //// Torso
        Posef torsoPose = BodyData.BodyState?.JointLocations[5].Pose ?? default;
        Vector3 torsoPosition = new Vector3(torsoPose.Position.x, torsoPose.Position.y, -torsoPose.Position.z);
        Quaternion torso_rotation =  new Quaternion(torsoPose.Orientation.x, torsoPose.Orientation.y, torsoPose.Orientation.z, torsoPose.Orientation.w);
        torso_rotation = Quaternion.Euler(
            torso_rotation.eulerAngles.x,
            -torso_rotation.eulerAngles.y + 90,
            torso_rotation.eulerAngles.z);
        Vector3 torso_forward = (torso_rotation * Vector3.forward).normalized;

        // Mode Switching Logic
        if(GazeData.IsFixating_DT() && _currentMode == DR_States.Gaze)
        {
            _currentMode = DR_States.Hand;

            if(Vector3.Angle(gazeDirection, (HandData.HandMidPosition - gazeOrigin).normalized) <= MathFunctions.Meter2Deg(Vector3.Distance(HandData.RightHandPosition, HandData.LeftHandPosition), Vector3.Distance(HandData.HandMidPosition, gazeOrigin)))
            {
                _currentArea = InteractionArea.Close;
            }
        }

        Vector3 virtualHandMidpoint = (_leftVirtualHandPosition + _rightVirtualHandPosition) / 2f;
        float distanceBewteenVirualHands = Vector3.Distance(_leftVirtualHandPosition, _rightVirtualHandPosition);
        float distanceToVirtualHandMidpoint = Vector3.Distance(gazeOrigin, virtualHandMidpoint);
        if(Vector3.Angle(gazeDirection, (virtualHandMidpoint - gazeOrigin).normalized) > MathFunctions.Deg2Meter(distanceBewteenVirualHands, distanceToVirtualHandMidpoint) && GazeData.IsFixating_DT() && _isGazeFixation_prev == false)
        {
            _currentMode = DR_States.Gaze;

            if(Vector3.Angle(gazeDirection, (HandData.HandMidPosition - gazeOrigin).normalized) > MathFunctions.Meter2Deg(Vector3.Distance(HandData.RightHandPosition, HandData.LeftHandPosition), Vector3.Distance(HandData.HandMidPosition, gazeOrigin)))
            {
                _currentArea = InteractionArea.Distance;
            }
        }

        // Set neutral head pitch angle
        if(_currentMode == DR_States.Gaze || GazeData.IsFixating_DT() == false)
        {
            _headPitch_neutral = HeadData.HeadAngle_WorldY;
        } 

        // mode effects
        if(_currentArea == InteractionArea.Close)
        {
            // Update previous frame data
            _isGazeFixation_prev = GazeData.IsFixating_DT();
            
            if(isRightHand)
            {
                return new Pose(rightHandPosition, HandData.RightHandRotation);
            }
            else
            {
                return new Pose(leftHandPosition, HandData.LeftHandRotation);
            }
        }

        switch (_currentMode)
        {
            case DR_States.Gaze:                
                // TODO: change 0.2f to cone-cast
                _redirectedCentroid = gazeOrigin + gazeDirection * Vector3.Distance(GazeData.GetGazeHitPoint_Sphere(out Vector3 hitPoint, 0.2f) ? hitPoint : _redirectedCentroid, gazeOrigin);
                break;
            case DR_States.Head:
                {
                    break;
                }
            case DR_States.Hand:
                {
                    break;
                }
            default:
                break;
        }

        // Calculate the depth axis
        Vector3 directionFromGazeOrigin = (_redirectedCentroid - gazeOrigin).normalized;

        // Keyboard depth control to replace head depth atm
        if(Input.GetKey(KeyCode.DownArrow))
        {
            _redirectedCentroid -= directionFromGazeOrigin * MathFunctions.Deg2Meter(Time.deltaTime, Vector3.Distance(_redirectedCentroid, gazeOrigin)) * 25f;
        }
        if(Input.GetKey(KeyCode.UpArrow))
        {
            _redirectedCentroid += directionFromGazeOrigin * MathFunctions.Deg2Meter(Time.deltaTime, Vector3.Distance(_redirectedCentroid, gazeOrigin)) * 25f;
        }

        // Calculate Virtual Hand Rotation
        Quaternion redirectionRotationOffset = Quaternion.LookRotation(directionFromGazeOrigin) * Quaternion.Inverse(Quaternion.LookRotation(MathFunctions.ProjectOntoXZPlane(torso_forward)));
        Quaternion rightVirtualHandRotation = redirectionRotationOffset * HandData.RightHandRotation;
        Quaternion leftVirtualHandRotation = redirectionRotationOffset * HandData.LeftHandRotation;

        // Calculate Virtual Hand Position
        if(_currentMode == DR_States.Gaze)
        {
            _handToTorsoOffset = MathFunctions.ProjectOntoXZPlane(HandData.HandMidPosition - torsoPosition);
        }
        _leftVirtualHandPosition = _redirectedCentroid + redirectionRotationOffset * (leftHandPosition - torsoPosition + _handToTorsoOffset);
        _rightVirtualHandPosition = _redirectedCentroid + redirectionRotationOffset * (rightHandPosition - torsoPosition + _handToTorsoOffset);


        // // Visualization
        // if(TestMode)
        // {
            // RealHand_Left_Visual1.SetActive(true);
            // RealHand_Left_Visual2.SetActive(true);
            // RealHand_Right_Visual1.SetActive(true);
            // RealHand_Right_Visual2.SetActive(true);

            // // Visulaizations
            // RedirectedPivotPoint.transform.position = reDirectedRefPosition;
            // LocalPivotPoint.transform.position = _pivot_redirected;
            // // MidPoint_OnRedirection.transform.position = _handMidpointPosition_OnRedirection;
            // // MidPoint_RealTime.transform.position = HandData.HandMidPosition;

            // if (_rightRealHandLine == null) _rightRealHandLine = new Linescript(0.01f, transform);
            // _rightRealHandLine.SetPosition(rightElbowPosition, rightHandPosition);

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
            // ShowText(ref _handDistance, HandData.HandMidPosition, Vector3.Distance(HandData.RightHandPosition, HandData.LeftHandPosition).ToString("F2") + " m");
        // }

        // Update previous frame data
        _isGazeFixation_prev = GazeData.IsFixating_DT();

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
