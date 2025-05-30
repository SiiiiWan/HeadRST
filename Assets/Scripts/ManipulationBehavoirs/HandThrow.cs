using UnityEngine;

public class HandThrow : ManipulationTechnique
{
    private Vector3 _targetVelocity;
    public float speedThreshold = 1f;
    public float speedMultiplier = 1.5f;
    
    public string wallTag = "Invisible wall";

    public override void ApplySingleHandGrabbedBehaviour(Transform target)
    {
        Vector3 deltaPos = HandPosition.GetInstance().GetDeltaHandPosition(usePinchTip: true);
        Vector3 nextTargetPosition = target.position + deltaPos;

        _targetVelocity = (nextTargetPosition - target.position) / Time.deltaTime;

        target.position = nextTargetPosition;

        Quaternion deltaRot = HandPosition.GetInstance().GetDeltaHandRotation(usePinchTip: true);
        target.rotation = deltaRot * target.rotation;
    }

    public override void ApplySingleHandReleasedBehaviour(Transform target)
    {
        if (_targetVelocity.magnitude < speedThreshold) return;

        Vector3 movementDirection = _targetVelocity.normalized;
        float speed = Mathf.Clamp(_targetVelocity.magnitude, speedThreshold, 2f);

        Vector3 nextTargetPosition = target.position + movementDirection * speed * speedMultiplier* Time.deltaTime;

        if (Physics.Raycast(target.position, movementDirection, out RaycastHit hit, (nextTargetPosition - target.position).magnitude + 0.01f))
        {
            if (hit.collider.CompareTag(wallTag))
            {
                // Reflect the velocity vector
                _targetVelocity = Vector3.Reflect(_targetVelocity, hit.normal);
                // Move the object to the hit point (with a small offset to avoid sticking)
                target.position = hit.point + hit.normal * 0.01f;
                return;
            }
        }

        if (Vector3.Distance(nextTargetPosition, Camera.main.transform.position) < 10f)
            target.position = nextTargetPosition;
    }
}