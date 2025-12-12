using UnityEngine;
using System;
using Unity.Mathematics;

public class TaskCursor : MonoBehaviour
{
    public EyeGaze GazeData { get; private set; }
    public Vector3 GazeOrigin { get; private set; }
    public Vector3 GazeDirection { get; private set; }
    public float EyeInHeadXAngle { get; private set; }
    public float EyeInHeadYAngle { get; private set; }
    public bool IsGazeFixating { get; private set; }
    public FixationTracker GazeFixationTracker { get; private set; }
    public float Filtered_EyeInHeadAngle { get; private set; }
    public float Filtered_EyeInHeadAngle_Pre { get; private set; }

    public HandData HandData { get; private set; }
    public Vector3 PinchPosition { get; private set; }
    public Vector3 PinchPosition_delta { get; private set; }
    public Quaternion PinchRotation_delta { get; private set; }
    public PinchDetector PinchDetector { get; private set; }

    public HeadMovement HeadData { get; private set; }
    public Vector3 HeadForward { get; private set; }
    public float HeadSpeed { get; private set; }
    public float DeltaHeadY { get; private set; }
    public Vector3 HeadPosition { get; private set; }
    public Vector3 HeadPosition_delta { get; private set; }

    float _fixationStartDepth, _accumulatedDepthOffset, _headPitchOnFixation;
    float _pitchDiffFromFixation, _pitchDiffFromFixation_LastFrame;
    Vector3 _pinchPositionOnFixation;

    Vector3 _headPositionOnFixation;

    private StaticState _currentMode = StaticState.Gaze;
    private float _depthUpdateTimer = 0f;

    public virtual void Awake()
    {
        GazeFixationTracker = new FixationTracker(0.25f, 3f);
    }

    void Update()
    {
        GazeData = EyeGaze.GetInstance();
        GazeOrigin = GazeData.GetGazeRay().origin;
        GazeDirection = GazeData.GetGazeRay().direction.normalized;
        EyeInHeadYAngle = GazeData.EyeInHeadYAngle;
        EyeInHeadXAngle = GazeData.EyeInHeadXAngle;
        IsGazeFixating = GazeFixationTracker.GetIsFixating(GazeDirection);
        Filtered_EyeInHeadAngle = GazeData.FilteredEyeInHeadAngle;

        HandData = HandData.GetInstance();
        PinchPosition_delta = HandData.GetDeltaHandPosition(usePinchTip: true);
        PinchPosition = HandData.GetHandPosition(usePinchTip: true);
        PinchRotation_delta = HandData.GetDeltaHandRotation(usePinchTip: true);
        PinchDetector = PinchDetector.GetInstance();

        HeadData = HeadMovement.GetInstance();
        HeadSpeed = HeadData.HeadSpeed;
        DeltaHeadY = HeadData.DeltaHeadY;
        HeadForward = Camera.main.transform.forward;
        HeadPosition_delta = Camera.main.transform.position - HeadPosition;
        HeadPosition = Camera.main.transform.position;

    
        if (PinchDetector.IsNoHandPinching)
        {

            transform.position += PinchPosition_delta * Mathf.Max(1, GetVisualGain(transform.position));
            Vector3 directionFromGazeOrigin = (transform.position - GazeOrigin).normalized;

            // transform.position += directionFromGazeOrigin * DeltaHeadY * 0.4f;
            // transform.position = GazeOrigin + directionFromGazeOrigin * Mathf.Clamp(Vector3.Distance(transform.position, GazeOrigin), 1, 10f);


            if (_currentMode == StaticState.Gaze)
            {
                transform.position = GazeOrigin + GazeDirection * Vector3.Distance(GazeOrigin, transform.position + directionFromGazeOrigin * DeltaHeadY * 0.4f);
                transform.position = GazeOrigin + GazeDirection * Mathf.Clamp(Vector3.Distance(transform.position, GazeOrigin), 1, 100f);



                if (IsGazeFixating)
                {
                    _currentMode = StaticState.Head;
                    _headPitchOnFixation = HeadData.HeadAngle_WorldY;
                    _pinchPositionOnFixation = PinchPosition;
                }
            }
            else
            {
                // transform.position = GazeOrigin + directionFromGazeOrigin * Vector3.Distance(GazeOrigin, transform.position + directionFromGazeOrigin * DeltaHeadY * 0.4f);
                // transform.position = GazeOrigin + directionFromGazeOrigin * Mathf.Clamp(Vector3.Distance(transform.position, GazeOrigin), 1, 100f);
                if(HeadData.HeadAngle_WorldY - _headPitchOnFixation >= 5f)
                {
                    transform.position += directionFromGazeOrigin * (HeadData.HeadAngle_WorldY - _headPitchOnFixation - 5f) * MathFunctions.Deg2Meter(Time.deltaTime, Vector3.Distance(transform.position, GazeOrigin)) * 10;                    
                }

                if(HeadData.HeadAngle_WorldY - _headPitchOnFixation <= -3f)
                {
                    transform.position -= directionFromGazeOrigin * (-HeadData.HeadAngle_WorldY + _headPitchOnFixation - 3f) * MathFunctions.Deg2Meter(Time.deltaTime, Vector3.Distance(transform.position, GazeOrigin)) * 10;                   
                }

                transform.position = GazeOrigin + directionFromGazeOrigin * Mathf.Clamp(Vector3.Distance(transform.position, GazeOrigin), 1, 100f);

                


                if (IsGazeFixating == false) // dont swtich back during small saccades assessing the big object correction; read the object hit box information 
                {
                    _currentMode = StaticState.Gaze;
                }
            }
        }
        else // if hand is pinching
        {
        

            transform.position += PinchPosition_delta * Mathf.Max(1, GetVisualGain(transform.position));
            // transform.position += PinchPosition_delta * Mathf.Max(1f, Vector3.Distance(transform.position, GazeOrigin));


            transform.rotation = PinchRotation_delta * transform.rotation;

            
            // Vector3 directionFromGazeOrigin = (transform.position - GazeOrigin).normalized;

            // // transform.position += directionFromGazeOrigin * DeltaHeadY * 0.4f;
            // // transform.position = GazeOrigin + directionFromGazeOrigin * Mathf.Clamp(Vector3.Distance(transform.position, GazeOrigin), 1, 10f);


            // if (_currentMode == StaticState.Gaze)
            // {
            //     transform.position = GazeOrigin + GazeDirection * Vector3.Distance(GazeOrigin, transform.position + directionFromGazeOrigin * DeltaHeadY * 0.4f);
            //     transform.position = GazeOrigin + GazeDirection * Mathf.Clamp(Vector3.Distance(transform.position, GazeOrigin), 1, 100f);



            //     if (IsGazeFixating)
            //     {
            //         _currentMode = StaticState.Head;
            //         _headPitchOnFixation = HeadData.HeadAngle_WorldY;
            //         _pinchPositionOnFixation = PinchPosition;
            //     }
            // }
            // else
            // {
            //     transform.position = GazeOrigin + directionFromGazeOrigin * Vector3.Distance(GazeOrigin, transform.position + directionFromGazeOrigin * DeltaHeadY * 0.4f);
            //     transform.position = GazeOrigin + directionFromGazeOrigin * Mathf.Clamp(Vector3.Distance(transform.position, GazeOrigin), 1, 100f);
            //     // if(HeadData.HeadAngle_WorldY - _headPitchOnFixation >= 5f)
            //     // {
            //     //     transform.position += directionFromGazeOrigin * (HeadData.HeadAngle_WorldY - _headPitchOnFixation - 5f) * MathFunctions.Deg2Meter(Time.deltaTime, Vector3.Distance(transform.position, GazeOrigin)) * 10;                    
            //     // }

            //     // if(HeadData.HeadAngle_WorldY - _headPitchOnFixation <= -3f)
            //     // {
            //     //     transform.position -= directionFromGazeOrigin * (-HeadData.HeadAngle_WorldY + _headPitchOnFixation - 3f) * MathFunctions.Deg2Meter(Time.deltaTime, Vector3.Distance(transform.position, GazeOrigin)) * 10;                   
            //     // }

            //     // transform.position = GazeOrigin + directionFromGazeOrigin * Mathf.Clamp(Vector3.Distance(transform.position, GazeOrigin), 1, 100f);

                


            //     if (IsGazeFixating == false) // dont swtich back during small saccades assessing the big object correction; read the object hit box information 
            //     {
            //         _currentMode = StaticState.Gaze;
            //     }
            // }

        }

        SetCursorVisibility(PinchDetector.IsNoHandPinching);

        Filtered_EyeInHeadAngle_Pre = GazeData.FilteredEyeInHeadAngle_Pre;
    }

