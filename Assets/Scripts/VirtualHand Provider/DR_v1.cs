using UnityEngine;

public class DR_v1 : VirtualHandProvider
{
    private AH_ControlMode _currentMode = AH_ControlMode.Gaze;
    private float _headPitch_OnFixation;
    [Range(0f, 20f)] public float DepthTransitionSpeed = 10f;

    private Vector3 _lastHandPosition;
    private float _timeStationary;

    public override Vector3 UpdatePivot(Vector3 currentPosition)
    {
        UpdateDataSource();

        Vector3 gazeOrigin = GazeData.GetGazeOrigin();
        Vector3 gazeDirection = GazeData.GetGazeDirection();
        float headPitchAngle = HeadData.HeadAngle_WorldY;

        currentPosition += HandData.GetDeltaHandPosition(usePinchTip: false) * GetVisualGain(currentPosition);
        
        if (_currentMode == AH_ControlMode.Gaze)
        {
            // currentPosition = gazeOrigin + gazeDirection * Vector3.Distance(gazeOrigin, currentPosition + gazeDirection * HeadData.DeltaHeadY * 0.4f);
            // currentPosition = gazeOrigin + gazeDirection * Mathf.Clamp(Vector3.Distance(currentPosition, gazeOrigin), 1, 100f);

            currentPosition = gazeOrigin + gazeDirection * Vector3.Distance(currentPosition, gazeOrigin);

            if (GazeData.IsFixating_DT())
            {
                _currentMode = AH_ControlMode.Head;
                _headPitch_OnFixation = headPitchAngle;
            }
        }
        else
        {

            Vector3 directionFromGazeOrigin = (currentPosition - gazeOrigin).normalized; 
            if(headPitchAngle - _headPitch_OnFixation >= 5f)
            {
                currentPosition += directionFromGazeOrigin * (headPitchAngle - _headPitch_OnFixation - 5f) * MathFunctions.Deg2Meter(Time.deltaTime, Vector3.Distance(currentPosition, gazeOrigin)) * DepthTransitionSpeed;                    
            }

            if(headPitchAngle - _headPitch_OnFixation <= -3f)
            {
                currentPosition -= directionFromGazeOrigin * (-headPitchAngle + _headPitch_OnFixation - 3f) * MathFunctions.Deg2Meter(Time.deltaTime, Vector3.Distance(currentPosition, gazeOrigin)) * DepthTransitionSpeed;                   
            }

            currentPosition = gazeOrigin + directionFromGazeOrigin * Mathf.Clamp(Vector3.Distance(currentPosition, gazeOrigin), 1, 5);

            if (GazeData.IsFixating_DT() == false)
            {
                _currentMode = AH_ControlMode.Gaze;
            }
        }            
        

        return currentPosition;
    }

}
