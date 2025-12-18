using System;
using UnityEngine;

public class AH_Ref_Frame_Global : MonoBehaviour
{
    private AH_ControlMode _currentMode = AH_ControlMode.Gaze;
    public bool IsUpdatingFrame = false;
    public bool IsUpdatingFrame_LastFrame = false;
    public float stabilityTHreshold = 0.1f;


    public EyeGaze GazeData { get; private set; }
    public HandData HandData { get; private set; }
    public HeadMovement HeadData { get; private set; }
    public PinchDetector PinchDetector { get; private set; }
    private GameObject _previewObject;
    private Quaternion _previewRotationOffset;

    void Update()
    {
        GazeData = EyeGaze.GetInstance();
        HeadData = HeadMovement.GetInstance();
        HandData = HandData.GetInstance();
        PinchDetector = PinchDetector.GetInstance();

        Vector3 gazeOrigin = GazeData.GetGazeOrigin();
        Vector3 gazeDirection = GazeData.GetGazeDirection();

        IsUpdatingFrame_LastFrame = IsUpdatingFrame;

        if (PinchDetector.GetInstance().IsOneHandPinching && PinchDetector.GetInstance().IsNoHandPinching_LastFrame)
        {
            IsUpdatingFrame = false;
            IsUpdatingFrame_LastFrame = false;
            transform.position = ObjectManager.GetInstance().PickedUpObject.transform.position;

            return;
        }

        if (IsUpdatingFrame)
        {
            transform.forward = gazeDirection;
            
            if(_previewObject != null)
            {
                _previewObject.transform.position = transform.position;
                _previewObject.transform.rotation = transform.rotation * _previewRotationOffset;
            }

            if (_currentMode == AH_ControlMode.Gaze)
            {
                DepthTranslation(gazeDirection);
                transform.position = gazeOrigin + gazeDirection * Vector3.Distance(gazeOrigin, GazeData.GetGazeHitPoint(out Vector3 hitPoint) ? hitPoint : transform.position);

                if (GazeData.IsFixating_DT())
                {
                    _currentMode = AH_ControlMode.Head;
                    // HeadData.ResetHeadStabilityHistory();
                }
            }
            else
            {
                // DepthTranslation((transform.position - gazeOrigin).normalized);

                if (GazeData.IsFixating_DT() == false)
                {
                    _currentMode = AH_ControlMode.Gaze;
                }
                
                // switch to lock frame mode
                if (_currentMode == AH_ControlMode.Head && HandData.IsHandActivelyMoving() == true)
                {
                    IsUpdatingFrame = false;

                    Destroy(_previewObject);
                }
            }
        }
        else
        {
            // switch to shifting frame mode
            if (GazeData.IsFixating_DT() == false)
            {
                _currentMode = AH_ControlMode.Gaze;
                IsUpdatingFrame = true;

                // For object Manipulation
                if(ObjectManager.GetInstance().PickedUpObject != null)
                {
                    _previewObject = Instantiate(ObjectManager.GetInstance().PickedUpObject.gameObject);
                    _previewObject.GetComponent<Collider>().enabled = false;
                    _previewObject.GetComponent<Rigidbody>().isKinematic = true;
                    _previewObject.GetComponent<Outline>().enabled = true;
                    _previewRotationOffset = Quaternion.Inverse(transform.rotation) * _previewObject.transform.rotation;                    
                }

            }
        }

        // GetComponent<Renderer>().enabled = IsUpdatingFrame;
        // && PinchDetector.GetInstance().IsOneHandPinching
    }

    void DepthTranslation(Vector3 axis)
    {
        transform.position += axis * HeadData.DeltaHeadY * VitLerp(Math.Abs(HeadData.HeadSpeed), 0, 0.8f, 0.1f, 0.6f) * EdgeGain();
        transform.position = GazeData.GetGazeOrigin() + axis * Mathf.Clamp(Vector3.Distance(GazeData.GetGazeOrigin(), transform.position), 1, 11);        
    }

    public float EdgeGain()
    {
        float eyeRange = GetEyeRange(GazeData.EyeInHeadXAngle, GazeData.EyeInHeadYAngle);
        float k = 3;
        float boostStartDeg = eyeRange / k;

        float gain = 1;

        float gazeAngleFromHead = Vector3.Angle(GazeData.GetGazeDirection(), HeadData.HeadForward);

        if (gazeAngleFromHead >= boostStartDeg & GazeData.FilteredEyeInHeadAngle > GazeData.FilteredEyeInHeadAngle_Pre) gain = linearDepthFunction_TwoPoints(gazeAngleFromHead, new Vector2(boostStartDeg, 1), new Vector2(eyeRange, k));

        return gain;
    }

    float GetEyeRange(float x, float y, float up_lim = 15, float down_lim = 30, float side_lim = 30)
    {
        if (y >= 0) return (1 - (1 - (up_lim / side_lim)) * Mathf.Sin(Mathf.Atan2(y, x))) * side_lim;
        else return (1 + (1 - (down_lim / side_lim)) * Mathf.Sin(Mathf.Atan2(y, x))) * side_lim;
    }

    protected float linearDepthFunction_TwoPoints(float x, Vector2 left, Vector2 right)
    {
        float k = (right.y - left.y) / (right.x - left.x);

        float b = right.y - k * right.x;

        return k * x + b;
    }

    public float VitLerp(float x, float k1 = 0.8f / 3f, float k2 = 0.8f, float v1 = 0.2f, float v2 = 0.6f)
    {
        if (x <= v1)
            return k1;

        if (x >= v2)
            return k2;

        return k1 + (k2 - k1) / (v2 - v1) * (x - v1);
    }
}

