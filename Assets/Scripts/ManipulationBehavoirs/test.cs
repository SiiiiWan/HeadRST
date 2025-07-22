
using Unity.VisualScripting;
using UnityEngine;

public class test : ManipulationTechnique
{
    public override void ApplySingleHandGrabbedBehaviour()
    {
        Vector3 gazeToObjDirection = (GrabbedObject.position - GazeOrigin).normalized;
        Vector3 handToObjDirection = (GrabbedObject.position - HandPosition).normalized;

        float currentDistance = Vector3.Distance(GazeOrigin, GrabbedObject.position);

        // if (DeltaHeadY < 0) GrabbedObject.position += (currentDistance > 2 ? gazeToObjDirection : handToObjDirection) * DeltaHeadY * VitLerp(HeadSpeed);
        // else GrabbedObject.position += gazeToObjDirection * DeltaHeadY * VitLerp(HeadSpeed);
        GrabbedObject.position += handToObjDirection * DeltaHeadY * VitLerp(HeadSpeed);
        if (IsGazeFixating == false)
        {
            GrabbedObject.position = GazeOrigin + GazeDirection * currentDistance;
        }
        else
        {
            GrabbedObject.position += HandPosition_delta * GetVisualGain();
        }   
        
        GrabbedObject.rotation = HandRotation_delta * GrabbedObject.rotation;
    }
}