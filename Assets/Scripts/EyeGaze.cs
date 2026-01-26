using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;

public class EyeGaze : Singleton<EyeGaze>
{
    public OVREyeGaze LeftEye, RightEye;
    public bool FilterBlink = true;
    public bool EyesOpen { get; private set; } = false;

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
    public int FrameOffset = 7;

    [Header("Gaze Correction")]
    private FixationTracker _gazeFixationTracker;

        [Header("Gaze Fixation Detection")]
    private bool _isGazeFixating_DT;
    public float Duration_SetOnAwake = 0.25f;
    public float Dispersion_SetOnAwake = 3f;

    [Header("One Euro Filter")]

    public bool FilteringGaze = true;


    public float FilterFrequency = 90f;
    public float FilterMinCutOff = 0.05f;
    public float FilterBeta = 10f;
    public float FitlerDcutoff = 1f;

    private OneEuroFilter<Vector3> _gazeDirFilter;
    private OneEuroFilter<Vector3> _gazePosFilter;
    private OneEuroFilter _eyeInHeadAngleFilter;

    enum Eye { Left = 0, Right = 1 };
    protected override void Awake()
    {
        base.Awake();

        _gazeDirFilter = new OneEuroFilter<Vector3>(FilterFrequency);
        _gazePosFilter = new OneEuroFilter<Vector3>(FilterFrequency);
        _eyeInHeadAngleFilter = new OneEuroFilter(FilterFrequency);

        _headRotationBuffer = new List<Quaternion>();

        _gazeFixationTracker = new FixationTracker(Duration_SetOnAwake, Dispersion_SetOnAwake);
    }

    public float EyeInHeadAngle
    {
        get { return Vector3.Angle(Camera.main.transform.forward, _combinedGazeDir); }
    }
    public float EyeInHeadYAngle
    {
        get { return MathFunctions.AngleAroundAxis(_combinedGazeDir, Camera.main.transform.forward, Camera.main.transform.right); }
    }

    public float EyeInHeadXAngle
    {
        get {return MathFunctions.AngleAroundAxis(_combinedGazeDir, Camera.main.transform.forward, Camera.main.transform.up);}
    }

    void Update()
    {
        if (FilterBlink)
        {
            if (EyesOpen) _combinedGazeOrigin = Vector3.Lerp(LeftEye.transform.position, RightEye.transform.position, 0.5f);
            if (EyesOpen) _combinedGazeDir = Quaternion.Slerp(LeftEye.transform.rotation, RightEye.transform.rotation, 0.5f).normalized * Vector3.forward;
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

        if (CorrectGaze && _headRotationBuffer.Count == FrameOffset)
        {
            Quaternion headRotOffset = _headRotationBuffer[0] * Quaternion.Inverse(_headRotationBuffer[_headRotationBuffer.Count - 1]);
            _combinedGazeDir = headRotOffset * _combinedGazeDir;
        }

        GazeCursor.transform.position = GetGazeRay().origin + GetGazeRay().direction * 2f;
        GazeCursor.gameObject.SetActive(ShowGazeCursor);

        _gazeSpeed = Vector3.Angle(_combinedGazeDir, _combinedGazeDir_pre) / Time.deltaTime;
        FilteredEyeInHeadAngle_Pre = FilteredEyeInHeadAngle;
        FilteredEyeInHeadAngle = _eyeInHeadAngleFilter.Filter(Vector3.Angle(Camera.main.transform.forward, _combinedGazeDir));
        
        _isGazeFixating_DT = _gazeFixationTracker.GetIsFixating(_combinedGazeDir);

        _combinedGazeDir_pre = _combinedGazeDir;
        UpdateHeadRotationBuffer();
    }

    void UpdateHeadRotationBuffer()
    {
        Quaternion currentHeadRotation = Camera.main.transform.rotation;
        _headRotationBuffer.Add(currentHeadRotation);
        if (_headRotationBuffer.Count > FrameOffset)
        {
            _headRotationBuffer.RemoveAt(0); // Remove oldest
        }
    }

    // public void OnBlink()
    // {
    //     print("Blink detected");
    // }

    public void OnEyesClosed()
    {
        EyesOpen = false;
        print("Eyes closed");
    }

    public void OnEyesOpened()
    {
        EyesOpen = true;
        print("Eyes opened");
    }

    public Ray GetGazeRay()
    {
        return new Ray(_combinedGazeOrigin, _combinedGazeDir);
    }

    public Vector3 GetGazeOrigin()
    {
        return _combinedGazeOrigin;
    }
    public Vector3 GetGazeDirection()
    {
        return _combinedGazeDir.normalized;
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

    public bool GetGazeHitPoint(out Vector3 hitPoint)
    {
        hitPoint = Vector3.zero;

        if (Physics.Raycast(GetGazeRay(), out RaycastHit hit, 100f))
        {
            hitPoint = hit.point;
            return true;
        }

        return false;
    }

    public bool IfGazeHitObjectsContains(GameObject obj, float maxDistance=100f)
    {
        RaycastHit[] hits;
        hits = Physics.RaycastAll(GetGazeRay(), maxDistance);

        if (hits.Length > 0)
        {
            foreach (var hit in hits)
            {
                if (hit.transform == obj.transform) return true;
            }
        }

        return false;
    }

    public bool GetGazeHitPoint_Sphere(out  RaycastHit hit, float radius)
    {
        if (Physics.SphereCast(GetGazeRay(), radius, out hit, 100f))
        {
            return true;
        }

        return false;
    }

    public bool IsSaccading_VT()
    {
        return _gazeSpeed >= SaccadeThr;
    }

    public bool IsFixating_DT()
    {
        return _isGazeFixating_DT;
    }
}
