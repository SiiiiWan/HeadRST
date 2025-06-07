using UnityEngine;

public class GazeNPinchOrigin : ManipulationTechnique
{
    public override void ApplySingleHandGrabbedBehaviour(Transform target)
    {
        Vector3 deltaPos = HandPosition.GetInstance().GetDeltaHandPosition(usePinchTip: true);
        target.position += deltaPos * Vector3.Distance(target.position, Camera.main.transform.position);

        Quaternion deltaRot = HandPosition.GetInstance().GetDeltaHandRotation(usePinchTip: true);
        target.rotation = deltaRot * target.rotation;
    }
}