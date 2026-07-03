using System.Collections.Generic;
using UnityEngine;

public class EyeGaze : Singleton<EyeGaze>
{
    public OVREyeGaze LeftEye, RightEye;

    public bool ShowGazeCursor;
    public Transform GazeCursor;

    private Vector3 _combinedGazeOrigin, _combinedGazeDir;
    private Vector3 _rawGazeOrigin, _rawGazeDir;

    private Vector3 _combinedGazeDir_pre;

    private float _gazeSpeed;

    private List<Quaternion> _headRotationBuffer;
    public float FilteredEyeInHeadAngle { get; private set; }
    public float FilteredEyeInHeadAngle_Pre { get; private set; }

    public float SaccadeThr = 120f;
    
    [Header("Gaze Correction")]
    public bool CorrectGaze;
    public int FramOffset = 7;

    [Header("One Euro Filter")]
    public bool FilteringGaze = true;

    public float FilterFrequency = 90f;
    public float FilterMinCutOff = 0.05f;
    public float FilterBeta = 10f;
    public float FitlerDcutoff = 1f;

    [Header("Eye Tracking Health")]
    public float HeadAlignedWarningAngle = 0.5f;
    public float HeadAlignedWarningDuration = 1.5f;
    public float HeadAlignedWarningRepeatInterval = 5f;

    private OneEuroFilter<Vector3> _gazeDirFilter;
    private OneEuroFilter<Vector3> _gazePosFilter;
    private OneEuroFilter _eyeInHeadAngleFilter;
    private float _headAlignedDuration;
    private float _lastHeadAlignedWarningTime = -999f;
    private bool _warnedMissingEyeBindings;

    protected override void Awake()
    {
        base.Awake();
        EnsureInternalState();
    }

    public float EyeInHeadAngle
    {
        get
        {
            Transform cameraTransform = Camera.main != null ? Camera.main.transform : null;
            return cameraTransform != null ? Vector3.Angle(cameraTransform.forward, _combinedGazeDir) : 0f;
        }
    }

    public float EyeInHeadYAngle
    {
        get
        {
            Transform cameraTransform = Camera.main != null ? Camera.main.transform : null;
            return cameraTransform != null ? MathFunctions.AngleAroundAxis(_combinedGazeDir, cameraTransform.forward, cameraTransform.right) : 0f;
        }
    }

    public float EyeInHeadXAngle
    {
        get
        {
            Transform cameraTransform = Camera.main != null ? Camera.main.transform : null;
            return cameraTransform != null ? MathFunctions.AngleAroundAxis(_combinedGazeDir, cameraTransform.forward, cameraTransform.up) : 0f;
        }
    }

