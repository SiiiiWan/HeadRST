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
    private bool _warnedMissingHandSource;
    private bool _warnedMissingGazeSource;
    private bool _warnedMissingHeadSource;

    protected override void Awake()
    {
        base.Awake();
        EnsureInternalState();
    }

    public void Refresh()
    {
        if (_lastRefreshFrame == Time.frameCount) return;

        EnsureInternalState();
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

    private void EnsureInternalState()
    {
        if (_gazeFixationTracker == null) _gazeFixationTracker = new FixationTracker(t_fixation, theta_fixation);
        if (_deltaHandMovementFilter == null) _deltaHandMovementFilter = new OneEuroFilter<Vector3>(90f);
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
        if (_handData == null || _pinchDetector == null)
        {
            if (!_warnedMissingHandSource)
            {
                Debug.LogWarning("TechniqueInputProvider is missing hand input bindings. Hand input will be ignored until the bindings are restored.", this);
                _warnedMissingHandSource = true;
            }

            Current.PinchPositionDelta = Vector3.zero;
            Current.PinchRotationDelta = Quaternion.identity;
            Current.PinchState = PinchState.NotPinching;
            Current.IsOneHandPinching = false;
            Current.IsNoHandPinchingLastFrame = true;
            return;
        }

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
        if (_gazeData == null)
        {
            if (!_warnedMissingGazeSource)
            {
                Debug.LogWarning("TechniqueInputProvider is missing EyeGaze. Falling back to the main camera direction.", this);
                _warnedMissingGazeSource = true;
            }

            Transform cameraTransform = Camera.main != null ? Camera.main.transform : null;
            Current.GazeRay = cameraTransform != null
                ? new Ray(cameraTransform.position, cameraTransform.forward)
                : new Ray(Vector3.zero, Vector3.forward);
            Current.IsGazeFixating = false;
            return;
        }

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
        if (_headData == null || Camera.main == null)
        {
            if (!_warnedMissingHeadSource)
            {
                Debug.LogWarning("TechniqueInputProvider is missing head input bindings. Head input will use neutral values until the bindings are restored.", this);
                _warnedMissingHeadSource = true;
            }

            Current.HeadForward = Vector3.forward;
            Current.HeadSpeed = 0f;
            Current.DeltaHeadY = 0f;
            return;
        }

        Transform cameraTransform = Camera.main.transform;

        Current.HeadForward = cameraTransform.forward;

        Current.HeadSpeed = _headData.HeadSpeed;
        Current.DeltaHeadY = _headData.DeltaHeadY;
    }
}
