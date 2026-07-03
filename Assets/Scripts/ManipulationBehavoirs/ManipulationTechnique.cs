using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class ManipulationTechnique : MonoBehaviour
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

    public virtual void TriggerOnGrabbed(ManipulatableObject obj)
    {
        GrabbedObject = obj;
        GrabbedObject.SetGrabbedState(GrabbedState.Grabbed);
    }

    public abstract void ApplyGrabbedBehaviour();

    public virtual void TriggerOnHandReleased()
    {
        GrabbedObject.SetGrabbedState(GrabbedState.NotGrabbed);
        GrabbedObject = null;
    }

    public virtual void Awake()
    {
        InputProvider.Refresh();
    }

    public virtual void Update()
    {
        InputProvider.Refresh();
        UpdateAndSortObjectInGazeConeList();

        if (GrabbedObject == null)
        {
            GazingObject = ObjectsInGazeCone.Count > 0 ? ObjectsInGazeCone[0] : null;

            if (GazingObject != null && InputFrame.IsOneHandPinching && InputFrame.IsNoHandPinchingLastFrame)
            {
                TriggerOnGrabbed(GazingObject);
            }
        }
        else
        {
            if (InputFrame.IsOneHandPinching)
            {
                ApplyGrabbedBehaviour();
            }
            else
            {
                TriggerOnHandReleased();
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

