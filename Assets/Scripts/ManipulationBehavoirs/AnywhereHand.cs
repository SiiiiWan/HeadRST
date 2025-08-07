using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework.Constraints;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public enum StaticState
{
    Gaze,
    Head
}

public class AnywhereHand : ManipulationTechnique
{
    public StaticState CurrentState { get; protected set; } = StaticState.Gaze;


    public override void Update()
    {
        base.Update();
        VirtualHandPosition = WristPosition;
    }


    // public Vector3 HandOffsetAroundObject { get; private set; }
    // public bool CloseDirectGrabbed { get; private set; }


    // public override void ApplyObjectFreeBehaviour()
    // {
    //     VirtualHandPosition = WristPosition;
    // }

    // public override void TriggerOnLookAtNewObjectBehavior()
    // {
    //     float distance = Vector3.Distance(GazeOrigin, GazingObject.transform.position);

    //     if (distance > 1f)
    //     {
    //         VirtualHandPosition = GazingObject.transform.position + (WristPosition - PinchPosition);      
    //     }

    // }

    // public override void ApplyGazingButNotGrabbingBehaviour()
    // {
    //     float distance = Vector3.Distance(GazeOrigin, GazingObject.transform.position);

    //     if (distance > 1f) VirtualHandPosition += WristPosition_delta * Vector3.Distance(GazingObject.transform.position, GazeOrigin);
    //     else VirtualHandPosition = WristPosition;
    // }

    // public override void TriggerOnSingleHandGrabbed(ManipulatableObject obj, GrabbedState grabbedState)
    // {
    //     base.TriggerOnSingleHandGrabbed(obj, grabbedState);

    //     CurrentState = StaticState.Head;
    //     HandOffsetAroundObject = VirtualHandPosition - obj.transform.position;

    //     if(grabbedState == GrabbedState.Grabbed_Direct) CloseDirectGrabbed = Vector3.Distance(GazeOrigin, GazingObject.transform.position) < 1f;
        
    // }

    // public override void ApplyDirectGrabbedBehaviour()
    // {
    //     VirtualHandPosition += WristPosition_delta;

    //     if (CloseDirectGrabbed) return;

    //     if (CurrentState == StaticState.Gaze)
    //     {
    //         float distance = Vector3.Distance(GazeOrigin, VirtualHandPosition);
    //         VirtualHandPosition = GazeOrigin + GazeDirection * distance;

    //         if (IsGazeFixating) CurrentState = StaticState.Head; // switch to Head state if gaze is fixating
    //     }
    //     else
    //     {
    //         Vector3 objectDirection = (VirtualHandPosition - GazeOrigin).normalized;
    //         VirtualHandPosition += GetHeadDepthOffset(objectDirection);
    //         VirtualHandPosition = GazeOrigin + objectDirection * Mathf.Clamp(Vector3.Distance(VirtualHandPosition, GazeOrigin), MinDepth, MaxDepth);

    //         if (IsGazeFixating == false && Vector3.Angle(GazeDirection, VirtualHandPosition - GazeOrigin) > 5f) CurrentState = StaticState.Gaze; // 5 degrees threshold catches gaze little saccade during hand correction with distance gain
    //     }

    // }

    public override void ApplyIndirectGrabbedBehaviour()
    {

        GrabbedObject.transform.position += PinchPosition_delta * Mathf.Max(1, Vector3.Distance(GrabbedObject.transform.position, GazeOrigin));
        GrabbedObject.transform.rotation = PinchRotation_delta * GrabbedObject.transform.rotation;

        if (CurrentState == StaticState.Gaze)
        {
            float distance = Vector3.Distance(GazeOrigin, GrabbedObject.transform.position);
            GrabbedObject.transform.position = GazeOrigin + GazeDirection * distance;

            if (IsGazeFixating) CurrentState = StaticState.Head; // switch to Head state if gaze is fixating
        }
        else
        {
            Vector3 objectDirection = (GrabbedObject.transform.position - GazeOrigin).normalized;
            GrabbedObject.transform.position += GetHeadDepthOffset(objectDirection);
            GrabbedObject.transform.position = GazeOrigin + objectDirection * Mathf.Clamp(Vector3.Distance(GrabbedObject.transform.position, GazeOrigin), MinDepth, MaxDepth);

            if (IsGazeFixating == false && Vector3.Angle(GazeDirection, GrabbedObject.transform.position - GazeOrigin) > 15f) CurrentState = StaticState.Gaze; // 15 degrees threshold catches gaze little saccade during hand correction with distance gain
        }

        VirtualHandPosition = WristPosition;
    }

    // public override void TriggerOnHandReleased()
    // {

    //     VirtualHandPosition = HandOffsetAroundObject + GrabbedObject.transform.position;
    //     base.TriggerOnHandReleased();
    // }


    #region HeadDepth EdgeGain
    public float MinHeadSpeed { get; protected set; } = 0.2f;
    public float MaxHeadSpeed { get; protected set; } = 0.6f;

    public float MinGainDeg { get; protected set; } = 30;
    public float MaxGainDeg { get; protected set; } = 10;
    public float BaseGain { get; protected set; }
    public float EdgeGain { get; protected set; }


    public virtual Vector3 GetHeadDepthOffset(Vector3 objectDirection)
    {
        // float max_gain = (MaxDepth - MinDepth) / MaxGainDeg;
        float max_gain = 0.8f;

        // float min_gain = (MaxDepth - MinDepth) / MinGainDeg;
        float min_gain = 0.4f/3;


        BaseGain = VitLerp(Math.Abs(HeadSpeed), min_gain, max_gain, MinHeadSpeed, MaxHeadSpeed);
        EdgeGain = EyeHeadGain();
        Vector3 headDepthOffset = objectDirection * DeltaHeadY * BaseGain * EdgeGain;


        return headDepthOffset;
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


    protected float linearDepthFunction_TwoPoints(float x, Vector2 left, Vector2 right)
    {
        float k = (right.y - left.y) / (right.x - left.x);

        float b = right.y - k * right.x;

        return k * x + b;
    }
    #endregion
}