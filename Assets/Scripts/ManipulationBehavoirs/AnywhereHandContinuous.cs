using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum StaticState
{
    Gaze, Head, Hand
}

public class AnywhereHandContinuous : ManipulationTechnique
{
    public TextMeshPro text;
    public StaticState CurrentState { get; private set; }

    [Header("Threshold Settings")]
    public float HandTranslationSpeedThreshold = 0.3f; // in meters per second
    public float HandRotationSpeedThreshold = 20f; // in degrees per second
    public float HeadSpeedThreshold = 0.1f; // in meters per second

    public bool HeadFixatedAfterGazeShift;
    public float HeadDepth { get; private set; }
    public override void ApplySingleHandGrabbedBehaviour()
    {

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("EyeInHeadYAngle: " + EyeInHeadYAngle.ToString("F2") + ", HeadYAngle: " + HeadYAngle.ToString("F2"));
        }


        bool isGazeMoving = IsGazeSaccading;
        bool isHAndMoving = HandTranslationSpeed >= HandTranslationSpeedThreshold || HandRotationSpeed >= HandRotationSpeedThreshold;
        bool isHEadMoving = HeadSpeed >= HeadSpeedThreshold;

        bool allowDetectHeadFixation = true;

        // if (isHAndMoving)
        // {
        //         GrabbedObject.position += HandPosition_delta * GetVisualGain();
        //         GrabbedObject.rotation = HandRotation_delta * GrabbedObject.rotation;
        //         text.text = "Hand";

        //         HeadFixationTracker.ResetFixationBuffer();
        //         // allowDetectHeadFixation = false;
        // }
        // else
        // {

        // }

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
        }
        else
        {
            
            Vector3 handOffset = HandPosition_delta * GetVisualGain();
            Vector3 objDirection = (GrabbedObject.position - GazeOrigin).normalized;

            Vector3 handOffset_alongObjDirection = Vector3.Project(handOffset, objDirection);
            Vector3 handOffset_perpendicular = handOffset - handOffset_alongObjDirection;


            // when gaze is fixating, use head to adjust the depth
            // text.text = "Head";
            float depthGain = (10f - 1f) / (AvailableHeadY_Up - AvailableHeadY_Down);
            Vector3 headDepthOffset = objDirection * depthGain * DeltaHeadY;

            bool isHeadHandInSync = Vector3.Dot(handOffset, headDepthOffset) > 0;
            
            GrabbedObject.position += handOffset;

            if (isHeadHandInSync)
            {
                GrabbedObject.position += headDepthOffset - handOffset_alongObjDirection;
            }

            float distance = Vector3.Distance(GrabbedObject.position, GazeOrigin);
            GrabbedObject.position = GazeOrigin + objDirection * Mathf.Clamp(distance, 1f, 10f);           


            // GrabbedObject.position += HandPosition_delta * GetVisualGain();
            GrabbedObject.rotation = HandRotation_delta * GrabbedObject.rotation;
        }

        // head hand moves together in the same direction for depth adjustment -> head > hand -> hand is moving but egnored
        // when head finishes depth adjustment at extreme angles, it will naturally moves back to the comfortable angle after the hand taks over the control -> hand > head -> head is movign but ignored

        text.transform.LookAt(Camera.main.transform);
        text.transform.Rotate(0, 180f, 0);

    }

}