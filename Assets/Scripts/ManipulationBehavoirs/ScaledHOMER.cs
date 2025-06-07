using Unity.Mathematics;
using UnityEngine;

public class ScaledHOMER : ManipulationTechnique
{
    public Transform TorsoPositionIndicator;
    public Transform HandPositionIndicator;

    public float TorsoOffset = 0.35f; // Offset from the camera to the torso

    private Vector3 lastHandPosition;
    private float scalingConstant = 0.15f; //TODO: fine parameters from papers
    private float minVelocityThreshold = 0.01f;

    public override void OnSingleHandGrabbed(Transform target)
    {
        HandPosition handData = HandPosition.GetInstance();
        Vector3 handPos = handData.GetHandPosition(usePinchTip: false);

        Vector3 torsoPosition = Camera.main.transform.position + Vector3.down * TorsoOffset;

        StartManipulation_HOMER(torsoPosition, handPos, target.position);

        lastHandPosition = handPos;
    }

    public override void ApplySingleHandGrabbedBehaviour(Transform target)
    {
        HandPosition handData = HandPosition.GetInstance();

        Vector3 torsoPosition = Camera.main.transform.position + Vector3.down * TorsoOffset;
        TorsoPositionIndicator.position = torsoPosition;
        HandPositionIndicator.position = handData.GetHandPosition(usePinchTip: false);
        Vector3 currentHandPosition = handData.GetHandPosition(usePinchTip: false);

        //TODO: update hand position tracking

        Vector3 handVelocity = (currentHandPosition - lastHandPosition) / Time.deltaTime;
        float handSpeed = handVelocity.magnitude;

        float scaleFactor = Mathf.Min(1.2f, handSpeed / scalingConstant);
        if (handSpeed < minVelocityThreshold)
        {
            scaleFactor = 0f; // Freeze motion for precision
        }

        Vector3 scaledHandMovement = handVelocity.normalized * handSpeed * scaleFactor * Time.deltaTime;
        Vector3 scaledHandPosition = lastHandPosition + scaledHandMovement;

        lastHandPosition = currentHandPosition;

        target.position = UpdateObjectPosition_HOMER(scaledHandPosition);

        Quaternion deltaRot = HandPosition.GetInstance().GetDeltaHandRotation(usePinchTip: true);
        target.rotation = deltaRot * target.rotation;
    }

    private Vector3 torsoPosition;
    private Vector3 initialHandPosition;
    private Vector3 initialObjectPosition;
    private float distanceHandToTorso;
    private float distanceObjectToTorso;
    private Vector3 offsetVector;

    public void StartManipulation_HOMER(Vector3 torso, Vector3 hand, Vector3 objectPosition)
    {
        torsoPosition = torso;
        initialHandPosition = hand;
        initialObjectPosition = objectPosition;

        distanceHandToTorso = Vector3.Distance(torsoPosition, initialHandPosition);
        distanceObjectToTorso = Vector3.Distance(torsoPosition, initialObjectPosition);

        Vector3 rayDirection = (initialHandPosition - torsoPosition).normalized;
        Vector3 expectedObjectPosition = torsoPosition + rayDirection * distanceObjectToTorso;
        offsetVector = initialObjectPosition - expectedObjectPosition;
    }

    public Vector3 UpdateObjectPosition_HOMER(Vector3 currentHandPosition)
    {
        float currentHandDistance = Vector3.Distance(torsoPosition, currentHandPosition);
        float scaledDistance = (distanceObjectToTorso / distanceHandToTorso) * currentHandDistance;

        Vector3 bodyToHand = (currentHandPosition - torsoPosition).normalized;
        Vector3 objectPosition = torsoPosition + bodyToHand * scaledDistance + offsetVector;
        return objectPosition;
    }
    
}
