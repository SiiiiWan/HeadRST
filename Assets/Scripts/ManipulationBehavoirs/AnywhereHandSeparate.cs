using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AnywhereHandSeparate : ManipulationTechnique
{
    public TextMeshPro text;
    public StaticState CurrentState { get; private set; }

    [Header("Threshold Settings")]
    public float HandTranslationSpeedThreshold = 0.3f; // in meters per second
    public float HandRotationSpeedThreshold = 20f; // in degrees per second
    public float HeadSpeedThreshold = 0.1f; // in meters per second

    public bool HeadFixatedAfterGazeShift;
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


        if(isHAndMoving == false && isHEadMoving)
        {
            CurrentState = StaticState.EyeHead;
        }

        if (isHAndMoving && isHEadMoving == false)
        {
            CurrentState = StaticState.Hand;
        }

        if (IsGazeFixating == false)
            {
                //  when head movement is involved in saccade, update the depth to the optimized position.
                //  if is just gaze saccade without head movement, then the depth is not updated.
                if (isHEadMoving)
                {
                    UpdateHeadDepthAnchor();
                    HeadDepth = Mathf.Lerp(1f, 10f, (HeadYAngle - Limit_HeadY_Down) / (Limit_HeadY_Up - Limit_HeadY_Down));
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



                float depthGain = (10f - 1f) / (Limit_HeadY_Up - Limit_HeadY_Down);
                Vector3 headDepthOffset = objDirection * depthGain * DeltaHeadY;

                if (CurrentState == StaticState.Hand)
                {
                    GrabbedObject.position += handOffset_alongObjDirection;
                }
                else if (CurrentState == StaticState.EyeHead)
                {
                    GrabbedObject.position += headDepthOffset;
                }

                float distance = Vector3.Distance(GrabbedObject.position, GazeOrigin);
                GrabbedObject.position = GazeOrigin + objDirection * Mathf.Clamp(distance, 1f, 10f);

                GrabbedObject.position += handOffset_perpendicular;

                GrabbedObject.rotation = HandRotation_delta * GrabbedObject.rotation;
            }

        // head hand moves together in the same direction for depth adjustment -> head > hand -> hand is moving but egnored
        // when head finishes depth adjustment at extreme angles, it will naturally moves back to the comfortable angle after the hand taks over the control -> hand > head -> head is movign but ignored

        text.transform.LookAt(Camera.main.transform);
        text.transform.Rotate(0, 180f, 0);

    }

}