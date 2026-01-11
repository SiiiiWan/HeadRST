using Unity.Android.Gradle.Manifest;
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

    private Vector3 _handMidpoint_OnRedirection;
    private bool _isGazeFixation_prev;
    private float _headPitch_neutral;

    public override Vector3 UpdatePivot(Vector3 currentPosition)
    {
        UpdateDataSource();
        Vector3 gazeOrigin = GazeData.GetGazeOrigin();
        Vector3 gazeDirection = GazeData.GetGazeDirection();
        float headPitchAngle = HeadData.HeadAngle_WorldY;

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
            if(headPitchAngle - _headPitch_neutral >= 5f || headPitchAngle - _headPitch_neutral <= -3f)
            {
                _currentMode = DR_States.Head;
            }
            else
            {
                _currentMode = DR_States.Hand;
            }
        }

        // TODO: change 15 deg to hand distance

        if(_currentMode != DR_States.Hand) _handMidpoint_OnRedirection = (HandData.RightHandPosition + HandData.LeftHandPosition) / 2f;
        if(_currentMode == DR_States.Gaze) _headPitch_neutral = HeadData.HeadAngle_WorldY;

        ShowCurrentMode();

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

    public override Vector3 GetVirtualHandPosition(bool isRightHand)
    {
        _currentPivotPoint = UpdatePivot(_currentPivotPoint);
        
        Vector3 midpoint = (HandData.RightHandPosition + HandData.LeftHandPosition) / 2f;
        Vector3 gazeOrigin = GazeData.GetGazeOrigin();
        Vector3 rightHandOffset = HandData.RightHandPosition - _handMidpoint_OnRedirection;
        Vector3 leftHandOffset = HandData.LeftHandPosition - _handMidpoint_OnRedirection;

        // Vector3 pinchMidpoint = (HandData.RightPinchTipPosition + HandData.LeftPinchTipPosition) / 2f;
        // float betweenPinchVector_projectToView_mag = MathFunctions.ProjectVectorOntoPlane(HandData.RightPinchTipPosition - HandData.LeftPinchTipPosition, HeadData.HeadForward).magnitude;
        
        // float requiredVirtualPinchDistance = Vector3.Distance(_currentPivotPoint, GazeData.GetGazeOrigin()) / Vector3.Distance(midpoint, GazeData.GetGazeOrigin()) * betweenPinchVector_projectToView_mag;
        // float actualPinchDistance = Vector3.Distance(HandData.RightPinchTipPosition, HandData.LeftPinchTipPosition);
        // float actualHandDistance = Vector3.Distance(HandData.RightHandPosition, HandData.LeftHandPosition);
        // float requiredVirtualHandDistance = requiredVirtualPinchDistance + (actualHandDistance - actualPinchDistance);
        // float gain = requiredVirtualHandDistance / actualHandDistance;

        // float gain = Vector3.Distance(_currentPivotPoint, GazeData.GetGazeOrigin()) / Vector3.Distance(_handMidpoint_OnRedirection, GazeData.GetGazeOrigin());

        // if (float.IsNaN(gain))
        // {
        //     gain = 1f;
        // }

        float gain = 1f;

        if(isRightHand)
        {
            return _currentPivotPoint + rightHandOffset * gain;
        }
        else
        {
            return _currentPivotPoint + leftHandOffset * gain;
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
        _distanceText.text = $"{_currentMode}m"; // Format to 2 decimal places

        // Orient the text towards the camera
        if (Camera.main != null)
        {
            _distanceText.transform.LookAt(Camera.main.transform);
            _distanceText.transform.Rotate(0, 180, 0);
        }
    }

}
