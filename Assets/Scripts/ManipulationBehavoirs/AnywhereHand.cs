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

    float _headPitchOnFixation;

    public override void ApplyIndirectGrabbedBehaviour()
    {

            GrabbedObject.transform.position += PinchPosition_delta * Mathf.Max(1, GetVisualGain(GrabbedObject.transform.position));
            Vector3 directionFromGazeOrigin = (GrabbedObject.transform.position - GazeOrigin).normalized;

            // transform.position += directionFromGazeOrigin * DeltaHeadY * 0.4f;
            // transform.position = GazeOrigin + directionFromGazeOrigin * Mathf.Clamp(Vector3.Distance(transform.position, GazeOrigin), 1, 10f);


            if (CurrentState == StaticState.Gaze)
            {
                GrabbedObject.transform.position = GazeOrigin + GazeDirection * Vector3.Distance(GazeOrigin, GrabbedObject.transform.position + directionFromGazeOrigin * DeltaHeadY * 0.4f);
                GrabbedObject.transform.position = GazeOrigin + GazeDirection * Mathf.Clamp(Vector3.Distance(GrabbedObject.transform.position, GazeOrigin), 1, 100f);



                if (IsGazeFixating)
                {
                    CurrentState = StaticState.Head;
                    _headPitchOnFixation = HeadData.HeadAngle_WorldY;
                }
            }
            else
            {
                
                if(HeadData.HeadAngle_WorldY - _headPitchOnFixation >= 5f)
                {
                    GrabbedObject.transform.position += directionFromGazeOrigin * (HeadData.HeadAngle_WorldY - _headPitchOnFixation - 5f) * MathFunctions.Deg2Meter(Time.deltaTime, Vector3.Distance(GrabbedObject.transform.position, GazeOrigin)) * 10;                    
                }

                if(HeadData.HeadAngle_WorldY - _headPitchOnFixation <= -3f)
                {
                    GrabbedObject.transform.position -= directionFromGazeOrigin * (-HeadData.HeadAngle_WorldY + _headPitchOnFixation - 3f) * MathFunctions.Deg2Meter(Time.deltaTime, Vector3.Distance(GrabbedObject.transform.position, GazeOrigin)) * 10;                   
                }

                GrabbedObject.transform.position = GazeOrigin + directionFromGazeOrigin * Mathf.Clamp(Vector3.Distance(GrabbedObject.transform.position, GazeOrigin), 1, 100f);

                


                if (IsGazeFixating == false) // dont swtich back during small saccades assessing the big object correction; read the object hit box information 
                {
                    CurrentState = StaticState.Gaze;
                }
            }

        // // Apply Hand Translation
        // VisualGainValue = Mathf.Max(1, GetVisualGain(GrabbedObject.transform.position));
        // OffsetAddedByHand = PinchPosition_delta * VisualGainValue;
        // GrabbedObject.transform.position += OffsetAddedByHand;

        // // // Apply Hand Rotation
        // // AngleRotatedByHand = Quaternion.Angle(PinchRotation_delta * GrabbedObject.transform.rotation, GrabbedObject.transform.rotation);
        // // GrabbedObject.transform.rotation = PinchRotation_delta * GrabbedObject.transform.rotation;

        // CurrentDistanceToGaze = Vector3.Distance(GazeOrigin, GrabbedObject.transform.position);

        // if (CurrentState == StaticState.Gaze)
        // {
        //     // Set Position along Gaze Ray
        //     GrabbedObject.transform.position = GazeOrigin + GazeDirection * CurrentDistanceToGaze;

        //     // Apply Head Depth Offset
        //     Vector3 objectDirection = (GrabbedObject.transform.position - GazeOrigin).normalized;
        //     HeadDepthOffset = GetHeadDepthOffset(objectDirection);
        //     GrabbedObject.transform.position += HeadDepthOffset;

        //     // Clamp Depth within Min and Max
        //     DistanceToGazeAfterAddingHeadDepth = Vector3.Distance(GrabbedObject.transform.position, GazeOrigin);
        //     GrabbedObject.transform.position = GazeOrigin + objectDirection * Mathf.Clamp(DistanceToGazeAfterAddingHeadDepth, MinDepth, MaxDepth);
            
        //     // Check to switch to Head state
        //     if (IsGazeFixating) CurrentState = StaticState.Head;
        // }
        // else
        // {


        //     // Check to switch back to Gaze state
        //     AngleGazeDirectionToObject = Vector3.Angle(GazeDirection, GrabbedObject.transform.position - GazeOrigin);
        //     if (IsGazeFixating == false && AngleGazeDirectionToObject > 15f) CurrentState = StaticState.Gaze; // 15 degrees threshold catches gaze little saccade during hand correction with distance gain
        // }

        VirtualHandPosition = WristPosition;
    }

    // public override void TriggerOnHandReleased()
    // {

    //     VirtualHandPosition = HandOffsetAroundObject + GrabbedObject.transform.position;
    //     base.TriggerOnHandReleased();
    // }


    #region HeadDepth EdgeGain

    public virtual Vector3 GetHeadDepthOffset(Vector3 objectDirection)
    {
        // float max_gain = (MaxDepth - MinDepth) / MaxGainDeg;
        float max_gain = 0.8f;

        // float min_gain = (MaxDepth - MinDepth) / MinGainDeg;
        float min_gain = 0;


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

    public float GetVisualGain(Vector3 objectPosition)
    {
        return Mathf.Max(1f, Vector3.Distance(objectPosition, GazeOrigin) / Vector3.Distance(PinchPosition, GazeOrigin));
    }

    #endregion
}