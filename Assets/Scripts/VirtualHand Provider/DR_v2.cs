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

    private Vector3 _handPosition_OnFixation;
    private Vector3 _virtualHandPosition_OnFixation;
    private bool _isGazeFixation_prev;

    public override Vector3 UpdatePivot(Vector3 currentPosition)
    {
        UpdateDataSource();
        Vector3 gazeOrigin = GazeData.GetGazeOrigin();
        Vector3 gazeDirection = GazeData.GetGazeDirection();

        if(GazeData.IsFixating_DT() && _currentMode == DR_States.Gaze)
        {
            _currentMode = DR_States.Hand;
            _handPosition_OnFixation = HandData.RightHandPosition;
            _virtualHandPosition_OnFixation = currentPosition;
        }

        if(Vector3.Angle(gazeDirection, (currentPosition - gazeOrigin).normalized) > 15f && GazeData.IsFixating_DT() && _isGazeFixation_prev == false)
        {
            _currentMode = DR_States.Gaze;
        }


        currentPosition = _currentMode switch
        {
            DR_States.Gaze => gazeOrigin + gazeDirection * Vector3.Distance(GazeData.GetGazeHitPoint(out Vector3 hitPoint) ? hitPoint : currentPosition, gazeOrigin),
            DR_States.Hand => currentPosition,
            _ => currentPosition
        };

        _isGazeFixation_prev = GazeData.IsFixating_DT();

        return currentPosition;
    }

    public override Vector3 GetVirtualHandPosition(bool isRightHand)
    {
        _currentPivotPoint = UpdatePivot(_currentPivotPoint);
        
        Vector3 midpoint = (HandData.RightHandPosition + HandData.LeftHandPosition) / 2f;
        Vector3 rightHandOffset = HandData.RightHandPosition - midpoint;
        Vector3 leftHandOffset = HandData.LeftHandPosition - midpoint;

        // Vector3 pinchMidpoint = (HandData.RightPinchTipPosition + HandData.LeftPinchTipPosition) / 2f;
        // float betweenPinchVector_projectToView_mag = MathFunctions.ProjectVectorOntoPlane(HandData.RightPinchTipPosition - HandData.LeftPinchTipPosition, HeadData.HeadForward).magnitude;
        
        // float requiredVirtualPinchDistance = Vector3.Distance(_currentPivotPoint, GazeData.GetGazeOrigin()) / Vector3.Distance(midpoint, GazeData.GetGazeOrigin()) * betweenPinchVector_projectToView_mag;
        // float actualPinchDistance = Vector3.Distance(HandData.RightPinchTipPosition, HandData.LeftPinchTipPosition);
        // float actualHandDistance = Vector3.Distance(HandData.RightHandPosition, HandData.LeftHandPosition);
        // float requiredVirtualHandDistance = requiredVirtualPinchDistance + (actualHandDistance - actualPinchDistance);



        // float gain = requiredVirtualHandDistance / actualHandDistance;

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

    Vector3 GetVirtualHandPosition_Absolute()
    {
        // local absolute mapping, as hand tracking crapy and drifts with relative mapping
        Vector3 realHandOffset = HandData.RightHandPosition - _handPosition_OnFixation;
        return _virtualHandPosition_OnFixation + realHandOffset * Mathf.Max(1f, Vector3.Distance(_virtualHandPosition_OnFixation, GazeData.GetGazeOrigin()) / Vector3.Distance(_handPosition_OnFixation, GazeData.GetGazeOrigin()));
    }


    public override bool IsGazeRedirecting()
    {
        return _currentMode == DR_States.Gaze;
    }

}
