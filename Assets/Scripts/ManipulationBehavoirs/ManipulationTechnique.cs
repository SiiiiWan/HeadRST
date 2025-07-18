using System;
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
    public virtual void OnSingleHandGrabbed(Transform obj)
    {
        GrabbedObject = obj;

        OnGazeFixation();
    }

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

    public float HandStablizeDuration = 0.25f; // in seconds
    public float HandStablizeThr = 0.1f; // in meters

    // Hand
    public HandData HandData { get; private set; }
    public Vector3 HandPosition { get; private set; }
    public Vector3 HandPosition_delta { get; private set; }
    public Quaternion HandRotation_delta { get; private set; }
    public float HandTranslationSpeed { get; private set; }
    public float HandRotationSpeed { get; private set; }
    public FixationTracker HandFixationTracker { get; private set; }
    public bool IsHandStablized { get; private set; }



    // Gaze
    public EyeGaze GazeData { get; private set; }
    public Vector3 GazeOrigin { get; private set; }
    public Vector3 GazeDirection { get; private set; }
    public FixationTracker GazeFixationTracker { get; private set; }
    public bool IsGazeFixating { get; private set; }
    public bool IsGazeFixating_pre { get; private set; }
    public bool IsGazeSaccading { get; private set; }
    public Vector3 GazeDirection_OnGazeFixation { get; private set; }
    public Vector3 HeadDirection_OnGazeFixation { get; private set; }
    public float EyeInHeadYAngle { get; private set; }
    public float EyeInHeadYAngle_OnGazeFixation { get; private set; }


    // Head
    public HeadMovement HeadData { get; private set; }
    public Vector3 HeadForward { get; private set; }
    public Vector3 HeadRight { get; private set; }
    public Vector3 HeadPosition { get; private set; }
    public FixationTracker HeadFixationTracker { get; private set; }
    public bool IsHeadFixating { get; private set; }
    public bool IsHeadFixating_pre { get; private set; }

    public float HeadSpeed { get; private set; }
    public float HeadYAngle { get; private set; }
    public float DeltaHeadY { get; private set; }
    public float AvailableHeadY_Up { get; private set; }
    public float AvailableHeadY_Down { get; private set; } // negtive value means down
    public float HeadYAngle_OnGazeFixation { get; private set; }

    public virtual void Awake()
    {
        GazeFixationTracker = new FixationTracker(GazeFixationDuration, GazeFixationAngle);
        HeadFixationTracker = new FixationTracker(HeadFixationDuration, HeadFixationAngle);
        HandFixationTracker = new FixationTracker(HandStablizeDuration, HandStablizeThr);
    }

    public virtual void Update()
    {
        HandData = HandData.GetInstance();
        HandPosition_delta = HandData.GetDeltaHandPosition(usePinchTip: true);
        HandRotation_delta = HandData.GetDeltaHandRotation(usePinchTip: true);
        HandPosition = HandData.GetHandPosition(usePinchTip: true);
        HandTranslationSpeed = HandData.GetHandSpeed(usePinchTip: true);
        HandRotationSpeed = HandData.GetHandRotationSpeed(usePinchTip: true);

        HandFixationTracker.UpdateThrshould(HandStablizeDuration, HandStablizeThr);
        IsHandStablized = HandFixationTracker.GetIsFixating(HandPosition);


        GazeData = EyeGaze.GetInstance();
        GazeOrigin = GazeData.GetGazeRay().origin;
        GazeDirection = GazeData.GetGazeRay().direction.normalized;
        IsGazeSaccading = GazeData.IsSaccading();
        GazeFixationTracker.UpdateThrshould(GazeFixationDuration, GazeFixationAngle);
        IsGazeFixating = GazeFixationTracker.GetIsFixating(GazeDirection);
        EyeInHeadYAngle = GazeData.EyeInHeadYAngle;

        HeadData = HeadMovement.GetInstance();
        HeadForward = Camera.main.transform.forward;
        HeadRight = Camera.main.transform.right;
        HeadPosition = Camera.main.transform.position;
        HeadFixationTracker.UpdateThrshould(HeadFixationDuration, HeadFixationAngle);
        IsHeadFixating = HeadFixationTracker.GetIsFixating(HeadForward);
        HeadSpeed = HeadData.HeadSpeed;
        DeltaHeadY = HeadData.DeltaHeadY;
        HeadYAngle = HeadData.HeadAngle_WorldY;


        if (IsGazeFixating_pre == false && IsGazeFixating == true) OnGazeFixation();

        IsGazeFixating_pre = IsGazeFixating;
        IsHeadFixating_pre = IsHeadFixating;
    }

    public float GetVisualGain()
    {
        return Vector3.Distance(GrabbedObject.position, GazeOrigin) / Vector3.Distance(HandPosition, GazeOrigin);
    }

    public float VitLerp(float x, float k1 = 0.8f / 3f, float k2 = 0.8f, float v1 = 0.2f, float v2 = 0.6f)
    {
        if (x <= v1)
            return k1;

        if (x >= v2)
            return k2;

        return k1 + (k2 - k1) / (v2 - v1) * (x - v1);
    }


    public void OnGazeFixation()
    {
        GazeDirection_OnGazeFixation = GazeDirection;
        HeadDirection_OnGazeFixation = HeadForward;

        HeadYAngle_OnGazeFixation = HeadYAngle;
        EyeInHeadYAngle_OnGazeFixation = EyeInHeadYAngle;
    }

    public void UpdateHeadDepthAnchor()
    {
        float maxEyeHeadAngle_Y_Up = 10f;
        float maxEyeHeadAngle_Y_Down = -15f;
        float maxHead_Y_up = 15f;
        float maxHead_Y_down = -15f;

        AvailableHeadY_Up = Math.Min(Math.Max(maxEyeHeadAngle_Y_Up - EyeInHeadYAngle_OnGazeFixation, 0) + HeadYAngle_OnGazeFixation, maxHead_Y_up);
        AvailableHeadY_Down = Math.Max(HeadYAngle_OnGazeFixation - Math.Max(EyeInHeadYAngle_OnGazeFixation - (maxEyeHeadAngle_Y_Down), 0), maxHead_Y_down);
    }
}