using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ManipulationTechnique : MonoBehaviour
{
    public virtual string TechniqueName => GetType().Name;

    public ManipulatableObject GrabbedObject { get; private set; }
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
    public float HandTranslationSpeed => InputFrame.HandTranslationSpeed;

    public Vector3 GazeOrigin => InputFrame.GazeOrigin;
    public Vector3 GazeDirection => InputFrame.GazeDirection;
    public bool IsGazeFixating => InputFrame.IsGazeFixating;
    public float EyeInHeadYAngle => InputFrame.EyeInHeadYAngle;
    public float Filtered_EyeInHeadAngle => InputFrame.FilteredEyeInHeadAngle;
    public float Filtered_EyeInHeadAngle_Pre => InputFrame.FilteredEyeInHeadAnglePrevious;
    public float EyeInHeadXAngle => InputFrame.EyeInHeadXAngle;
    public Vector3 Filtered_HandMovementVector => InputFrame.FilteredHandMovementVector;

    public Vector3 HeadForward => InputFrame.HeadForward;
    public float HeadSpeed => InputFrame.HeadSpeed;
    public float DeltaHeadY => InputFrame.DeltaHeadY;

    public List<ManipulatableObject> ObjectsInGazeCone { get; private set; } = new List<ManipulatableObject>();

    public virtual void TriggerOnSingleHandGrabbed(ManipulatableObject obj, GrabbedState grabbedState)
    {
        GrabbedObject = obj;
        GrabbedObject.SetGrabbedState(grabbedState);
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

    public float VisualGainValue { get; protected set; }
    public Vector3 OffsetAddedByHand { get; protected set; }
    public float AngleRotatedByHand { get; protected set; }
}
