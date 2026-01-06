using UnityEngine;

public class DR_v2 : VirtualHandProvider
{
    private bool _isHandState;
    private AH_ControlMode _currentMode = AH_ControlMode.Gaze;
    private float _headPitch_OnFixation;
    [Range(0f, 20f)] public float DepthTransitionSpeed = 10f;

    [Header("Hand Movement Detection")]
    [Tooltip("The speed threshold (m/s) to be considered moving.")]
    public float MovementThreshold = 0.01f;
    [Tooltip("Time in seconds with no movement to be considered stopped.")]
    public float TimeToStop = 0.2f;

    public bool IsHandMoving { get; private set; }

    private Vector3 _lastHandPosition;
    private float _timeStationary;

    public override Vector3 UpdatePivot(Vector3 currentPosition)
    {
        UpdateDataSource();

        Vector3 gazeOrigin = GazeData.GetGazeOrigin();
        Vector3 gazeDirection = GazeData.GetGazeDirection();
        float headPitchAngle = HeadData.HeadAngle_WorldY;

        if(_currentMode == AH_ControlMode.Gaze)
        {
            currentPosition = gazeOrigin + gazeDirection * Vector3.Distance(currentPosition, gazeOrigin);    
            
            // if(GazeData.GetGazeHitPoint(out Vector3 gazeHitPoint))
            // {
            //     Vector3 HandToPinchOffset = HandData.GetHandPosition(usePinchTip: true) - HandData.GetHandPosition(usePinchTip: false);
            //     currentPosition = gazeHitPoint;
            // }
            // else
            // {
            //     currentPosition = gazeOrigin + gazeDirection * Vector3.Distance(currentPosition, gazeOrigin);    
            // }
        }
        else
        {
                Vector3 directionFromGazeOrigin = (currentPosition - gazeOrigin).normalized; 
                if(headPitchAngle - _headPitch_OnFixation >= 5f)
                {
                    currentPosition += directionFromGazeOrigin * (headPitchAngle - _headPitch_OnFixation - 5f) * MathFunctions.Deg2Meter(Time.deltaTime, Vector3.Distance(currentPosition, gazeOrigin)) * DepthTransitionSpeed;    
                    currentPosition = gazeOrigin + directionFromGazeOrigin * Mathf.Clamp(Vector3.Distance(currentPosition, gazeOrigin), 1, 5);
                }
                else if(headPitchAngle - _headPitch_OnFixation <= -3f)
                {
                    currentPosition -= directionFromGazeOrigin * (-headPitchAngle + _headPitch_OnFixation - 3f) * MathFunctions.Deg2Meter(Time.deltaTime, Vector3.Distance(currentPosition, gazeOrigin)) * DepthTransitionSpeed;                   
                    currentPosition = gazeOrigin + directionFromGazeOrigin * Mathf.Clamp(Vector3.Distance(currentPosition, gazeOrigin), 1, 5);
                }
                else
                {
                    currentPosition += HandData.GetDeltaHandPosition(usePinchTip: false) * Mathf.Max(1f, Vector3.Distance(currentPosition, GazeData.GetGazeOrigin()) / Vector3.Distance(HandData.GetHandPosition(usePinchTip: false), GazeData.GetGazeOrigin()));
                    _isHandState = true;
                    // if(IsHandMoving == false) _headPitch_OnFixation = headPitchAngle;
                }
        }
        

        if (_currentMode == AH_ControlMode.Gaze)
        {

            if (GazeData.IsFixating_DT())
            {
                _currentMode = AH_ControlMode.Head;
                _headPitch_OnFixation = headPitchAngle;
            }
        }
        else
        {
            if (GazeData.IsFixating_DT() == false)
            {
                _currentMode = AH_ControlMode.Gaze;
            }
        }            
        

        return currentPosition;
    }

    private void DetectHandMovement()
    {
        // It's more robust to check velocity than just delta position
        // as delta is frame-rate dependent.
        Vector3 currentHandPosition = HandData.GetHandPosition(usePinchTip: false);
        float speed = (currentHandPosition - _lastHandPosition).magnitude / Time.deltaTime;
        _lastHandPosition = currentHandPosition;

        if (speed > MovementThreshold)
        {
            IsHandMoving = true;
            _timeStationary = 0f;
        }
        else
        {
            _timeStationary += Time.deltaTime;
            if (_timeStationary >= TimeToStop)
            {
                IsHandMoving = false;
            }
        }
    }

}
