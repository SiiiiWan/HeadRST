using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Pen : MonoBehaviour
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

    float _accumulatedDepthOffset, _headPitchOnFixation;
    float _pitchDiffFromFixation, _pitchDiffFromFixation_LastFrame;
    private float _depthUpdateTimer = 0f;

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

        _pitchDiffFromFixation = HeadData.HeadAngle_WorldY - _headPitchOnFixation;


        if (PinchDetector.IsOneHandPinching)
        {
            Draw();
            transform.position += PinchPosition_delta * Mathf.Max(1, GetVisualGain(transform.position));
        }
        else
        {
            _currentDrawing = null;

            // transform.position += PinchPosition_delta * Mathf.Max(1, GetVisualGain(transform.position));

            // if (_currentMode == StaticState.Gaze)
            // {
            //     // Set Position along Gaze Ray
            //     transform.position = GazeOrigin + GazeDirection * Vector3.Distance(GazeOrigin, transform.position);

            //     // Apply Head Depth Offset
            //     Vector3 directionFromGazeOrigin = (transform.position - GazeOrigin).normalized;

            //     float baseGain = VitLerp(Math.Abs(HeadSpeed), 0, 0.8f, 0.1f, 0.6f);
            //     float edgeGain = EyeHeadGain();

            //     transform.position += directionFromGazeOrigin * DeltaHeadY * baseGain * edgeGain;

            //     // Clamp Depth within Min and Max
            //     transform.position = GazeOrigin + directionFromGazeOrigin * Mathf.Clamp(Vector3.Distance(transform.position, GazeOrigin), 1f, 11f);

            //     // Check to switch to Head state
            //     if (IsGazeFixating) _currentMode = StaticState.Head;
            // }
            // else
            // {

            //     // Check to switch back to Gaze state
            //     float angleGazeDirectionToObject = Vector3.Angle(GazeDirection, transform.position - GazeOrigin);
            //     if (IsGazeFixating == false && angleGazeDirectionToObject > 15f) _currentMode = StaticState.Gaze; // 15 degrees threshold catches gaze little saccade during hand correction with distance gain
            // }

            transform.position += PinchPosition_delta * Mathf.Max(1, GetVisualGain(transform.position));
            Vector3 directionFromGazeOrigin = (transform.position - GazeOrigin).normalized;

            transform.position += directionFromGazeOrigin * DeltaHeadY * 0.4f;
            transform.position = GazeOrigin + directionFromGazeOrigin * Mathf.Clamp(Vector3.Distance(transform.position, GazeOrigin), 1, 10f);

            if (_currentMode == StaticState.Gaze)
            {
                // transform.position = GazeOrigin + GazeDirection * Vector3.Distance(GazeOrigin, transform.position);
                
                // transform.position += GazeDirection * DeltaHeadY * 0.4f;
                transform.position = GazeOrigin + GazeDirection * Mathf.Clamp(Vector3.Distance(transform.position, GazeOrigin), 1, 10f);

                if (IsGazeFixating)
                {
                    _currentMode = StaticState.Head;
                    _headPitchOnFixation = HeadData.HeadAngle_WorldY;
                }
            }
            else
            {

                // if (_pitchDiffFromFixation >= 4f && _pitchDiffFromFixation - _pitchDiffFromFixation_LastFrame > 0f)
                // {
                //     _accumulatedDepthOffset = Mathf.Clamp(_accumulatedDepthOffset + Time.deltaTime * 8f, 1f, 11f);
                //     transform.position = GazeOrigin + directionFromGazeOrigin * Mathf.Max(Mathf.Round(_accumulatedDepthOffset / 3f) * 3f, 1f);
                // }

                // if (_pitchDiffFromFixation <= -4f && _pitchDiffFromFixation - _pitchDiffFromFixation_LastFrame < 0f)
                // {
                //     _accumulatedDepthOffset = Mathf.Clamp(_accumulatedDepthOffset + Time.deltaTime * -8f, 1f, 11f);
                //     transform.position = GazeOrigin + directionFromGazeOrigin * Mathf.Max(Mathf.Round(_accumulatedDepthOffset / 3f) * 3f, 1f);
                // }

                // bool isMoving = false;
                // if (_pitchDiffFromFixation >= 4f && _pitchDiffFromFixation - _pitchDiffFromFixation_LastFrame > 0f)
                // {
                //     isMoving = true;
                //     _depthUpdateTimer += Time.deltaTime;
                //     if (_depthUpdateTimer >= 0.375f)
                //     {
                //         _depthUpdateTimer = 0f; // Reset timer
                //         // The original rate was 8 units/sec. For a 0.375s interval, the step is 8 * 0.375 = 3.
                //         _accumulatedDepthOffset = Mathf.Clamp(_accumulatedDepthOffset + 3f, 1f, 11f);
                //         transform.position = GazeOrigin + directionFromGazeOrigin * Mathf.Max(Mathf.Round(_accumulatedDepthOffset / 3f) * 3f, 1f);
                //     }
                // }

                // if (_pitchDiffFromFixation <= -4f && _pitchDiffFromFixation - _pitchDiffFromFixation_LastFrame < 0f)
                // {
                //     isMoving = true;
                //     _depthUpdateTimer += Time.deltaTime;
                //     if (_depthUpdateTimer >= 0.375f)
                //     {
                //         _depthUpdateTimer = 0f; // Reset timer
                //         _accumulatedDepthOffset = Mathf.Clamp(_accumulatedDepthOffset - 3f, 1f, 11f);
                //         transform.position = GazeOrigin + directionFromGazeOrigin * Mathf.Max(Mathf.Round(_accumulatedDepthOffset / 3f) * 3f, 1f);
                //     }
                // }

                // // If head movement stops, reset the timer
                // if (!isMoving)
                // {
                //     _depthUpdateTimer = 0f;
                // }

                if (IsGazeFixating == false) // dont swtich back during small saccades assessing the big object correction; read the object hit box information 
                {
                    _currentMode = StaticState.Gaze;
                }
            }

            _pitchDiffFromFixation_LastFrame = _pitchDiffFromFixation;
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
    

    private LineRenderer _currentDrawing;
    private int _index;

    private void Draw()
    {
        if (_currentDrawing == null)
        {
            _index = 0;
            _currentDrawing = new GameObject().AddComponent<LineRenderer>();
            _currentDrawing.startWidth = 0.1f;
            _currentDrawing.endWidth = 0.1f;
            _currentDrawing.material = new Material(Shader.Find("Sprites/Default"));
            _currentDrawing.positionCount = 1;
            _currentDrawing.SetPosition(0, transform.position);

        }
        else
        {
            Vector3 currentPos = _currentDrawing.GetPosition(_index);
            if (Vector3.Distance(currentPos, transform.position) > 0.01f)
            {
                _index++;
                _currentDrawing.positionCount = _index + 1;
                _currentDrawing.SetPosition(_index, transform.position);
            }

        }
    }
}
