using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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

    private OneEuroFilter<Vector3> _gazeDirFilter;
    private OneEuroFilter<Vector3> _gazePosFilter;

    enum Eye { Left = 0, Right = 1 };
    protected override void Awake()
    {
        base.Awake();

        _gazeDirFilter = new OneEuroFilter<Vector3>(FilterFrequency);
        _gazePosFilter = new OneEuroFilter<Vector3>(FilterFrequency);

        _headRotationBuffer = new List<Quaternion>();
    }

    public float EyeInHeadYAngle
    {
        get {return MathFunctions.AngleAroundAxis(_combinedGazeDir, Camera.main.transform.forward, Camera.main.transform.right);}
    }

    void Update()
    {
        // TODO: blink filtering

        // OVRPlugin.EyeGazesState _eyeGazesState = new OVRPlugin.EyeGazesState();
        // if (!OVRPlugin.GetEyeGazesState(OVRPlugin.Step.Render, -1, ref _eyeGazesState))
        // {
        //     Debug.Log("Failed to get eye gaze state from OVR");
        // }

        // OVRPlugin.EyeGazeState _leftEyeGazeState = _eyeGazesState.EyeGazes[(int)Eye.Left];
        // OVRPlugin.EyeGazeState _rightEyeGazeState = _eyeGazesState.EyeGazes[(int)Eye.Right];

        // OVRPose _leftEyePose = _leftEyeGazeState.Pose.ToOVRPose().ToHeadSpacePose();
        // OVRPose _rightEyePose = _rightEyeGazeState.Pose.ToOVRPose().ToHeadSpacePose();

        // _combinedGazeOrigin = Vector3.Lerp(_leftEyePose.position, _rightEyePose.position, 0.5f);
        // _combinedGazeDir = Vector3.Scale(Quaternion.Slerp(_leftEyePose.orientation, _rightEyePose.orientation, 0.5f).normalized * Vector3.forward, new Vector3(-1,1,-1));

        _combinedGazeOrigin = Vector3.Lerp(LeftEye.transform.position, RightEye.transform.position, 0.5f);
        _combinedGazeDir = Quaternion.Slerp(LeftEye.transform.rotation, RightEye.transform.rotation, 0.5f).normalized * Vector3.forward;

        _rawGazeOrigin = _combinedGazeOrigin;
        _rawGazeDir = _combinedGazeDir;

        if (FilteringGaze)
        {
            _gazeDirFilter.UpdateParams(FilterFrequency, FilterMinCutOff, FilterBeta, FitlerDcutoff);
            _gazePosFilter.UpdateParams(FilterFrequency, FilterMinCutOff, FilterBeta, FitlerDcutoff);

            _combinedGazeDir = _gazeDirFilter.Filter(_combinedGazeDir);
            _combinedGazeOrigin = _gazePosFilter.Filter(_combinedGazeOrigin);
        }

        if (CorrectGaze  && _headRotationBuffer.Count == FramOffset)
        {
            Quaternion headRotOffset = _headRotationBuffer[0] * Quaternion.Inverse(_headRotationBuffer[_headRotationBuffer.Count - 1]);
            _combinedGazeDir = headRotOffset * _combinedGazeDir;
        }

        GazeCursor.transform.position = GetGazeRay().origin + GetGazeRay().direction * 2f;
        GazeCursor.gameObject.SetActive(ShowGazeCursor);

        _gazeSpeed = Vector3.Angle(_combinedGazeDir, _combinedGazeDir_pre) / Time.deltaTime;

        _combinedGazeDir_pre = _combinedGazeDir;
        UpdateHeadRotationBuffer();
    }

    void UpdateHeadRotationBuffer()
    {
        Quaternion currentHeadRotation = Camera.main.transform.rotation;
        _headRotationBuffer.Add(currentHeadRotation);
        if (_headRotationBuffer.Count > FramOffset)
        {
            _headRotationBuffer.RemoveAt(0); // Remove oldest
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