    private float GetVisualGain(Vector3 objectPosition)
    {
        return Mathf.Max(1f, Vector3.Distance(objectPosition, GazeOrigin) / Vector3.Distance(PinchPosition, GazeOrigin));
    }

    public float EyeHeadGain()
    {
        float eyeRange = GetEyeRange(EyeInHeadXAngle, EyeInHeadYAngle);
        float k = 3;
        float boostStartDeg = eyeRange / k;

        float gain = 1;

        float gazeAngleFromHead = Vector3.Angle(GazeDirection, HeadForward);

        if (gazeAngleFromHead >= boostStartDeg & Filtered_EyeInHeadAngle > Filtered_EyeInHeadAngle_Pre) gain = linearDepthFunction_TwoPoints(gazeAngleFromHead, new Vector2(boostStartDeg, 1), new Vector2(eyeRange, k));

        return gain;
    }

    float GetEyeRange(float x, float y, float up_lim = 15, float down_lim = 30, float side_lim = 30)
    {
        if (y >= 0) return (1 - (1 - (up_lim / side_lim)) * Mathf.Sin(Mathf.Atan2(y, x))) * side_lim;
        else return (1 + (1 - (down_lim / side_lim)) * Mathf.Sin(Mathf.Atan2(y, x))) * side_lim;
    }

    public float VitLerp(float x, float k1 = 0.8f / 3f, float k2 = 0.8f, float v1 = 0.2f, float v2 = 0.6f)
    {
        if (x <= v1)
            return k1;

        if (x >= v2)
            return k2;

        return k1 + (k2 - k1) / (v2 - v1) * (x - v1);
    }

    protected float linearDepthFunction_TwoPoints(float x, Vector2 left, Vector2 right)
    {
        float k = (right.y - left.y) / (right.x - left.x);

        float b = right.y - k * right.x;

        return k * x + b;
    }
    
    public void SetCursorPositionTo(Vector3 newPosition)
    {
        transform.position = newPosition;
    }

    public void SetCursorVisibility(bool isVisible)
    {
         transform.GetComponent<MeshRenderer>().enabled = isVisible;
    }

    void PlaceSelfToClosestObject()
    {
        ManipulatableObject closestObject = ObjectManager.GetInstance().UpdateAndGetClosestFocusedObject();
        if (closestObject != null) transform.position = closestObject.transform.position;
    }
}
