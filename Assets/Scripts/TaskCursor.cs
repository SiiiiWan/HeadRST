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
    public PinchDetector PinchDetector { get; private set; }

    public HeadMovement HeadData { get; private set; }
    public Vector3 HeadForward { get; private set; }
    public float HeadSpeed { get; private set; }
    public float DeltaHeadY { get; private set; }

    float _fixationStartDepth, _accumulatedDepthOffset;


    private StaticState _currentMode = StaticState.Gaze;

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
        PinchDetector = PinchDetector.GetInstance();

        HeadData = HeadMovement.GetInstance();
        HeadSpeed = HeadData.HeadSpeed;
        DeltaHeadY = HeadData.DeltaHeadY;
        HeadForward = Camera.main.transform.forward;

        if (PinchDetector.IsOneHandPinching)
        {
            transform.position += PinchPosition_delta * Mathf.Max(1, GetVisualGain(transform.position));

            // if (_currentMode == StaticState.Gaze)
            // {
            //     // transform.position = CubeManager.GetInstance().GetCenterOfFocusedCubes();

            //     // Set Position along Gaze Ray
            //     transform.position = GazeOrigin + GazeDirection * Vector3.Distance(GazeOrigin, transform.position);

            //     Vector3 directionFromGazeOrigin = (transform.position - GazeOrigin).normalized;
            //     _accumulatedDepthOffset = Mathf.Clamp(_accumulatedDepthOffset + DeltaHeadY * 0.2f, 1f, 11f);
            //     transform.position = GazeOrigin + directionFromGazeOrigin * _accumulatedDepthOffset;

            //     // // Apply Head Depth Offset
            //     // Vector3 directionFromGazeOrigin = (transform.position - GazeOrigin).normalized;
            //     // float baseGain = VitLerp(Math.Abs(HeadSpeed), 0, 0.8f, 0.1f, 0.6f);
            //     // float edgeGain = EyeHeadGain();
            //     // transform.position += directionFromGazeOrigin * DeltaHeadY * baseGain * edgeGain;

            //     // // Clamp Depth within Min and Max
            //     // transform.position = GazeOrigin + directionFromGazeOrigin * Mathf.Clamp(Vector3.Distance(transform.position, GazeOrigin), 1f, 11f);

            //     // Check to switch to Head state

            //     _accumulatedDepthOffset += DeltaHeadY * 0.4f;

            //     if (IsGazeFixating)
            //     {
            //         _currentMode = StaticState.Head;
            //     }
            // }
            // else
            // {

            //     // Apply Head Depth Offset
            //     // Vector3 directionFromGazeOrigin = (transform.position - GazeOrigin).normalized;
            //     // // float baseGain = VitLerp(Math.Abs(HeadSpeed), 0, 0.8f, 0.1f, 0.6f);
            //     // // float edgeGain = EyeHeadGain();
            //     // float angleHeadForwardToObjectDirection = MathFunctions.AngleAroundAxis(HeadForward, directionFromGazeOrigin, Camera.main.transform.right);

            //     // if (angleHeadForwardToObjectDirection >= 5f)
            //     // {
            //     //     _accumulatedDepthOffset = Mathf.Clamp(_accumulatedDepthOffset + Time.deltaTime * 8f, 1f, 11f);
            //     //     transform.position = GazeOrigin + directionFromGazeOrigin * Mathf.Max(Mathf.Round(_accumulatedDepthOffset / 3f) * 3f, 1f);
            //     // }

            //     // if (angleHeadForwardToObjectDirection <= -5f)
            //     // {
            //     //     _accumulatedDepthOffset = Mathf.Clamp(_accumulatedDepthOffset + Time.deltaTime * -8f, 1f, 11f);
            //     //     transform.position = GazeOrigin + directionFromGazeOrigin * Mathf.Max(Mathf.Round(_accumulatedDepthOffset / 3f) * 3f, 1f);
            //     // }


            //     // transform.position += directionFromGazeOrigin * DeltaHeadY * baseGain * edgeGain;

            //         // Clamp Depth within Min and Max
            //         // transform.position = GazeOrigin + directionFromGazeOrigin * Mathf.Clamp(Vector3.Distance(transform.position, GazeOrigin), 1f, 11f);


            //         // Check to switch back to Gaze state
            //         float angleGazeDirectionToObject = Vector3.Angle(GazeDirection, transform.position - GazeOrigin);
            //     if (IsGazeFixating == false && angleGazeDirectionToObject > 15f) _currentMode = StaticState.Gaze; // 15 degrees threshold catches gaze little saccade during hand correction with distance gain
            // }


            // float baseGain = VitLerp(Math.Abs(HeadSpeed), 0, 0.8f, 0.1f, 0.6f);
            // float edgeGain = EyeHeadGain();

            // transform.position += PinchPosition_delta * Mathf.Max(1, GetVisualGain(transform.position));
            // Vector3 directionFromGazeOrigin = (transform.position - GazeOrigin).normalized;

            // if (_currentMode == StaticState.Gaze)
            // {
            //     // transform.position = CubeManager.GetInstance().GetCenterOfFocusedCubes();

            //     // Set Position along Gaze Ray
            //     transform.position = GazeOrigin + GazeDirection * Vector3.Distance(GazeOrigin, transform.position);

            //     // Apply Head Depth Offset
            //     transform.position += directionFromGazeOrigin * DeltaHeadY * baseGain * edgeGain;

            //     // Clamp Depth within Min and Max
            //     transform.position = GazeOrigin + directionFromGazeOrigin * Mathf.Clamp(Vector3.Distance(transform.position, GazeOrigin), 1f, 11f);

            //     // Check to switch to Head state
            //     if (IsGazeFixating)
            //     {
            //         _currentMode = StaticState.Head;
            //         _fixationStartDepth = Vector3.Distance(transform.position, GazeOrigin);
            //         _accumulatedDepthOffset = 0f;
            //     }
            // }
            // else
            // {
            //     _accumulatedDepthOffset += DeltaHeadY * baseGain * edgeGain;
            //     transform.position = GazeOrigin + directionFromGazeOrigin * (_fixationStartDepth + Mathf.Round(_accumulatedDepthOffset / 3f) * 3f);
            //     // Clamp Depth within Min and Max
            //     transform.position = GazeOrigin + directionFromGazeOrigin * Mathf.Clamp(Vector3.Distance(transform.position, GazeOrigin), 1f, 11f);

            //     // Check to switch back to Gaze state
            //     float angleGazeDirectionToObject = Vector3.Angle(GazeDirection, transform.position - GazeOrigin);
            //     if (IsGazeFixating == false && angleGazeDirectionToObject > 15f)
            //     {
            //         _currentMode = StaticState.Gaze; // 15 degrees threshold catches gaze little saccade during hand correction with distance gain
            //     }

            // }

        }
        else
        {
            if(PinchDetector.IsOneHandPinching_LastFrame) _currentMode = StaticState.Gaze;

            if (_currentMode == StaticState.Gaze)
            {
                // // Enable to allow changing depth with hand during gaze shifts
                // transform.position += PinchPosition_delta * Mathf.Max(1, GetVisualGain(transform.position));

                // transform.position = GazeOrigin + GazeDirection * Vector3.Distance(GazeOrigin, transform.position);

                transform.position = GazeOrigin + GazeDirection * 1.5f;


                if (IsGazeFixating)
                {
                    _currentMode = StaticState.Head;
                    // ManipulatableObject closestObject = ObjectManager.GetInstance().UpdateAndGetClosestFocusedObject();
                    // if (closestObject != null) transform.position = closestObject.transform.position;
                }
            }
            else
            {

                transform.position += PinchPosition_delta * Mathf.Max(1, GetVisualGain(transform.position));
                // transform.position += PinchPosition_delta;


                // ManipulatableCube closestCube = CubeManager.GetInstance().UpdateAndGetClosestFocusedCube();
                // if (closestCube != null) transform.position = closestCube.transform.position;

                // Check to switch back to Gaze state
                float angleGazeDirectionToObject = Vector3.Angle(GazeDirection, transform.position - GazeOrigin);
                // if (GazeData.IsSaccading() == true)
                if (IsGazeFixating == false)
                {
                    _currentMode = StaticState.Gaze;
                }
            }
        }


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

}
