using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum StaticState
{
    Gaze,
    Head
}

public class ManipulationTechnique : MonoBehaviour
{
    public virtual string TechniqueName => GetType().Name;

    [Header("Parameters")]
    [SerializeField] protected float MinDepth = 1f;
    [SerializeField] protected float MaxDepth = 11f;
    [SerializeField] protected float theta_thr = 15f;
    [SerializeField] protected float v_min = 0.1f;
    [SerializeField] protected float v_max = 0.6f;
    [SerializeField] protected float G_min = 0f;
    [SerializeField] protected float G_max = 0.8f;

    public ManipulatableObject GrabbedObject { get; private set; }
    public ManipulatableObject LastGrabbedObject { get; private set; }
    public ManipulatableObject GazingObject { get; private set; }

    private TechniqueInputProvider _inputProvider;

    protected TechniqueInputProvider InputProvider
    {
        get
        {
            if (_inputProvider == null) _inputProvider = TechniqueInputProvider.GetInstance();
            _inputProvider.Refresh();
            return _inputProvider;
        }
    }

    protected TechniqueInputFrame InputFrame => InputProvider.Current;

    public Vector3 VirtualHandPosition { get; protected set; }
    public Vector3 PinchPosition => InputFrame.PinchPosition;
    public Vector3 PinchPosition_delta => InputFrame.PinchPositionDelta;
    public Quaternion PinchRotation_delta => InputFrame.PinchRotationDelta;
    public Vector3 WristPosition => InputFrame.WristPosition;
    public Vector3 WristPosition_delta => InputFrame.WristPositionDelta;
    public float HandTranslationSpeed => InputFrame.HandTranslationSpeed;
    public float HandRotationSpeed => InputFrame.HandRotationSpeed;
    public bool IsHandStablized => InputFrame.IsHandStabilized;
    public Vector3 VirtualHandPosition_OnGrab { get; private set; }
    public Vector3 ObjectPosition_OnGrab { get; private set; }

    public Vector3 GazeOrigin => InputFrame.GazeOrigin;
    public Vector3 GazeDirection => InputFrame.GazeDirection;
    public bool IsGazeFixating => InputFrame.IsGazeFixating;
    public bool IsGazeFixating_pre => InputFrame.WasGazeFixating;
    public Vector3 GazeFixationCentroid => InputFrame.GazeFixationCentroid;
    public bool IsGazeSaccading => InputFrame.IsGazeSaccading;
    public Vector3 GazeDirection_OnGazeFixation => InputFrame.GazeDirectionOnGazeFixation;
    public Vector3 HeadDirection_OnGazeFixation => InputFrame.HeadDirectionOnGazeFixation;
    public float EyeInHeadYAngle => InputFrame.EyeInHeadYAngle;
    public float Filtered_EyeInHeadAngle => InputFrame.FilteredEyeInHeadAngle;
    public float Filtered_EyeInHeadAngle_Pre => InputFrame.FilteredEyeInHeadAnglePrevious;
    public float EyeInHeadXAngle => InputFrame.EyeInHeadXAngle;
    public float EyeInHeadYAngle_OnGazeFixation => InputFrame.EyeInHeadYAngleOnGazeFixation;
    public Vector3 Filtered_HandMovementVector => InputFrame.FilteredHandMovementVector;

    public Vector3 HeadForward => InputFrame.HeadForward;
    public Vector3 HeadRight => InputFrame.HeadRight;
    public Vector3 HeadPosition => InputFrame.HeadPosition;
    public bool IsHeadFixating => InputFrame.IsHeadFixating;
    public bool IsHeadFixating_pre => InputFrame.WasHeadFixating;
    public Vector3 HeadFixationCentroid => InputFrame.HeadFixationCentroid;
    public float HeadSpeed => InputFrame.HeadSpeed;
    public float HeadYAngle => InputFrame.HeadYAngle;
    public float DeltaHeadY => InputFrame.DeltaHeadY;
    public float Limit_HeadY_Up => InputFrame.LimitHeadYUp;
    public float Limit_HeadY_Down => InputFrame.LimitHeadYDown;
    public float HeadYAngle_OnGazeFixation => InputFrame.HeadYAngleOnGazeFixation;

    public List<ManipulatableObject> ObjectsInGazeCone { get; private set; } = new List<ManipulatableObject>();

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
    public virtual void TriggerOnLookAtNewObjectBehavior() { }

    public virtual void Awake()
    {
        InputProvider.Refresh();
        VirtualHandPosition = WristPosition;
    }

    public virtual void Update()
    {
        InputProvider.Refresh();
        UpdateAndSortObjectInGazeConeList();

        if (GrabbedObject == null)
        {
            if (ObjectsInGazeCone.Count > 0)
            {
                if (GazingObject != ObjectsInGazeCone[0])
                {
                    GazingObject = ObjectsInGazeCone[0];
                    TriggerOnLookAtNewObjectBehavior();
                }
            }
            else
            {
                GazingObject = null;
            }

            if (GazingObject != null)
            {
                if (InputFrame.IsOneHandPinching && InputFrame.IsNoHandPinchingLastFrame)
                {
                    TriggerOnSingleHandGrabbed(GazingObject, GrabbedState.Grabbed_Indirect);
                }
                else
                {
                    ApplyGazingButNotGrabbingBehaviour();
                }
            }
            else
            {
                ApplyObjectFreeBehaviour();
            }
        }
        else
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
                if (InputFrame.IsOneHandPinching)
                {
                    ApplyIndirectGrabbedBehaviour();
                }
                else
                {
                    TriggerOnHandReleased();
                }
            }
        }
    }

    public float VitLerp(float x, float k1 = 0.8f / 3f, float k2 = 0.8f, float v1 = 0.2f, float v2 = 0.6f)
    {
        if (x <= v1) return k1;
        if (x >= v2) return k2;
        return k1 + (k2 - k1) / (v2 - v1) * (x - v1);
    }

    public void TriggerOnGazeFixation()
    {
        InputProvider.CaptureGazeFixation();
    }

    public void ObjectHighlight(bool highlight, ManipulatableObject obj)
    {
        if (obj.TryGetComponent(out Outline outline))
        {
            outline.enabled = highlight;
        }
    }

    public void UpdateHeadInputRange()
    {
        InputProvider.Refresh();
    }

    public void UpdateAndSortObjectInGazeConeList()
    {
        ManipulatableObject[] anchors = FindObjectsByType<ManipulatableObject>(FindObjectsSortMode.None);

        var sortedAnchors = anchors
            .Where(anchor => anchor.IsHitbyGaze)
            .OrderBy(anchor => anchor.AngleToGaze)
            .ToList();

        ObjectsInGazeCone.Clear();
        ObjectsInGazeCone.AddRange(sortedAnchors);
    }

    public StaticState CurrentState { get; protected set; } = StaticState.Gaze;

    public float VisualGainValue { get; protected set; }
    public Vector3 OffsetAddedByHand { get; protected set; }
    public float AngleRotatedByHand { get; protected set; }
    public float CurrentDistanceToGaze { get; protected set; }
    public Vector3 HeadDepthOffset { get; protected set; }
    public float DistanceToGazeAfterAddingHeadDepth { get; protected set; }
    public float AngleGazeDirectionToObject { get; protected set; }

    public float theta_gain_min { get; protected set; } = 30;
    public float theta_gain_max { get; protected set; } = 10;
    public float BaseGain { get; protected set; }
    public float EdgeGain { get; protected set; }
    public float PitchGain { get; protected set; }

    public Vector3 HeadDepthOffset_base { get; protected set; }
    public float Attenuation { get; protected set; } = 1;
}
