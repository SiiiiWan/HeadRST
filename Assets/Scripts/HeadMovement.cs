using UnityEngine;
using System.Collections.Generic;
using UnityEngine.XR;
using System.Linq;
public class HeadMovement : Singleton<HeadMovement>
{
    protected override void Awake()
    {
        base.Awake();

        _headVelWindow = new List<float>();

        _headAccFilter = new OneEuroFilter(90f);
        _headVelFilter = new OneEuroFilter<Vector3>(90f);

        _rollSpdFilter = new OneEuroFilter(90f);
        _rollAccFilter = new OneEuroFilter(90f);
        
        _headAngleYFilter = new OneEuroFilter(90f);
    }

    void Start()
    {
        getHMD();
    }

    void Update()
    {
        if(!deviceDetected)
        {
            getHMD(); 
            return;
        }

        UpdateHeadVel();
        UpdateHeadAcc();
        UpdateHeadRoll();

        HeadPosition_Pre = HeadPosition;
        HeadPosition = Camera.main.transform.position;

        HeadForward_Pre = HeadForward;
        HeadForward = Camera.main.transform.forward;

        HeadRotation_Pre = HeadRotation;
        HeadRotation = Camera.main.transform.rotation;

        Pre_HeadAngle_WorldY = HeadAngle_WorldY;
        HeadAngle_WorldY = _headAngleYFilter.Filter(MathFunctions.AngleFrom_XZ_Plane(HeadForward));
    }

    public Quaternion HeadRotation {get; private set;}
    public Quaternion HeadRotation_Pre {get; private set;}

    public Vector3 HeadPosition {get; private set;}
    public Vector3 HeadPosition_Pre {get; private set;}
    public Vector3 DeltaHeadPosition
    {
        get {return HeadPosition - HeadPosition_Pre;}
    }

    public Vector3 HeadForward  {get; private set;}
    public Vector3 HeadForward_Pre {get; private set;}

    private OneEuroFilter _headAngleYFilter;
    public float HeadAngle_WorldY { get; private set; }
    public float Pre_HeadAngle_WorldY {get; private set;}


    public float HeadSpeed
    {
        get {return Mathf.Abs(FilteredHeadVel.magnitude); }
    }
    private readonly List<(float time, float speed)> _headSpeedHistory = new List<(float, float)>();
    private const float HeadStableTimeWindow = 0.7f; // 700ms

    public bool IsHeadStable(float speedThreshold = 0.2f)
    {
        // Ensure enough time has passed to have a meaningful average over the window.
        if (Time.time < HeadStableTimeWindow)
        {
            return false;
        }

        // If there's no history, we can't determine stability.
        if (!_headSpeedHistory.Any())
        {
            return false;
        }

        float historyTimeSpan = _headSpeedHistory.Last().time - _headSpeedHistory.First().time;
        if (historyTimeSpan < HeadStableTimeWindow * 0.9f)
        {
            return false;
        }

        // Calculate the average speed from the recorded history.
        float averageSpeed = _headSpeedHistory.Average(entry => entry.speed);

        return averageSpeed < speedThreshold;
    }
    public void ResetHeadStabilityHistory()
    {
        _headSpeedHistory.Clear();
    }

    public Ray HeadRay
    {
        get { return new Ray(HeadPosition, HeadForward); }
    }

    public Vector3 HeadDirXZ
    {
        get {return new Vector3(Camera.main.transform.forward.x, 0, Camera.main.transform.forward.z).normalized;}
    }

    public float DeltaHeadY
    {
        get {return HeadAngle_WorldY - Pre_HeadAngle_WorldY;}
    }

    public float DeltaHeadRotation
    {
        get {return Vector3.Angle(HeadForward, HeadForward_Pre);}
        // get {return Quaternion.Angle(CamRotation, PreCamRotation);} // including head roll
    }

    private OneEuroFilter _rollSpdFilter, _rollAccFilter;

    public float HeadRollAngle {get; private set;}
    public float Pre_HeadRollAngle {get; private set;}
    public float RawHeadRollSpeed {get; private set;}
    public float Pre_RawHeadRollSpeed {get; private set;}
    public float FilteredHeadRollSpeed {get; private set;}
    public float FilteredHeadRollAcc {get; private set;}
    public float RawHeadRollAcc {get; private set;}

    void UpdateHeadRoll()
    {
        HeadRollAngle = MathFunctions.AngleAroundAxis(Camera.main.transform.up, Vector3.up, HeadForward); // right positive; left negtive
        RawHeadRollSpeed = (HeadRollAngle - Pre_HeadRollAngle) / Time.deltaTime; // positive = rolling right; negative = rolling left
        FilteredHeadRollSpeed = _rollSpdFilter.Filter(RawHeadRollSpeed);

        RawHeadRollAcc = (RawHeadRollSpeed - Pre_RawHeadRollSpeed) / Time.deltaTime;
        FilteredHeadRollAcc = _rollAccFilter.Filter(RawHeadRollAcc);
        
        Pre_HeadRollAngle = HeadRollAngle;
        Pre_RawHeadRollSpeed = RawHeadRollSpeed;
    }

    [HideInInspector] public float RawHeadSpeed;
    [HideInInspector] public Vector3 FilteredHeadVel;
    private List<float> _headVelWindow;
    private int _headVelWindowSize = 2;
    private OneEuroFilter<Vector3> _headVelFilter;

    void UpdateHeadVel()
    {   
        inputHeadset.TryGetFeatureValue(CommonUsages.deviceAngularVelocity, out Vector3 headAngularVel);

        _headVelWindow.Add(headAngularVel.magnitude);

        if(_headVelWindow.Count > _headVelWindowSize)
        {
            _headVelWindow.RemoveAt(0);
        }

        FilteredHeadVel = _headVelFilter.Filter(headAngularVel);
        RawHeadSpeed = headAngularVel.magnitude;

        // Record head speed history for stability check
        _headSpeedHistory.Add((Time.time, HeadSpeed));
        // Clean up old entries to keep the list from growing indefinitely
        _headSpeedHistory.RemoveAll(entry => Time.time - entry.time > HeadStableTimeWindow);
    }

    [HideInInspector] public float HeadAcc, FilteredHeadAcc;
    private OneEuroFilter _headAccFilter;

    void UpdateHeadAcc()
    {
        if(_headVelWindow.Count != _headVelWindowSize) return;

        float acc = (_headVelWindow[_headVelWindowSize - 1] - _headVelWindow[_headVelWindowSize - 2]) / Time.deltaTime;

        FilteredHeadAcc = _headAccFilter.Filter(acc);

        HeadAcc = acc;
    }

    /// Detects HMD
    private List<InputDevice> devices;
    private InputDeviceCharacteristics desiredCharacteristics;
    private InputDevice inputHeadset;
    private bool deviceDetected;

    void getHMD(){

        devices = new List<InputDevice>();

        desiredCharacteristics = InputDeviceCharacteristics.HeadMounted;
        InputDevices.GetDevicesWithCharacteristics(desiredCharacteristics, devices);

        if(devices.Count > 0)
        {
            inputHeadset = devices[0];            
            deviceDetected = true;
        }
    }
}
