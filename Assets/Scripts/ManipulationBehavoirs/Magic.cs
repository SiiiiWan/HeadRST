using UnityEngine;

public class Magic : GazePinch
{
    public override string TechniqueName => "MAGIC";

    public override void ApplyIndirectGrabbedBehaviour()
    {
        ApplyGazePinchTransform();

        CurrentDistanceToGaze = Vector3.Distance(GazeOrigin, GrabbedObject.transform.position);

        if (CurrentState == StaticState.Gaze)
        {
            ApplyGazeProjection();
        }
        else
        {
            ApplyHeadStateBehaviour();
        }

        VirtualHandPosition = WristPosition;
    }

    protected void ApplyGazeProjection()
    {
        GrabbedObject.transform.position = GazeOrigin + GazeDirection * CurrentDistanceToGaze;

        if (IsGazeFixating) CurrentState = StaticState.Head;
    }

    protected virtual void ApplyHeadStateBehaviour()
    {
        AngleGazeDirectionToObject = Vector3.Angle(GazeDirection, GrabbedObject.transform.position - GazeOrigin);
        if (IsGazeFixating == false && AngleGazeDirectionToObject > theta_thr) CurrentState = StaticState.Gaze;
    }
}
