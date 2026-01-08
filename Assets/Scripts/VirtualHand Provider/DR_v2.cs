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

        ;

        currentPosition = _currentMode switch
        {
            DR_States.Gaze => gazeOrigin + gazeDirection * Vector3.Distance(GazeData.GetGazeHitPoint(out Vector3 hitPoint) ? hitPoint : currentPosition, gazeOrigin),
            DR_States.Hand => GetVirtualHandPosition_Absolute(),
            _ => currentPosition
        };

        _isGazeFixation_prev = GazeData.IsFixating_DT();

        if(Input.GetKey(KeyCode.Space))
        {
            Debug.Log("IsGazeFixating: " + GazeData.IsFixating_DT());
        }

        return currentPosition;
    }

    Vector3 GetVirtualHandPosition_Absolute()
    {
        // local absolute mapping, as hand tracking crapy and drifts with relative mapping
        Vector3 realHandOffset = HandData.RightHandPosition - _handPosition_OnFixation;
        return _virtualHandPosition_OnFixation + realHandOffset * Mathf.Max(1f, Vector3.Distance(_virtualHandPosition_OnFixation, GazeData.GetGazeOrigin()) / Vector3.Distance(_handPosition_OnFixation, GazeData.GetGazeOrigin()));
    }



}
