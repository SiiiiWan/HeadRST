using UnityEngine;

public class GazeNPinch1 : ManipulationTechnique
{
    public override void Apply(Transform target)
    {
        float objectDistance = Vector3.Distance(target.position, Camera.main.transform.position);
        Vector3 deltaPos = HandPosition.GetInstance().GetDeltaHandPosition(usePinchTip: true);
        target.position += deltaPos * objectDistance;

    }
}