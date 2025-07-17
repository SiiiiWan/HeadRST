using UnityEngine;

public interface IManipulationBehavior
{
    void OnSingleHandGrabbed(Transform target);
    void ApplyHandReleasedBehaviour();
    void ApplySingleHandGrabbedBehaviour();
    void ApplyBothHandGrabbedBehaviour();
}

public class ManipulationTechnique : MonoBehaviour, IManipulationBehavior
{
    public Transform GrabbedObject { get; private set; }
    public virtual void OnSingleHandGrabbed(Transform obj) { GrabbedObject = obj; }
    public virtual void ApplySingleHandGrabbedBehaviour()
    {
        Debug.LogWarning("Apply method not implemented in " + this.GetType().Name);
    }

    public virtual void ApplyHandReleasedBehaviour() { GrabbedObject = null; }
    public virtual void ApplyBothHandGrabbedBehaviour() { }



    [Header("Fixation Settings")] // not allowed adjustment in runtime
    public float GazeFixationDuration = 0.25f; // in seconds
    public float GazeFixationAngle = 3f; // in degrees

    public float HeadFixationDuration = 0.25f; // in seconds
    public float HeadFixationAngle = 1.5f; // in degrees


    // Hand
    public HandData HandData { get; private set; }
    public Vector3 HandPosition { get; private set; }
    public Vector3 HandPosition_delta { get; private set; }
    public Quaternion HandRotation_delta { get; private set; }

    // Gaze
    public EyeGaze GazeData { get; private set; }
    public Vector3 GazeOrigin { get; private set; }
    public Vector3 GazeDirection { get; private set; }
    public FixationTracker GazeFixationTracker { get; private set; }
    public bool IsGazeFixating { get; private set; }
    public bool IsGazeSaccading { get; private set; }


    // Head
    public HeadMovement HeadData { get; private set; }
    public Vector3 HeadForward { get; private set; }
    public Vector3 HeadRight { get; private set; }
    public Vector3 HeadPosition { get; private set; }
    public FixationTracker HeadFixationTracker { get; private set; }
    public bool IsHeadFixating { get; private set; }


    public virtual void Awake()
    {
        GazeFixationTracker = new FixationTracker(GazeFixationDuration, GazeFixationAngle);
        HeadFixationTracker = new FixationTracker(HeadFixationDuration, HeadFixationAngle);
    }

    public virtual void Update()
    {
        HandData = HandData.GetInstance();
        HandPosition_delta = HandData.GetDeltaHandPosition(usePinchTip: true);
        HandRotation_delta = HandData.GetDeltaHandRotation(usePinchTip: true);
        HandPosition = HandData.GetHandPosition(usePinchTip: true);


        GazeData = EyeGaze.GetInstance();
        GazeOrigin = GazeData.GetGazeRay().origin;
        GazeDirection = GazeData.GetGazeRay().direction.normalized;
        IsGazeSaccading = GazeData.IsSaccading();
        IsGazeFixating = GazeFixationTracker.GetIsFixating(GazeDirection);

        HeadData = HeadMovement.GetInstance();
        HeadForward = Camera.main.transform.forward;
        HeadRight = Camera.main.transform.right;
        HeadPosition = Camera.main.transform.position;
        IsHeadFixating = HeadFixationTracker.GetIsFixating(HeadForward);
    }

    float GetVisualGain()
    {
        return Vector3.Distance(GrabbedObject.position, GazeOrigin) / Vector3.Distance(HandPosition, GazeOrigin);
    }
    
    float VitLerp(float x, float k1 = 0.8f / 3f, float k2 = 0.8f, float v1 = 0.2f, float v2 = 0.6f)
    {
        if (x <= v1)
            return k1;

        if (x >= v2)
            return k2;

        return k1 + (k2 - k1) / (v2 - v1) * (x - v1);
    }
}