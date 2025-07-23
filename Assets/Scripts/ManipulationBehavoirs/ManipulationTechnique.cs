using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public interface IManipulationBehavior
{
    void OnSingleHandGrabbed(Transform target);
    void ApplyHandReleasedBehaviour();
    void ApplySingleHandGrabbedBehaviour();
    void ApplyBothHandGrabbedBehaviour();
}

public enum StaticState { Gaze, Head, Hand }

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
    public float HandStablizeThr = 0.3f; // in meters

    // Hand
    public Vector3 VirtualHandPosition { get; private set; }
    public Vector3 VirtualAnchorPosition { get; private set; }
    public HandData HandData { get; private set; }
    public Vector3 PinchPosition { get; private set; }
    public Vector3 PinchPosition_delta { get; private set; }
    public Vector3 WristPosition { get; private set; }
    public Vector3 WristPosition_delta { get; private set; }
    public Quaternion PinchRotation_delta { get; private set; }
    public float HandTranslationSpeed { get; private set; }
    public float HandRotationSpeed { get; private set; }
    public FixationTracker HandFixationTracker { get; private set; }
    public bool IsHandStablized { get; private set; }
    public Vector3 AccumulatedHandOffset { get; private set; } = Vector3.zero;



    // Gaze
    public EyeGaze GazeData { get; private set; }
    public Vector3 GazeOrigin { get; private set; }
    public Vector3 GazeDirection { get; private set; }
    public FixationTracker GazeFixationTracker { get; private set; }
    public bool IsGazeFixating { get; private set; }
    public bool IsGazeFixating_pre { get; private set; }
    public Vector3 GazeFixationCentroid { get; private set; }
    public bool IsGazeSaccading { get; private set; }
    public Vector3 GazeDirection_OnGazeFixation { get; private set; }
    public Vector3 HeadDirection_OnGazeFixation { get; private set; }
    public float EyeInHeadYAngle { get; private set; }
    public float EyeInHeadYAngle_OnGazeFixation { get; private set; }
    public List<Anchor> ObjectsInGazeCone_OnGazeFixation { get; private set; } = new List<Anchor>();


    // Head
    public HeadMovement HeadData { get; private set; }
    public Vector3 HeadForward { get; private set; }
    public Vector3 HeadRight { get; private set; }
    public Vector3 HeadPosition { get; private set; }
    public FixationTracker HeadFixationTracker { get; private set; }
    public bool IsHeadFixating { get; private set; }
    public bool IsHeadFixating_pre { get; private set; }
    public Vector3 HeadFixationCentroid { get; private set; }
    public float HeadSpeed { get; private set; }
    public float HeadYAngle { get; private set; }
    public float DeltaHeadY { get; private set; }
    public float Limit_HeadY_Up { get; private set; }
    public float Limit_HeadY_Down { get; private set; } // negtive value means down
    public float HeadYAngle_OnGazeFixation { get; private set; }

    public virtual void Awake()
    {
        GazeFixationTracker = new FixationTracker(GazeFixationDuration, GazeFixationAngle);
        HeadFixationTracker = new FixationTracker(HeadFixationDuration, HeadFixationAngle);
        HandFixationTracker = new FixationTracker(HandStablizeDuration, HandStablizeThr);

        VirtualHandPosition = HandData.GetInstance().GetHandPosition(usePinchTip: false);
    }

    public virtual void Update()
    {
        HandData = HandData.GetInstance();
        PinchPosition_delta = HandData.GetDeltaHandPosition(usePinchTip: true);
        PinchRotation_delta = HandData.GetDeltaHandRotation(usePinchTip: true);
        PinchPosition = HandData.GetHandPosition(usePinchTip: true);
        WristPosition = HandData.GetHandPosition(usePinchTip: false);
        WristPosition_delta = HandData.GetDeltaHandPosition(usePinchTip: false);
        HandTranslationSpeed = HandData.GetHandSpeed(usePinchTip: true);
        HandRotationSpeed = HandData.GetHandRotationSpeed(usePinchTip: true);
        HandFixationTracker.UpdateThrshould(HandStablizeDuration, HandStablizeThr);
        IsHandStablized = HandFixationTracker.GetIsFixating(PinchPosition);

        GazeData = EyeGaze.GetInstance();
        GazeOrigin = GazeData.GetGazeRay().origin;
        GazeDirection = GazeData.GetGazeRay().direction.normalized;
        IsGazeSaccading = GazeData.IsSaccading();
        GazeFixationTracker.UpdateThrshould(GazeFixationDuration, GazeFixationAngle);
        IsGazeFixating = GazeFixationTracker.GetIsFixating(GazeDirection);
        GazeFixationCentroid = GazeFixationTracker.FixationCentroid;
        EyeInHeadYAngle = GazeData.EyeInHeadYAngle;

        HeadData = HeadMovement.GetInstance();
        HeadForward = Camera.main.transform.forward;
        HeadRight = Camera.main.transform.right;
        HeadPosition = Camera.main.transform.position;
        HeadFixationTracker.UpdateThrshould(HeadFixationDuration, HeadFixationAngle);
        IsHeadFixating = HeadFixationTracker.GetIsFixating(HeadForward);
        HeadFixationCentroid = HeadFixationTracker.FixationCentroid;
        HeadSpeed = HeadData.HeadSpeed;
        DeltaHeadY = HeadData.DeltaHeadY;
        HeadYAngle = HeadData.HeadAngle_WorldY;


        if (IsGazeFixating_pre == false && IsGazeFixating == true) OnGazeFixation();




        UpdateAndSortAnchorList();
        if (ObjectsInGazeCone_OnGazeFixation.Count > 0)
        {
            Anchor closestAnchor = ObjectsInGazeCone_OnGazeFixation[0];

            if (closestAnchor.IsRealHand) VirtualHandPosition = WristPosition;
            else
            {
                // TODO: can be optimized when hit a shelf or plane, in this case should take the gaze hit point and aling the depth only. should be able to free with gaze pointnig.
                VirtualAnchorPosition = closestAnchor.transform.position + (WristPosition - PinchPosition);

                AccumulatedHandOffset += WristPosition_delta;
                VirtualHandPosition = VirtualAnchorPosition + AccumulatedHandOffset;
                VirtualHandPosition =  closestAnchor.transform.position + (PinchPosition - GazeOrigin);
            }
        }
        else
        {
            AccumulatedHandOffset = Vector3.zero;
            VirtualHandPosition = GetNewVirtualHandPosition();
        }

        IsGazeFixating_pre = IsGazeFixating;
        IsHeadFixating_pre = IsHeadFixating;
    }

    public virtual Vector3 GetNewVirtualHandPosition() { return VirtualHandPosition; }

    public float GetVisualGain()
    {
        return Vector3.Distance(VirtualAnchorPosition, GazeOrigin) / Vector3.Distance(PinchPosition, GazeOrigin);
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

    public void UpdateAndSortAnchorList()
    {
        Anchor[] anchors = FindObjectsByType<Anchor>(FindObjectsSortMode.None);

        if (anchors.Length != 0)
        {
            var sortedAnchors = anchors
                .Where(anchor => Vector3.Angle(GazeDirection, anchor.transform.position - GazeOrigin) < 10f && anchor.GrabbedState == GrabbedState.NotGrabbed)
                .OrderBy(anchor => Vector3.Angle(GazeDirection, anchor.transform.position - GazeOrigin))
                .ToList();

            ObjectsInGazeCone_OnGazeFixation.Clear();
            ObjectsInGazeCone_OnGazeFixation.AddRange(sortedAnchors);
        }
    }

    public void UpdateHeadDepthAnchor()
    {
        float maxEyeHeadAngle_Y_Up = 5f;
        float maxEyeHeadAngle_Y_Down = -15f;
        float maxHead_Y_up = 10f;
        float maxHead_Y_down = -10f;

        Limit_HeadY_Up = Math.Min(Math.Max(EyeInHeadYAngle - maxEyeHeadAngle_Y_Down, 0) + HeadYAngle, maxHead_Y_up);
        Limit_HeadY_Down = Math.Max(HeadYAngle - Math.Max(maxEyeHeadAngle_Y_Up - EyeInHeadYAngle, 0), maxHead_Y_down);
        // print(Limit_HeadY_Up + " " + Limit_HeadY_Down);
    }
}