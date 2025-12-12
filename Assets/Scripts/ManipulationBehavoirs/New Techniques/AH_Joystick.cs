using UnityEngine;

public class AH_Joystick : PositionRotationProvider
{

    private AH_ControlMode _currentMode = AH_ControlMode.Gaze;
    private float _headPitch_OnFixation;

    public override Vector3 GetPositionOutput(Vector3 currentPosition)
    {
        UpdateDataSource();

        Vector3 gazeOrigin = GazeData.GetGazeOrigin();
        Vector3 gazeDirection = GazeData.GetGazeDirection();
        float headPitchAngle = HeadData.HeadAngle_WorldY;

        currentPosition += HandData.GetDeltaHandPosition() * GetVisualGain(currentPosition);
        Vector3 directionFromGazeOrigin = (currentPosition - gazeOrigin).normalized;

        if (_currentMode == AH_ControlMode.Gaze)
        {
            currentPosition = gazeOrigin + gazeDirection * Vector3.Distance(gazeOrigin, currentPosition + gazeDirection * HeadData.DeltaHeadY * 0.4f);
            currentPosition = gazeOrigin + gazeDirection * Mathf.Clamp(Vector3.Distance(currentPosition, gazeOrigin), 1, 100f);



            if (GazeData.IsFixating_DT())
            {
                _currentMode = AH_ControlMode.Head;
                _headPitch_OnFixation = headPitchAngle;
            }
        }
        else
        {
            if(headPitchAngle - _headPitch_OnFixation >= 5f)
            {
                currentPosition += directionFromGazeOrigin * (headPitchAngle - _headPitch_OnFixation - 5f) * MathFunctions.Deg2Meter(Time.deltaTime, Vector3.Distance(currentPosition, gazeOrigin)) * 10;                    
            }

            if(headPitchAngle - _headPitch_OnFixation <= -3f)
            {
                currentPosition -= directionFromGazeOrigin * (-headPitchAngle + _headPitch_OnFixation - 3f) * MathFunctions.Deg2Meter(Time.deltaTime, Vector3.Distance(currentPosition, gazeOrigin)) * 10;                   
            }

            currentPosition = gazeOrigin + directionFromGazeOrigin * Mathf.Clamp(Vector3.Distance(currentPosition, gazeOrigin), 1, 100f);


            if (GazeData.IsFixating_DT() == false)
            {
                _currentMode = AH_ControlMode.Gaze;
            }
        }

        return currentPosition;
    }

    public override Quaternion GetRotationOutput(Quaternion currentRotation)
    {
        UpdateDataSource();

        return HandData.GetDeltaHandRotation() * currentRotation;
    }
}
