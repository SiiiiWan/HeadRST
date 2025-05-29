using UnityEngine;

public class HandThrow : ManipulationTechnique
{
    private Vector3 _targetVelocity;
    public float speedThreshold = 0.01f;
    public override void ApplyGrabbedBehaviour(Transform target)
    {
        Vector3 deltaPos = HandPosition.GetInstance().GetDeltaHandPosition(usePinchTip: true);
        Vector3 nextTargetPosition = target.position + deltaPos * Vector3.Distance(target.position, Camera.main.transform.position); ;

        _targetVelocity = (nextTargetPosition - target.position) / Time.deltaTime;

        target.position = nextTargetPosition;
        
        Quaternion deltaRot = HandPosition.GetInstance().GetDeltaHandRotation(usePinchTip: true);
        target.rotation = deltaRot * target.rotation;
    }

    public override void ApplyReleasedBehaviour(Transform target)
    {
        if (_targetVelocity.magnitude < speedThreshold) return;

        Vector3 movementDirection = _targetVelocity.normalized;
        float speed = Mathf.Clamp(_targetVelocity.magnitude, speedThreshold, 2f);

        Vector3 nextTargetPosition = target.position + movementDirection * speed * Time.deltaTime;

        if (Vector3.Distance(nextTargetPosition, Camera.main.transform.position) < 10f)
            target.position = nextTargetPosition;
    }
}