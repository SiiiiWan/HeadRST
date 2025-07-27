using UnityEngine;
using System.Collections.Generic;
using UnityEngine.XR;
using UnityEngine.Experimental.GlobalIllumination;

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

        PreCamPos = CamPos;
        CamPos = Camera.main.transform.position;

        PreCamDir = CamDir;
        CamDir = Camera.main.transform.forward;

        PreCamRotation = CamRotation;
        CamRotation = Camera.main.transform.rotation;

        Pre_HeadAngle_WorldY = HeadAngle_WorldY;
        HeadAngle_WorldY = _headAngleYFilter.Filter(MathFunctions.AngleFrom_XZ_Plane(CamDir));
    }

    public Quaternion CamRotation {get; private set;}
    public Quaternion PreCamRotation {get; private set;}

    public Vector3 CamPos {get; private set;}
    public Vector3 PreCamPos {get; private set;}

    public Vector3 CamDir  {get; private set;}
    public Vector3 PreCamDir {get; private set;}

    private OneEuroFilter _headAngleYFilter;
    public float HeadAngle_WorldY { get; private set; }
    public float Pre_HeadAngle_WorldY {get; private set;}


    public float HeadSpeed
    {
        get {return Mathf.Abs(FilteredHeadVel.magnitude); }
    }
    public Ray HeadRay
    {
        get { return new Ray(CamPos, CamDir); }
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
        get {return Vector3.Angle(CamDir, PreCamDir);}
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
        HeadRollAngle = MathFunctions.AngleAroundAxis(Camera.main.transform.up, Vector3.up, CamDir); // right positive; left negtive
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
