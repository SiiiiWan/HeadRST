
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

        if (IsGazeFixating == false)
        {
            VirtualHandPosition = GazeOrigin + GazeDirection * Mathf.Clamp(Vector3.Distance(GazeOrigin, VirtualHandPosition), 1f, 10f);

        }
        else
        {
            Vector3 objDirection = (VirtualHandPosition - GazeOrigin).normalized;

            if (IsHeadFixating == false)
            {
                VirtualHandPosition += objDirection * DeltaHeadY * VitLerp(HeadSpeed);
                VirtualHandPosition = GazeOrigin + objDirection *  Mathf.Clamp(Vector3.Distance(GazeOrigin, VirtualHandPosition), 1f, 10f);
            }    

            VirtualHandPosition += WristPosition_delta * Vector3.Distance(VirtualHandPosition, GazeOrigin) / Vector3.Distance(WristPosition, GazeOrigin);
        }


    }

    public override void ApplyIndirectGrabbedBehaviour()
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
}