    void Update()
    {
        EnsureInternalState();

        Transform cameraTransform = Camera.main != null ? Camera.main.transform : null;
        if (LeftEye == null || RightEye == null)
        {
            if (!_warnedMissingEyeBindings)
            {
                Debug.LogWarning("EyeGaze is missing OVREyeGaze bindings. Falling back to the main camera gaze ray.", this);
                _warnedMissingEyeBindings = true;
            }

            _combinedGazeOrigin = cameraTransform != null ? cameraTransform.position : Vector3.zero;
            _combinedGazeDir = cameraTransform != null ? cameraTransform.forward : Vector3.forward;
        }
        else
        {
            _combinedGazeOrigin = Vector3.Lerp(LeftEye.transform.position, RightEye.transform.position, 0.5f);
            _combinedGazeDir = Quaternion.Slerp(LeftEye.transform.rotation, RightEye.transform.rotation, 0.5f).normalized * Vector3.forward;
        }

        _rawGazeOrigin = _combinedGazeOrigin;
        _rawGazeDir = _combinedGazeDir;

        if (FilteringGaze)
        {
            _gazeDirFilter.UpdateParams(FilterFrequency, FilterMinCutOff, FilterBeta, FitlerDcutoff);
            _gazePosFilter.UpdateParams(FilterFrequency, FilterMinCutOff, FilterBeta, FitlerDcutoff);

            _combinedGazeDir = _gazeDirFilter.Filter(_combinedGazeDir);
            _combinedGazeOrigin = _gazePosFilter.Filter(_combinedGazeOrigin);
        }

        if (CorrectGaze && _headRotationBuffer.Count == FramOffset)
        {
            Quaternion headRotOffset = _headRotationBuffer[0] * Quaternion.Inverse(_headRotationBuffer[_headRotationBuffer.Count - 1]);
            _combinedGazeDir = headRotOffset * _combinedGazeDir;
        }

        if (GazeCursor != null)
        {
            GazeCursor.transform.position = GetGazeRay().origin + GetGazeRay().direction * 2f;
            GazeCursor.gameObject.SetActive(ShowGazeCursor);
        }

        float deltaTime = Mathf.Max(Time.deltaTime, 0.0001f);
        _gazeSpeed = Vector3.Angle(_combinedGazeDir, _combinedGazeDir_pre) / deltaTime;
        FilteredEyeInHeadAngle_Pre = FilteredEyeInHeadAngle;
        FilteredEyeInHeadAngle = _eyeInHeadAngleFilter.Filter(cameraTransform != null ? Vector3.Angle(cameraTransform.forward, _combinedGazeDir) : 0f);
        UpdateEyeTrackingHealth(cameraTransform);

        _combinedGazeDir_pre = _combinedGazeDir;
        UpdateHeadRotationBuffer();
    }

    private void EnsureInternalState()
    {
        if (_gazeDirFilter == null) _gazeDirFilter = new OneEuroFilter<Vector3>(FilterFrequency);
        if (_gazePosFilter == null) _gazePosFilter = new OneEuroFilter<Vector3>(FilterFrequency);
        if (_eyeInHeadAngleFilter == null) _eyeInHeadAngleFilter = new OneEuroFilter(FilterFrequency);
        if (_headRotationBuffer == null) _headRotationBuffer = new List<Quaternion>();
    }

    private void UpdateEyeTrackingHealth(Transform cameraTransform)
    {
        if (cameraTransform == null) return;

        float gazeHeadAngle = Vector3.Angle(cameraTransform.forward, _combinedGazeDir);
        if (gazeHeadAngle <= HeadAlignedWarningAngle)
        {
            _headAlignedDuration += Time.deltaTime;
            if (_headAlignedDuration >= HeadAlignedWarningDuration &&
                Time.unscaledTime - _lastHeadAlignedWarningTime >= HeadAlignedWarningRepeatInterval)
            {
                Debug.LogWarning("Eye tracking may be unavailable: gaze direction has stayed aligned with head direction. Check headset eye tracking permissions and calibration.", this);
                _lastHeadAlignedWarningTime = Time.unscaledTime;
            }
        }
        else
        {
            _headAlignedDuration = 0f;
        }
    }

    void UpdateHeadRotationBuffer()
    {
        Transform cameraTransform = Camera.main != null ? Camera.main.transform : null;
        if (cameraTransform == null) return;

        Quaternion currentHeadRotation = cameraTransform.rotation;
        _headRotationBuffer.Add(currentHeadRotation);
        if (_headRotationBuffer.Count > FramOffset)
        {
            _headRotationBuffer.RemoveAt(0);
        }
    }

    public Ray GetGazeRay()
    {
        return new Ray(_combinedGazeOrigin, _combinedGazeDir);
    }

    public Vector3 GetRawGazeOrigin()
    {
        return _rawGazeOrigin;
    }
    public Vector3 GetRawGazeDirection()
    {
        return _rawGazeDir;
    }

    public Transform GetGazeHitTrans()
    {
        RaycastHit hit;
        if (Physics.Raycast(GetGazeRay(), out hit, 100f))
        {
            return hit.transform;
        }
        return null;
    }

    public bool IsSaccading()
    {
        return _gazeSpeed >= SaccadeThr;
    }
}
