using UnityEngine;

public enum StaticState
{
    Gaze,
    Head
}

public class Magic : GazePinch
{
    public override string TechniqueName => "MAGIC";

    [Header("MAGIC Parameters")]
    [SerializeField] protected float theta_thr = 15f;

    public StaticState CurrentState { get; protected set; } = StaticState.Gaze;
    public float CurrentDistanceToGaze { get; protected set; }
    public float AngleGazeDirectionToObject { get; protected set; }

    public override void ApplyGrabbedBehaviour()
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
