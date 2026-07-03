using UnityEngine;
using UnityEngine.Serialization;

public class TechniqueInputProvider : Singleton<TechniqueInputProvider>
{
    [Header("Fixation Settings")]
    [FormerlySerializedAs("GazeFixationDuration")]
    public float t_fixation = 0.25f;
    [FormerlySerializedAs("GazeFixationAngle")]
    public float theta_fixation = 3f;

    public TechniqueInputFrame Current { get; private set; } = new TechniqueInputFrame();

    private EyeGaze _gazeData;
    private HeadMovement _headData;
    private HandData _handData;
    private PinchDetector _pinchDetector;
    private FixationTracker _gazeFixationTracker;
    private OneEuroFilter<Vector3> _deltaHandMovementFilter;
    private int _lastRefreshFrame = -1;

    protected override void Awake()
    {
        base.Awake();

        _gazeFixationTracker = new FixationTracker(t_fixation, theta_fixation);
        _deltaHandMovementFilter = new OneEuroFilter<Vector3>(90f);
    }

    public void Refresh()
    {
        if (_lastRefreshFrame == Time.frameCount) return;

        ResolveSources();

        Current.DominantHand = GetDominantHand();
        Current.WasGazeFixating = Current.IsGazeFixating;

        RefreshHandInput();
        RefreshGazeInput();
        RefreshHeadInput();

        _lastRefreshFrame = Time.frameCount;
    }

    private void ResolveSources()
    {
        if (_handData == null) _handData = HandData.GetInstance();
        if (_pinchDetector == null) _pinchDetector = PinchDetector.GetInstance();
        if (_gazeData == null) _gazeData = EyeGaze.GetInstance();
        if (_headData == null) _headData = HeadMovement.GetInstance();
    }

    private Handedness GetDominantHand()
    {
        StudyControl studyControl = StudyControl.GetInstance();
        if (studyControl != null && studyControl.TechniqueControl != null)
        {
            return studyControl.TechniqueControl.DominantHand;
        }

        return Handedness.right;
    }

    private void RefreshHandInput()
    {
        Current.PinchPositionDelta = _handData.GetDeltaHandPosition(Current.DominantHand, usePinchTip: true);
        Current.PinchRotationDelta = _handData.GetDeltaHandRotation(Current.DominantHand, usePinchTip: true);
        Current.PinchPosition = _handData.GetHandPosition(Current.DominantHand, usePinchTip: true);
        Current.WristPosition = _handData.GetHandPosition(Current.DominantHand, usePinchTip: false);
        Current.HandTranslationSpeed = _handData.GetHandSpeed(Current.DominantHand, usePinchTip: true);
        Current.FilteredHandMovementVector = _deltaHandMovementFilter.Filter(Current.PinchPositionDelta);

        Current.PinchState = _pinchDetector.PinchState;
        Current.IsOneHandPinching = _pinchDetector.IsOneHandPinching;
        Current.IsNoHandPinchingLastFrame = _pinchDetector.IsNoHandPinching_LastFrame;
    }

    private void RefreshGazeInput()
    {
        Current.GazeRay = _gazeData.GetGazeRay();

        _gazeFixationTracker.UpdateThrshould(t_fixation, theta_fixation);
        Current.IsGazeFixating = _gazeFixationTracker.GetIsFixating(Current.GazeDirection);

        Current.EyeInHeadYAngle = _gazeData.EyeInHeadYAngle;
        Current.EyeInHeadXAngle = _gazeData.EyeInHeadXAngle;
        Current.FilteredEyeInHeadAngle = _gazeData.FilteredEyeInHeadAngle;
        Current.FilteredEyeInHeadAnglePrevious = _gazeData.FilteredEyeInHeadAngle_Pre;
    }

    private void RefreshHeadInput()
    {
        Transform cameraTransform = Camera.main.transform;

        Current.HeadForward = cameraTransform.forward;

        Current.HeadSpeed = _headData.HeadSpeed;
        Current.DeltaHeadY = _headData.DeltaHeadY;
    }
}
