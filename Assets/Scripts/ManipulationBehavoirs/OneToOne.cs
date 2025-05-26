using UnityEngine;

public class OneToOne : ManipulationTechnique
{
    public override void Apply(Transform target)
    {
        Vector3 deltaPos = HandPosition.GetInstance().GetDeltaHandPosition(usePinchTip: true);
        target.position += deltaPos;

        Quaternion deltaRot = HandPosition.GetInstance().GetDeltaHandRotation();
        target.rotation = deltaRot * target.rotation;
    }
}