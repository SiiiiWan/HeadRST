using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework.Constraints;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class GazeDirected : ManipulationTechnique
{
    public StaticState CurrentState { get; protected set; } = StaticState.Gaze;


    public override void Update()
    {
        base.Update();
        VirtualHandPosition = WristPosition;
    }


    public override void ApplyIndirectGrabbedBehaviour()
    {

        // GrabbedObject.transform.position += PinchPosition_delta * Mathf.Max(1, Vector3.Distance(GrabbedObject.transform.position, GazeOrigin));
        GrabbedObject.transform.position += PinchPosition_delta * Mathf.Max(1, GetVisualGain(GrabbedObject.transform.position));

        GrabbedObject.transform.rotation = PinchRotation_delta * GrabbedObject.transform.rotation;

        if (CurrentState == StaticState.Gaze)
        {
            float distance = Vector3.Distance(GazeOrigin, GrabbedObject.transform.position);
            GrabbedObject.transform.position = GazeOrigin + GazeDirection * distance;

            if (IsGazeFixating) CurrentState = StaticState.Head; // switch to Head state if gaze is fixating
        }
        else
        {

            if (IsGazeFixating == false && Vector3.Angle(GazeDirection, GrabbedObject.transform.position - GazeOrigin) > 15f) CurrentState = StaticState.Gaze; // 15 degrees threshold catches gaze little saccade during hand correction with distance gain
        }

        VirtualHandPosition = WristPosition;
    }


    public float GetVisualGain(Vector3 objectPosition)
    {
        return Mathf.Max(1f, Vector3.Distance(objectPosition, GazeOrigin) / Vector3.Distance(PinchPosition, GazeOrigin));
    }
}