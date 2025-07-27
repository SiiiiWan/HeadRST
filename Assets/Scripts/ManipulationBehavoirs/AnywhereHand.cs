using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class AnywhereHand : ManipulationTechnique
{
    public float MaxHeadDepth { get; protected set; } = 10f;
    public float MinHeadDepth { get; protected set; } = 1f;

    public Vector3 AccumulatedHandOffsetAroundObject { get; private set; }
    public Vector3 VirtualHandPosition_Indirect_Update { get; private set; }

    public override void ApplyObjectFreeBehaviour()
    {
        VirtualHandPosition += WristPosition_delta;

        if (IsGazeFixating == false)
        {
            float distance = Vector3.Distance(GazeOrigin, VirtualHandPosition);
            VirtualHandPosition = GazeOrigin + GazeDirection * distance;
        }
        else
        {
            Vector3 objectDirection = (VirtualHandPosition - GazeOrigin).normalized;
            VirtualHandPosition += GetHeadDepthOffset(objectDirection);
            VirtualHandPosition = GazeOrigin + objectDirection * Mathf.Clamp(Vector3.Distance(VirtualHandPosition, GazeOrigin), MinHeadDepth, MaxHeadDepth);
        }
    }

    public override void TriggerOnLookAtNewObjectBehavior()
    {
        VirtualHandPosition = GazingObject.transform.position + (WristPosition - PinchPosition);
        AccumulatedHandOffsetAroundObject = Vector3.zero;
    }

    public override void ApplyGazingButNotGrabbingBehaviour()
    {
        AccumulatedHandOffsetAroundObject += WristPosition_delta * Vector3.Distance(GazingObject.transform.position, GazeOrigin);
        VirtualHandPosition = GazingObject.transform.position + AccumulatedHandOffsetAroundObject + (WristPosition - PinchPosition);
    }

    public override void ApplyDirectGrabbedBehaviour()
    {
        ApplyObjectFreeBehaviour();
    }

    public override void ApplyIndirectGrabbedBehaviour()
    {

        GrabbedObject.transform.position += PinchPosition_delta * Vector3.Distance(GrabbedObject.transform.position, GazeOrigin);
        GrabbedObject.transform.rotation = PinchRotation_delta * GrabbedObject.transform.rotation;

        if (IsGazeFixating == false)
        {
            float distance = Vector3.Distance(GazeOrigin, GrabbedObject.transform.position);
            GrabbedObject.transform.position = GazeOrigin + GazeDirection * distance;
        }
        else
        {
            Vector3 objectDirection = (GrabbedObject.transform.position - GazeOrigin).normalized;
            GrabbedObject.transform.position += GetHeadDepthOffset(objectDirection);
            GrabbedObject.transform.position = GazeOrigin + objectDirection * Mathf.Clamp(Vector3.Distance(GrabbedObject.transform.position, GazeOrigin), MinHeadDepth, MaxHeadDepth);
        }

        VirtualHandPosition = Vector3.zero;
        VirtualHandPosition_Indirect_Update = GrabbedObject.transform.position + AccumulatedHandOffsetAroundObject + (WristPosition - PinchPosition);
    }

    public override void TriggerOnHandReleased()
    {
        if (GrabbedObject.GrabbedState == GrabbedState.Grabbed_Indirect)
        {
            VirtualHandPosition = VirtualHandPosition_Indirect_Update;
            if (Log) print("ah: Update VirtualHandPosition to Relative Position");
        }

        base.TriggerOnHandReleased();
    }


#region HeadDepth EdgeGain
    public float MinHeadSpeed { get; protected set; } = 0.2f;
    public float MaxHeadSpeed { get; protected set; } = 0.6f;

    public float MinGainDeg { get; protected set; } = 30;
    public float MaxGainDeg { get; protected set; } = 10;
    public float BaseGain { get; protected set; }
    public float EdgeGain { get; protected set; }
    public float Attenuation { get; protected set; } = 1;



    Vector3 GetHeadDepthOffset(Vector3 objectDirection)
    {
        float max_gain = (MaxHeadDepth - MinHeadDepth) / MaxGainDeg;
        float min_gain = (MaxHeadDepth - MinHeadDepth) / MinGainDeg;

        BaseGain = VitLerp(Math.Abs(HeadSpeed), min_gain, max_gain, MinHeadSpeed, MaxHeadSpeed);
        EdgeGain = EyeHeadGain();
        Vector3 headDepthOffset = objectDirection * DeltaHeadY * BaseGain * EdgeGain;

        Attenuation = HeadAttenuation(headDepthOffset);

        // return headDepthOffset * Attenuation;
        return headDepthOffset;
    }

    float HeadAttenuation(Vector3 headDepthOffset)
    {
        float attenuation = 1;

        if (Filtered_EyeInHeadAngle < Filtered_EyeInHeadAngle_Pre && Vector3.Dot(headDepthOffset, Filtered_HandMovementVector) < 0) // eye in head angle is decreasing
        {
            attenuation = Vector3.Project(Filtered_HandMovementVector, headDepthOffset).magnitude / headDepthOffset.magnitude;
        }

        return Mathf.Clamp(attenuation, 0, 1);
    }

    float EyeHeadGain()
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


    protected float linearDepthFunction_TwoPoints(float x, Vector2 left, Vector2 right)
    {
        float k = (right.y - left.y) / (right.x - left.x);

        float b = right.y - k * right.x;

        return k * x + b;
    }
#endregion
}