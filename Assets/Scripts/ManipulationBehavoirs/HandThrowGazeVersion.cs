using UnityEngine;

public class HandThrowGazeVersion : ManipulationTechnique
{
    private Vector3 _targetVelocity;
    public float speedThreshold = 1f;
    public float speedMultiplier = 1.5f;
    
    public string wallTag = "Invisible wall";

    public override void ApplySingleHandGrabbedBehaviour(Transform target)
    {

        Vector3 gazeOrigin = EyeGaze.GetInstance().GetGazeRay().origin;
        Vector3 gazeDirection = EyeGaze.GetInstance().GetGazeRay().direction;

        if (EyeGaze.GetInstance().IsSaccading() == false)
        {
            Vector3 deltaPos = HandPosition.GetInstance().GetDeltaHandPosition(usePinchTip: true);
            Vector3 nextTargetPosition = target.position + deltaPos;

            _targetVelocity = (nextTargetPosition - target.position) / Time.deltaTime;

            target.position = nextTargetPosition;
        }
        else
        {
            float distance = Vector3.Distance(gazeOrigin, target.position);
            target.position = gazeOrigin + gazeDirection * distance;
        }
        
        Quaternion deltaRot = HandPosition.GetInstance().GetDeltaHandRotation(usePinchTip: true);
        target.rotation = deltaRot * target.rotation;
    }

    public override void ApplyHandReleasedBehaviour(Transform target)
    {

        Vector3 handToTargetDirection = (target.position - HandPosition.GetInstance().GetHandPosition(usePinchTip: true)).normalized;
        Vector3 movementVelocity = Vector3.Project(_targetVelocity, handToTargetDirection);
        if (movementVelocity.magnitude < speedThreshold) return;

        Vector3 nextTargetPosition = target.position + movementVelocity * speedMultiplier * Time.deltaTime;

        if (Physics.Raycast(target.position, movementVelocity.normalized, out RaycastHit hit, (nextTargetPosition - target.position).magnitude + 0.01f))
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

        // if (Vector3.Distance(nextTargetPosition, Camera.main.transform.position) < 10f)
        target.position = nextTargetPosition;
    }
}