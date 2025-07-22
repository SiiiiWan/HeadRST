
using Unity.VisualScripting;
using UnityEngine;

public class test : ManipulationTechnique
{
    public Vector3 VirtualHandPosition;
    public void Start()
    {
        VirtualHandPosition = WristPosition;
    }

    Vector3 accumulatedOffset = Vector3.zero;

    public override void Update()
    {
        base.Update();

        if(IsGazeSaccading) accumulatedOffset = Vector3.zero;

        VirtualHandPosition = GazeOrigin + GazeDirection * 2 + accumulatedOffset;
        accumulatedOffset += WristPosition_delta * Vector3.Distance(VirtualHandPosition, GazeOrigin);


    }

    public override void ApplySingleHandGrabbedBehaviour()
    {

        // Vector3 gazeToObjDirection = (GrabbedObject.position - GazeOrigin).normalized;
        // Vector3 handToObjDirection = (GrabbedObject.position - PinchPosition).normalized;

        // float currentDistance = Vector3.Distance(GazeOrigin, GrabbedObject.position);

        // // if (DeltaHeadY < 0) GrabbedObject.position += (currentDistance > 2 ? gazeToObjDirection : handToObjDirection) * DeltaHeadY * VitLerp(HeadSpeed);
        // // else GrabbedObject.position += gazeToObjDirection * DeltaHeadY * VitLerp(HeadSpeed);
        // GrabbedObject.position += handToObjDirection * DeltaHeadY * VitLerp(HeadSpeed);
        // if (IsGazeFixating == false)
        // {
        //     GrabbedObject.position = GazeOrigin + GazeDirection * currentDistance;
        // }
        // else
        // {
        //     GrabbedObject.position += PinchPosition_delta * GetVisualGain();
        // }

        // GrabbedObject.rotation = PinchRotation_delta * GrabbedObject.rotation;
    }

    public override Vector3 GetVirtualHandPosition()
    {
        
        return VirtualHandPosition;
    }
}