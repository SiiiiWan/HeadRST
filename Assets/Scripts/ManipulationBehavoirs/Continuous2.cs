using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;



public class Continuous2 : ManipulationTechnique
{
    public TextMeshPro text;
    public StaticState CurrentState { get; private set; }

    [Header("Threshold Settings")]
    public float HandTranslationSpeedThreshold = 0.3f; // in meters per second
    public float HandRotationSpeedThreshold = 20f; // in degrees per second
    public float HeadSpeedThreshold = 0.1f; // in meters per second

    public bool ReadyToSwitchToHand;
    public float HeadDepth { get; private set; }

    public override void OnSingleHandGrabbed(Transform obj)
    {
        base.OnSingleHandGrabbed(obj);

        CurrentState = StaticState.Hand;
    }

    public override void ApplySingleHandGrabbedBehaviour()
    {

        bool isGazeMoving = IsGazeSaccading;
        bool isHAndMoving = HandTranslationSpeed >= HandTranslationSpeedThreshold || HandRotationSpeed >= HandRotationSpeedThreshold;
        bool isHEadMoving = HeadSpeed >= HeadSpeedThreshold;

        if (CurrentState == StaticState.EyeHead)
        {
            text.text = "EyeHead: " + ReadyToSwitchToHand.ToString();

            if (ReadyToSwitchToHand && isHAndMoving)
            {
                CurrentState = StaticState.Hand;
                ReadyToSwitchToHand = false;
            }
        }
        else if (CurrentState == StaticState.Hand)
        {
            text.text = "Hand";
            if (IsGazeFixating == false && Vector3.Angle(GazeDirection, GrabbedObject.position - GazeOrigin) > 10f) CurrentState = StaticState.EyeHead;
            
            if (IsHandStablized && IsHeadFixating == false)
            {
                // if hand is not moving and head is not fixating, switch to eye-head state
                CurrentState = StaticState.EyeHead;
                // ReadyToSwitchToHand = false;
            }
        }

        if (CurrentState == StaticState.EyeHead)
        {
            UpdateHeadDepthAnchor();
            if (IsGazeFixating == false)
            {
                float distance = Vector3.Distance(GrabbedObject.position, GazeOrigin);
                GrabbedObject.position = GazeOrigin + GazeDirection * distance;
                ReadyToSwitchToHand = false;
            }
            else if (IsGazeFixating == true)
            {
                Vector3 objDirection = (GrabbedObject.position - GazeOrigin).normalized;
                float depthGain = (10f - 1f) / (Limit_HeadY_Up - Limit_HeadY_Down);

                Vector3 headDepthOffset = objDirection * depthGain * DeltaHeadY;
                GrabbedObject.position += headDepthOffset;

                float distance = Vector3.Distance(GrabbedObject.position, GazeOrigin);
                GrabbedObject.position = GazeOrigin + objDirection * Mathf.Clamp(distance, 1f, 10f);

                print(distance.ToString("F2"));


                if (isHAndMoving == false && IsHandStablized) ReadyToSwitchToHand = true;
            }
        }
        else if (CurrentState == StaticState.Hand)
        {
            GrabbedObject.position += PinchPosition_delta * GetVisualGain();
            GrabbedObject.rotation = PinchRotation_delta * GrabbedObject.rotation;
        }


        // head hand moves together in the same direction for depth adjustment -> head > hand -> hand is moving but egnored
        // when head finishes depth adjustment at extreme angles, it will naturally moves back to the comfortable angle after the hand taks over the control -> hand > head -> head is movign but ignored

        text.transform.LookAt(Camera.main.transform);
        text.transform.Rotate(0, 180f, 0);

    }

}