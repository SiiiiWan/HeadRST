using UnityEngine;

public class AnywhereHandStatic : ManipulationTechnique
{

    public StaticState CurrentState = StaticState.Hand;
    public float HandSpeedThreshold = 0.1f; // in meters per second
    public override Vector3 GetNewVirtualHandPosition()
    {
        Vector3 nextVirtualHandPosition = VirtualHandPosition;

        if (IsGazeFixating == false)
        {
            CurrentState = StaticState.Gaze;
        }
        else
        {
            if (IsHeadFixating == false)
            {
                CurrentState = StaticState.Head;
            }
            else
            {
                CurrentState = StaticState.Hand;
            }
        }

        switch (CurrentState)
        {
            case StaticState.Gaze:
                float distance = Vector3.Distance(GazeOrigin, VirtualHandPosition);
                nextVirtualHandPosition = GazeOrigin + GazeDirection * distance;
                break;
            case StaticState.Head:
                Vector3 virtualHandDirection = (VirtualHandPosition - GazeOrigin).normalized;
                nextVirtualHandPosition = VirtualHandPosition + virtualHandDirection * DeltaHeadY * VitLerp(HeadSpeed, 0.8f / 3f, 0.8f, 0.2f, 0.6f);
                // nextVirtualHandPosition = GazeOrigin + virtualHandDirection * Mathf.Clamp(Vector3.Distance(nextVirtualHandPosition, GazeOrigin), 1, 10);
                break;
            case StaticState.Hand:
                nextVirtualHandPosition = VirtualHandPosition + WristPosition_delta;
                break;
            default:
                nextVirtualHandPosition = VirtualHandPosition + WristPosition_delta;
                break;
        }

        return nextVirtualHandPosition;
    }
}