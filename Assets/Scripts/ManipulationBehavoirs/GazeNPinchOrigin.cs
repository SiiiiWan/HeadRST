using UnityEngine;

public class GazeNPinchOrigin : ManipulationTechnique
{
    public override void ApplyGrabbedBehaviour(Transform target)
    {
        Vector3 deltaPos = HandPosition.GetInstance().GetDeltaHandPosition(usePinchTip: true);
        target.position += deltaPos * Vector3.Distance(target.position, Camera.main.transform.position);
    }
}