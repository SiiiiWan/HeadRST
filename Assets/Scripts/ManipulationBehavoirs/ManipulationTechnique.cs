using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public class ManipulationTechnique : MonoBehaviour
{
    #region Manipulation Behaviors

    public float MaxDepth { get; set; } = 11f;
    public float MinDepth { get; set; } = 1f;

    public ManipulatableObject GrabbedObject { get; private set; }
    public ManipulatableObject LastGrabbedObject { get; private set; }

    public ManipulatableObject GazingObject { get; private set; }

    public virtual void TriggerOnSingleHandGrabbed(ManipulatableObject obj, GrabbedState grabbedState)
    {
        GrabbedObject = obj;
        LastGrabbedObject = obj;
        GrabbedObject.SetGrabbedState(grabbedState);
        StudyControl.GetInstance().IsAfterFirstPickUpInTrial = true;

        TriggerOnGazeFixation();

        VirtualHandPosition_OnGrab = VirtualHandPosition;
        ObjectPosition_OnGrab = GrabbedObject.transform.position;
    }

    public virtual void ApplyIndirectGrabbedBehaviour() { }
    public virtual void ApplyDirectGrabbedBehaviour() { }
    public virtual void ApplyGazingButNotGrabbingBehaviour() { }
    public virtual void ApplyObjectFreeBehaviour() { }

    public virtual void TriggerOnHandReleased()
    {
        GrabbedObject.SetGrabbedState(GrabbedState.NotGrabbed);
        GrabbedObject = null;
    }

    public virtual void TriggerOnLookAtHandBehavior() { }
    public virtual void TriggerOnLookAtNewObjectBehavior()
    {
        // ObjectHighlight(true, GazingObject);
    }

    #endregion

    #region Tracking Data

    [Header("Fixation Settings")]
    public float GazeFixationDuration { get; protected set; } = 0.25f; // in seconds
    public float GazeFixationAngle { get; protected set; } = 3f; // in degrees

    public float HeadFixationDuration { get; protected set; } = 0.25f; // in seconds
    public float HeadFixationAngle { get; protected set; } = 1.5f; // in degrees

    public float HandStablizeDuration { get; protected set; } = 0.25f; // in seconds
    public float HandStablizeThr { get; protected set; } = 0.3f; // in meters

    // Hand
    public Vector3 VirtualHandPosition { get; protected set; }
    public HandData HandData { get; private set; }
    public PinchDetector PinchDetector { get; private set; }
    public Vector3 PinchPosition { get; private set; }
    public Vector3 PinchPosition_delta { get; private set; }
    public Vector3 WristPosition { get; private set; }
    public Vector3 WristPosition_delta { get; private set; }
    public Quaternion PinchRotation_delta { get; private set; }
    public float HandTranslationSpeed { get; private set; }
    public float HandRotationSpeed { get; private set; }
    public FixationTracker HandFixationTracker { get; private set; }
    public bool IsHandStablized { get; private set; }
    public Vector3 VirtualHandPosition_OnGrab { get; private set; }
    public Vector3 ObjectPosition_OnGrab { get; private set; }



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
    public float Filtered_EyeInHeadAngle { get; private set; }
    public float Filtered_EyeInHeadAngle_Pre { get; private set; }
    public float EyeInHeadXAngle { get; private set; }
    public float EyeInHeadYAngle_OnGazeFixation { get; private set; }
    public List<ManipulatableObject> ObjectsInGazeCone { get; private set; } = new List<ManipulatableObject>();
    public OneEuroFilter<Vector3> DeltaHandMovementFilter { get; private set; } = new OneEuroFilter<Vector3>(90f);
    public Vector3 Filtered_HandMovementVector { get; private set; }


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

    public void UpdateTrackingData()
    {
        HandData = HandData.GetInstance();
        PinchDetector = PinchDetector.GetInstance();
        PinchPosition_delta = HandData.GetDeltaHandPosition(usePinchTip: true);
        PinchRotation_delta = HandData.GetDeltaHandRotation(usePinchTip: true);
        PinchPosition = HandData.GetHandPosition(usePinchTip: true);
        WristPosition = HandData.GetHandPosition(usePinchTip: false);
        WristPosition_delta = HandData.GetDeltaHandPosition(usePinchTip: false);
        HandTranslationSpeed = HandData.GetHandSpeed(usePinchTip: true);
        Filtered_HandMovementVector = DeltaHandMovementFilter.Filter(PinchPosition_delta);
        HandRotationSpeed = HandData.GetHandRotationSpeed(usePinchTip: true);
        HandFixationTracker.UpdateThrshould(HandStablizeDuration, HandStablizeThr);
        IsHandStablized = HandFixationTracker.GetIsFixating(PinchPosition);

        GazeData = EyeGaze.GetInstance();
        GazeOrigin = GazeData.GetGazeRay().origin;
        GazeDirection = GazeData.GetGazeRay().direction.normalized;
        IsGazeSaccading = GazeData.IsSaccading();
        GazeFixationTracker.UpdateThrshould(GazeFixationDuration, GazeFixationAngle);
        IsGazeFixating = GazeFixationTracker.GetIsFixating(GazeDirection);
        if (IsGazeFixating_pre == false && IsGazeFixating == true) TriggerOnGazeFixation();
        GazeFixationCentroid = GazeFixationTracker.FixationCentroid;
        EyeInHeadYAngle = GazeData.EyeInHeadYAngle;
        EyeInHeadXAngle = GazeData.EyeInHeadXAngle;
        Filtered_EyeInHeadAngle = GazeData.FilteredEyeInHeadAngle;

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

        ExtraUpdateTrackingData();
    }

    public virtual void ExtraUpdateTrackingData()
    {

    }

    public void UpdatePreData()
    {
        IsGazeFixating_pre = IsGazeFixating;
        IsHeadFixating_pre = IsHeadFixating;
        Filtered_EyeInHeadAngle_Pre = GazeData.FilteredEyeInHeadAngle_Pre;
    }

    #endregion

    #region MonoBehaviour Methods
    public virtual void Awake()
    {
        GazeFixationTracker = new FixationTracker(GazeFixationDuration, GazeFixationAngle);
        HeadFixationTracker = new FixationTracker(HeadFixationDuration, HeadFixationAngle);
        HandFixationTracker = new FixationTracker(HandStablizeDuration, HandStablizeThr);

        VirtualHandPosition = HandData.GetInstance().GetHandPosition(usePinchTip: false);
    }

    public virtual void Update()
    {
        UpdateTrackingData();

        UpdateAndSortObjectInGazeConeList();

        if (GrabbedObject == null) // not grabbed
        {
            // check if gaze is fixating on an object
            if (ObjectsInGazeCone.Count > 0) // has object in gaze cone
            {
                if (GazingObject != ObjectsInGazeCone[0]) // trigger on gazing different object
                {
                    GazingObject = ObjectsInGazeCone[0];
                    TriggerOnLookAtNewObjectBehavior();
                }
            }
            else // no object in gaze cone
            {
                GazingObject = null;
            }

            // take action based on gaze state
            if (GazingObject != null)
            {
                // if (GazingObject.Grabbable.SelectingPointsCount > 0)
                // {
                //     TriggerOnSingleHandGrabbed(GazingObject, GrabbedState.Grabbed_Direct);
                // }
                if (PinchDetector.IsOneHandPinching && PinchDetector.IsNoHandPinchingOrGrabbing_LastFrame)
                {
                    TriggerOnSingleHandGrabbed(GazingObject, GrabbedState.Grabbed_Indirect);
                }
                else // gaze hover but not grabbed yet
                {
                    ApplyGazingButNotGrabbingBehaviour();
                }
            }
            else
            {
                ApplyObjectFreeBehaviour();
            }
        }
        else // grabbed
        {
            if (GrabbedObject.GrabbedState == GrabbedState.Grabbed_Direct)
            {
                if (GrabbedObject.Grabbable.SelectingPointsCount > 0)
                {
                    ApplyDirectGrabbedBehaviour();
                }
                else
                {
                    TriggerOnHandReleased();
                }

            }
            else if (GrabbedObject.GrabbedState == GrabbedState.Grabbed_Indirect)
            {
                if (PinchDetector.IsOneHandPinching)
                {
                    ApplyIndirectGrabbedBehaviour();
                }
                else
                {
                    TriggerOnHandReleased();
                }
            }
        }

        UpdatePreData();
    }
    # endregion

    #region Accessory Functions


    public float VitLerp(float x, float k1 = 0.8f / 3f, float k2 = 0.8f, float v1 = 0.2f, float v2 = 0.6f)
    {
        if (x <= v1)
            return k1;

        if (x >= v2)
            return k2;

        return k1 + (k2 - k1) / (v2 - v1) * (x - v1);
    }


    public void TriggerOnGazeFixation()
    {
        GazeDirection_OnGazeFixation = GazeDirection;
        HeadDirection_OnGazeFixation = HeadForward;

        HeadYAngle_OnGazeFixation = HeadYAngle;
        EyeInHeadYAngle_OnGazeFixation = EyeInHeadYAngle;
    }

    public void ObjectHighlight(bool highlight, ManipulatableObject obj)
    {
        Outline outline;
        if (outline = obj.GetComponent<Outline>())
        {
            outline.enabled = highlight;
        }
    }

    public void UpdateHeadInputRange()
    {
        float maxEyeHeadAngle_Y_Up = 5f;
        float maxEyeHeadAngle_Y_Down = -15f;
        float maxHead_Y_up = 10f;
        float maxHead_Y_down = -10f;

        Limit_HeadY_Up = Math.Min(Math.Max(EyeInHeadYAngle - maxEyeHeadAngle_Y_Down, 0) + HeadYAngle, maxHead_Y_up);
        Limit_HeadY_Down = Math.Max(HeadYAngle - Math.Max(maxEyeHeadAngle_Y_Up - EyeInHeadYAngle, 0), maxHead_Y_down);
    }

    public void UpdateAndSortObjectInGazeConeList()
    {
        // Get all objects currently in gaze cone, sort by angle to gaze
        ManipulatableObject[] anchors = FindObjectsByType<ManipulatableObject>(FindObjectsSortMode.None);

        if (anchors.Length != 0)
        {
            var sortedAnchors = anchors
                .Where(anchor => anchor.IsHitbyGaze)
                .OrderBy(anchor => anchor.AngleToGaze)
                .ToList();

            ObjectsInGazeCone.Clear();
            ObjectsInGazeCone.AddRange(sortedAnchors);
        }
    }
    #endregion

    // Special Technique Variables
    public StaticState CurrentState { get; protected set; } = StaticState.Gaze;

    public float VisualGainValue { get; protected set; }
    public Vector3 OffsetAddedByHand { get; protected set; }
    public float AngleRotatedByHand { get; protected set; }
    public float CurrentDistanceToGaze { get; protected set; }
    public Vector3 HeadDepthOffset { get; protected set; }
    public float DistanceToGazeAfterAddingHeadDepth { get; protected set; }
    public float AngleGazeDirectionToObject { get; protected set; }

    public float MinHeadSpeed { get; protected set; } = 0.1f;
    public float MaxHeadSpeed { get; protected set; } = 0.6f;

    public float MinGainDeg { get; protected set; } = 30;
    public float MaxGainDeg { get; protected set; } = 10;
    public float BaseGain { get; protected set; }
    public float EdgeGain { get; protected set; }

    // att
    public Vector3 HeadDepthOffset_base { get; protected set; }
    public float Attenuation { get; protected set; } = 1;

}