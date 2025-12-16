using UnityEngine;

public class TestTechnique : PositionRotationProvider
{

    private AH_ControlMode _currentMode = AH_ControlMode.Gaze;
    private float _headPitch_OnFixation;

    public override Vector3 GetPositionOutput(Vector3 currentPosition)
    {
        UpdateDataSource();

        Vector3 gazeOrigin = GazeData.GetGazeOrigin();
        Vector3 gazeDirection = GazeData.GetGazeDirection();

        currentPosition += HandData.GetDeltaHandPosition() * GetVisualGain(currentPosition);
        Vector3 directionFromGazeOrigin = (currentPosition - gazeOrigin).normalized;

        currentPosition += directionFromGazeOrigin * HeadData.DeltaHeadY * 0.4f;

        if (_currentMode == AH_ControlMode.Gaze)
        {
            currentPosition = gazeOrigin + gazeDirection * Mathf.Clamp(Vector3.Distance(currentPosition, gazeOrigin), 1, 100f);

            if (GazeData.IsFixating_DT())
            {
                _currentMode = AH_ControlMode.Head;
            }
        }
        else
        {
            currentPosition = gazeOrigin + directionFromGazeOrigin * Mathf.Clamp(Vector3.Distance(currentPosition, gazeOrigin), 1, 100f);

            if (GazeData.IsFixating_DT() == false)
            {
                _currentMode = AH_ControlMode.Gaze;
            }
        }

        return currentPosition;
    }

}
