using System;
using UnityEngine;

public class AH_CHI : PositionRotationProvider
{
    private AH_ControlMode _currentMode = AH_ControlMode.Gaze;

    public override Vector3 GetPositionOutput(Vector3 currentPosition)
    {
        UpdateDataSource();

        Vector3 gazeOrigin = GazeData.GetGazeOrigin();
        Vector3 gazeDirection = GazeData.GetGazeDirection();

        currentPosition += HandData.GetDeltaHandPosition() * GetVisualGain(currentPosition);
        Vector3 directionFromGazeOrigin = (currentPosition - gazeOrigin).normalized;

        if (_currentMode == AH_ControlMode.Gaze)
        {
            currentPosition = gazeOrigin + gazeDirection * Vector3.Distance(gazeOrigin, currentPosition);

            if (GazeData.IsFixating_DT())
            {
                _currentMode = AH_ControlMode.Head;
            }
        }
        else
        {
            currentPosition += directionFromGazeOrigin * HeadData.DeltaHeadY * VitLerp(Math.Abs(HeadData.HeadSpeed), 0, 0.8f, 0.1f, 0.6f) * EdgeGain();
            currentPosition = gazeOrigin + directionFromGazeOrigin * Mathf.Clamp(Vector3.Distance(gazeOrigin, currentPosition), 1, 11);

            if (GazeData.IsFixating_DT() == false &&  Vector3.Angle(gazeDirection, currentPosition - gazeOrigin) > 15f)
            {
                _currentMode = AH_ControlMode.Gaze;
            }
        }

        return currentPosition;
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

