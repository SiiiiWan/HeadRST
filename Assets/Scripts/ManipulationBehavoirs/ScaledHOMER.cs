using Unity.Mathematics;
using UnityEngine;

public class ScaledHOMER : ManipulationTechnique
{
    public Transform TorsoPositionIndicator;
    public Transform HandPositionIndicator;

    public float TorsoOffset = 0.35f; // Offset from the camera to the torso

    private Vector3 lastHandPosition;
    public float scalingConstant = 0.15f; //TODO: fine parameters from papers
    public float minVelocityThreshold = 0.01f;

    public override void OnSingleHandGrabbed(Transform target)
    {
        HandPosition handData = HandPosition.GetInstance();
        Vector3 handPos = handData.GetHandPosition(usePinchTip: true);

        StartManipulation_HOMER(GetCurrentTorsoPosition(), handPos, target.position);

        lastHandPosition = handPos;
    }

    public override void ApplySingleHandGrabbedBehaviour(Transform target)
    {
        HandPosition handData = HandPosition.GetInstance();

        Vector3 handPos_delta = handData.GetDeltaHandPosition(usePinchTip: true);
        Quaternion handRot_delta = handData.GetDeltaHandRotation(usePinchTip: true);
        Vector3 handPos = handData.GetHandPosition(usePinchTip: true);


        Vector3 handVelocity = handPos_delta / Time.deltaTime;
        float handSpeed = handVelocity.magnitude;

        float scaleFactor = Mathf.Min(1.2f, handSpeed / scalingConstant);
        if (handSpeed < minVelocityThreshold)  scaleFactor = 0f;

        Vector3 scaledHandMovement = handPos_delta * scaleFactor;
        Vector3 scaledHandPosition = lastHandPosition + scaledHandMovement;

        lastHandPosition = handPos;

        target.position = UpdateObjectPosition_HOMER(scaledHandPosition);
        // target.position = UpdateObjectPosition_HOMER(handPos);

        Quaternion deltaRot = HandPosition.GetInstance().GetDeltaHandRotation(usePinchTip: true);
        target.rotation = deltaRot * target.rotation;
    }

    private Vector3 _torsoPosition_init;
    private Vector3 _handPosition_init;
    private Vector3 _objectPosition_init;
    private float _distanceHandToTorso_init;
    private float _distanceObjectToTorso_init;
    private Vector3 _offsetVector;

    public void StartManipulation_HOMER(Vector3 torso, Vector3 hand, Vector3 objectPosition)
    {
        // Difference between HOMER and visual gain is that HOMER handle the depth translation
        _torsoPosition_init = torso;
        _handPosition_init = hand;
        _objectPosition_init = objectPosition;

        _distanceHandToTorso_init = Vector3.Distance(_torsoPosition_init, _handPosition_init);
        _distanceObjectToTorso_init = Vector3.Distance(_torsoPosition_init, _objectPosition_init);

        Vector3 torso_to_hand_Direction_init = (_handPosition_init - _torsoPosition_init).normalized;
        Vector3 expectedObjectPosition = _torsoPosition_init + torso_to_hand_Direction_init * _distanceObjectToTorso_init;
        _offsetVector = _objectPosition_init - expectedObjectPosition;
    }

    Vector3 GetCurrentTorsoPosition()
    {
        return Camera.main.transform.position + Vector3.down * TorsoOffset;
    }

    public Vector3 UpdateObjectPosition_HOMER(Vector3 currentHandPosition)
    {
        Vector3 currentTorsoPosition = GetCurrentTorsoPosition();
        float distanceHandToTorso_current = Vector3.Distance(currentTorsoPosition, currentHandPosition);
        float scaledDistance = _distanceObjectToTorso_init / _distanceHandToTorso_init * distanceHandToTorso_current;

        Vector3 torso_to_hand_Direction_current = (currentHandPosition - currentTorsoPosition).normalized;
        Vector3 objectPosition = currentTorsoPosition + torso_to_hand_Direction_current * scaledDistance + _offsetVector;
        return objectPosition;
    }
    
}
