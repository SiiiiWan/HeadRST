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
        bool isHandMoving = HandTranslationSpeed >= HandTranslationSpeedThreshold || HandRotationSpeed >= HandRotationSpeedThreshold;
        bool isHeadMoving = HeadSpeed >= HeadSpeedThreshold;

        bool allowDetectHeadFixation = true;

        if (IsGazeFixating == false)
        {
            UpdateHeadDepthAnchor();

            HeadDepth = Mathf.Lerp(1f, 10f, (HeadYAngle - AvailableHeadY_Down) / (AvailableHeadY_Up - AvailableHeadY_Down));
            HeadDepth = Mathf.Clamp(HeadDepth, 1f, 10f);

            // text.text = ((HeadYAngle - AvailableHeadY_Down) / (AvailableHeadY_Up - AvailableHeadY_Down)).ToString("F2");
            text.text = "Gaze";
            GrabbedObject.position = GazeOrigin + GazeDirection * HeadDepth;

            HeadFixationTracker.ResetFixationBuffer();
            allowDetectHeadFixation = false;
        }

        if (IsGazeFixating & isHandMoving)
        {
            GrabbedObject.position += HandPosition_delta * GetVisualGain();
            GrabbedObject.rotation = HandRotation_delta * GrabbedObject.rotation;
            text.text = "Hand";

            HeadFixationTracker.ResetFixationBuffer();
            allowDetectHeadFixation = false;
        }

        if (IsGazeFixating & isHeadMoving & (allowDetectHeadFixation & IsHeadFixating) & !isHandMoving)
        {
            HeadDepth = Mathf.Lerp(1f, 10f, (HeadYAngle - AvailableHeadY_Down) / (AvailableHeadY_Up - AvailableHeadY_Down));
            HeadDepth = Mathf.Clamp(HeadDepth, 1f, 10f);

            text.text = "Head";
            GrabbedObject.position = GazeOrigin + GazeDirection_OnGazeFixation * HeadDepth;
        }



        text.transform.LookAt(Camera.main.transform);
        text.transform.Rotate(0, 180f, 0); // Optional: flip to face the camera properly

    }

}