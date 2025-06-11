using UnityEngine;

public class GazeNPinchOrigin : ManipulationTechnique
{
    public override void ApplySingleHandGrabbedBehaviour(Transform target)
    {
        Vector3 deltaPos = HandPosition.GetInstance().GetDeltaHandPosition(usePinchTip: true);
        target.position += deltaPos * GetOriginGain(target);

        Quaternion deltaRot = HandPosition.GetInstance().GetDeltaHandRotation(usePinchTip: true);
        target.rotation = deltaRot * target.rotation;
    }

    float GetOriginGain(Transform target)
    {
        return Vector3.Distance(target.position, Camera.main.transform.position);
    }

    float GetVisualGain(Transform target)
    {
        return Vector3.Distance(target.position, Camera.main.transform.position) / Vector3.Distance(HandPosition.GetInstance().GetDeltaHandPosition(usePinchTip: true), Camera.main.transform.position);
    }
}