using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AnywhereHandStatic : AnywhereHand
{
    // public bool HeadAttenuation;
    // public float k; // k > 0
    // public StaticState CurrentState = StaticState.Hand;
    // public TextMeshPro text;

    // public override void TriggerOnSingleHandGrabbed(Transform obj, GrabbedState grabbedState)
    // {
    //     base.TriggerOnSingleHandGrabbed(obj, grabbedState);

    //     CurrentState = StaticState.Hand;
    //     text.text = "grabbed";
    // }

    // public override void TriggerOnHandReleased(GrabbedState grabbedState)
    // {
    //     base.TriggerOnHandReleased(grabbedState);

    //     text.text = "Released";
    // }

    // public override void ApplyIndirectGrabbedBehaviour()
    // {
    //     base.ApplyIndirectGrabbedBehaviour();

    //     if (IsGazeFixating == false)
    //     {

    //         CurrentState = StaticState.Gaze;
    //     }
    //     else CurrentState = StaticState.Head;

    //     GrabbedObject.position += PinchPosition_delta * Vector3.Distance(GrabbedObject.position, GazeOrigin);
    //     GrabbedObject.rotation = PinchRotation_delta * GrabbedObject.rotation;

    //     if (CurrentState == StaticState.Gaze)
    //     {
    //         float distance = Vector3.Distance(GazeOrigin, GrabbedObject.position);
    //         GrabbedObject.position = GazeOrigin + GazeDirection * distance;
    //     }
    //     else
    //     {
    //         Vector3 objectDirection = (GrabbedObject.position - GazeOrigin).normalized;
    //         GrabbedObject.position += objectDirection * GetHeadDepthOffset();
    //         GrabbedObject.position = GazeOrigin + objectDirection * Mathf.Clamp(Vector3.Distance(GrabbedObject.position, GazeOrigin), 1, 10);
    //     }
    // }
    

    // private Vector3 _accumulatedHandOffset = Vector3.zero;
    // private ManipulatableObject _closestAnchor;   
    // private Vector3 _anchorPosition; 

    // public override Vector3 UpdateVirtualHandPosition()
    // {
    //     text.transform.LookAt(Camera.main.transform);
    //     text.transform.Rotate(0, 180f, 0);

    //     if (GrabbedObject)
    //     {
    //         if (GrabbedObject.GetComponent<ManipulatableObject>().GrabbedState == GrabbedState.Grabbed_Indirect)
    //         {

    //             _anchorPosition = GrabbedObject.position + (WristPosition - PinchPosition);

    //             _accumulatedHandOffset = VirtualHandPosition - _anchorPosition;

    //             return Vector3.zero;
    //         }
    //     }

    //     Vector3 nextVirtualHandPosition = VirtualHandPosition;

    //     UpdateAndSortObjectInGazeConeList();
    //     if (ObjectsInGazeCone.Count > 0)
    //     {
    //         if (ObjectsInGazeCone[0].IsHand)
    //         {
    //             print("Set Hand To Wrist Position");
    //             return WristPosition;
    //         }
    //         else
    //         {
    //             if (_closestAnchor != ObjectsInGazeCone[0])
    //             {
    //                 _accumulatedHandOffset = Vector3.zero;

    //                 _closestAnchor = ObjectsInGazeCone[0];

    //                 _anchorPosition = _closestAnchor.transform.position + (WristPosition - PinchPosition);
    //             }


    //             _accumulatedHandOffset += WristPosition_delta * Vector3.Distance(_closestAnchor.transform.position, GazeOrigin);
    //             nextVirtualHandPosition = _anchorPosition + _accumulatedHandOffset;

    //             //TODO: keep the relative position of the virtual hand to object when start and end the indirect grab
    //             // if (closestAnchor.IsRealHand) VirtualHandPosition = WristPosition;
    //             // else
    //             // {

    //             // }
    //             // print("Update VirtualHandPosition 1");                
    //         }

    //     }
    //     else
    //     {
    //         // _accumulatedHandOffset = Vector3.zero;
    //         _closestAnchor = null;
    //         nextVirtualHandPosition = GetNewAnywhereHandPosition(VirtualHandPosition);
    //         // print("Update VirtualHandPosition 2");
    //     }

    //     return nextVirtualHandPosition;
    // }

    // public Vector3 GetNewAnywhereHandPosition(Vector3 currentObjectPosition)
    // {
    //     Vector3 nextObjectPosition = currentObjectPosition;

    //     // text.text = IsGazeFixating.ToString();

    //     if (IsGazeFixating == false)
    //     {
    //         CurrentState = StaticState.Gaze;
    //     }
    //     else CurrentState = StaticState.Head;

    //     // CurrentState = StaticState.Gaze;

    //     nextObjectPosition += WristPosition_delta;
        
    //     if (CurrentState == StaticState.Gaze)
    //     {
    //         float distance = Vector3.Distance(GazeOrigin, nextObjectPosition);
    //         nextObjectPosition = GazeOrigin + GazeDirection * distance;
    //     }
    //     else
    //     {
    //         Vector3 objectDirection = (nextObjectPosition - GazeOrigin).normalized;
    //         nextObjectPosition = nextObjectPosition + objectDirection * GetHeadDepthOffset();
    //         nextObjectPosition = GazeOrigin + objectDirection * Mathf.Clamp(Vector3.Distance(nextObjectPosition, GazeOrigin), 1, 10);
    //     }
        

    //     return nextObjectPosition;
    // }



}