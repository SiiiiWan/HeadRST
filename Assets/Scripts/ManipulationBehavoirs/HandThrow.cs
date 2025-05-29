using UnityEngine;

public class HandThrow : ManipulationTechnique
{
    private Vector3 _targetVelocity;

    public override void ApplyGrabbedBehaviour(Transform target)
    {
        Vector3 deltaPos = HandPosition.GetInstance().GetDeltaHandPosition(usePinchTip: true);
        Vector3 nextTargetPosition = target.position + deltaPos * Vector3.Distance(target.position, Camera.main.transform.position);;

        _targetVelocity = (nextTargetPosition - target.position) / Time.deltaTime;
        
        target.position = nextTargetPosition;
    }

    public override void ApplyReleasedBehaviour(Transform target)
    {
        if (_targetVelocity.magnitude < 0.01f) return;
        // print("throwing");
        target.position += _targetVelocity * Time.deltaTime;
    }
}