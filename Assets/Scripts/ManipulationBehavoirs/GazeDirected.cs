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

    public override void Update()
    {
        base.Update();
        VirtualHandPosition = WristPosition;
    }

    public override void ApplyIndirectGrabbedBehaviour(bool isDoubleHand = true)
    {

        VisualGainValue = Mathf.Max(1, GetVisualGain(GrabbedObject.transform.position));
        OffsetAddedByHand = PinchPosition_delta * VisualGainValue;
        GrabbedObject.transform.position += OffsetAddedByHand;

        AngleRotatedByHand = Quaternion.Angle(PinchRotation_delta * GrabbedObject.transform.rotation, GrabbedObject.transform.rotation);
        GrabbedObject.transform.rotation = PinchRotation_delta * GrabbedObject.transform.rotation;

        CurrentDistanceToGaze = Vector3.Distance(GazeOrigin, GrabbedObject.transform.position);

        if (CurrentState == StaticState.Gaze)
        {
            GrabbedObject.transform.position = GazeOrigin + GazeDirection * CurrentDistanceToGaze;

            if (IsGazeFixating) CurrentState = StaticState.Head; // switch to Head state if gaze is fixating
        }
        else
        {
            
            AngleGazeDirectionToObject = Vector3.Angle(GazeDirection, GrabbedObject.transform.position - GazeOrigin);
            if (IsGazeFixating == false && AngleGazeDirectionToObject > 15f) CurrentState = StaticState.Gaze; // 15 degrees threshold catches gaze little saccade during hand correction with distance gain
        }

        VirtualHandPosition = WristPosition;
    }


}