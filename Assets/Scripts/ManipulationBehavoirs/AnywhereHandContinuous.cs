using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;



public class AnywhereHandContinuous : ManipulationTechnique
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

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("EyeInHeadYAngle: " + EyeInHeadYAngle.ToString("F2") + ", HeadYAngle: " + HeadYAngle.ToString("F2"));
        }


        bool isGazeMoving = IsGazeSaccading;
        bool isHAndMoving = HandTranslationSpeed >= HandTranslationSpeedThreshold || HandRotationSpeed >= HandRotationSpeedThreshold;
        bool isHEadMoving = HeadSpeed >= HeadSpeedThreshold;

        // print("Hand Rotation Speed: " + HandRotationSpeed.ToString("F2"));

        if (CurrentState == StaticState.EyeHead)
        {

            text.text = "EyeHead: " + ReadyToSwitchToHand.ToString();

            if (ReadyToSwitchToHand && isHAndMoving)
            {
                // Vector3 objDirection = (GrabbedObject.position - GazeOrigin).normalized;

                // print("Hand movement angle: " + Vector3.Angle(HandPosition_delta, objDirection));
                // if (Vector3.Angle(HandFixationTracker.GetFixationDirCentroid(), objDirection) >= 45f)
                // {
                //     CurrentState = StaticState.Hand;
                //     ReadyToSwitchToHand = false;
                // }

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
                ReadyToSwitchToHand = false;
            }
        }

        if (CurrentState == StaticState.EyeHead)
        {
            if (IsGazeFixating == false)
            {
                //  when head movement is involved in saccade, update the depth to the optimized position.
                //  if is just gaze saccade without head movement, then the depth is not updated.
                if (isHEadMoving)
                {
                    UpdateHeadDepthAnchor();
                    HeadDepth = Mathf.Lerp(1f, 10f, (HeadYAngle - AvailableHeadY_Down) / (AvailableHeadY_Up - AvailableHeadY_Down));
                }

                // text.text = "Gaze";
                GrabbedObject.position = GazeOrigin + GazeDirection * Mathf.Clamp(HeadDepth, 1f, 10f);
                ReadyToSwitchToHand = false;
            }
            else if (IsGazeFixating == true)
            {
                Vector3 objDirection = (GrabbedObject.position - GazeOrigin).normalized;
                float depthGain = (10f - 1f) / (AvailableHeadY_Up - AvailableHeadY_Down);
                Vector3 headDepthOffset = objDirection * depthGain * DeltaHeadY;
                GrabbedObject.position += headDepthOffset;

                float distance = Vector3.Distance(GrabbedObject.position, GazeOrigin);
                GrabbedObject.position = GazeOrigin + objDirection * Mathf.Clamp(distance, 1f, 10f);

                if (isHAndMoving == false && IsHandStablized) ReadyToSwitchToHand = true;
            }
        }
        else if (CurrentState == StaticState.Hand)
        {
            GrabbedObject.position += HandPosition_delta * GetVisualGain();
            GrabbedObject.rotation = HandRotation_delta * GrabbedObject.rotation;
        }


        // head hand moves together in the same direction for depth adjustment -> head > hand -> hand is moving but egnored
        // when head finishes depth adjustment at extreme angles, it will naturally moves back to the comfortable angle after the hand taks over the control -> hand > head -> head is movign but ignored

        text.transform.LookAt(Camera.main.transform);
        text.transform.Rotate(0, 180f, 0);

    }

}