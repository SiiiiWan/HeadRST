using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class ScaledHOMER : ManipulationTechnique
{
    public float TorsoOffset = 0.35f; // Offset from the camera to the torso

    private Vector3 lastHandPosition;
    public float scalingConstant = 0.15f; // at what hand speed m/s do you want to achive a 1:1 mapping //TODO: fine parameters from papers
    public float MinVelocityThreshold = 0.01f;

    public override void ApplyObjectFreeBehaviour()
    {
        VirtualHandPosition = WristPosition;
    }

     public override void TriggerOnLookAtNewObjectBehavior()
    {
        GazingObject.DisableDirectGrab();

        VirtualHandPosition = GazingObject.transform.position + (WristPosition - PinchPosition);   
    }   

    private Vector3 _torsoPosition_init;
    private Vector3 _handPosition_init;
    private Vector3 _objectPosition_init;
    private float _distanceHandToTorso_init;
    private float _distanceObjectToTorso_init;
    private Vector3 _offsetVector;
    public override void TriggerOnSingleHandGrabbed(ManipulatableObject obj, GrabbedState grabbedState)
    {
        base.TriggerOnSingleHandGrabbed(obj, grabbedState);

        _torsoPosition_init = HeadPosition + Vector3.down * TorsoOffset;
        _handPosition_init = WristPosition;
        _objectPosition_init = GrabbedObject.transform.position;

        _distanceHandToTorso_init = Vector3.Distance(_torsoPosition_init, _handPosition_init);
        _distanceObjectToTorso_init = Vector3.Distance(_torsoPosition_init, _objectPosition_init);

        Vector3 torso_to_hand_Direction_init = (_handPosition_init - _torsoPosition_init).normalized;
        Vector3 expectedObjectPosition = _torsoPosition_init + torso_to_hand_Direction_init * _distanceObjectToTorso_init;
        _offsetVector = _objectPosition_init - expectedObjectPosition;

        lastHandPosition = WristPosition;
    }

    public override void ApplyIndirectGrabbedBehaviour()
    {
        float scaleFactor = Mathf.Min(1.2f, HandTranslationSpeed / scalingConstant);
        if (HandTranslationSpeed < MinVelocityThreshold) scaleFactor = 0f;

        Vector3 scaledHandMovement = WristPosition_delta * scaleFactor;
        Vector3 scaledHandPosition = lastHandPosition + scaledHandMovement;

        lastHandPosition = scaledHandPosition;

        GrabbedObject.transform.position = UpdateObjectPosition_HOMER(scaledHandPosition);
        VirtualHandPosition = GrabbedObject.transform.position + (WristPosition - PinchPosition);

        GrabbedObject.transform.rotation = PinchRotation_delta * GrabbedObject.transform.rotation;
    }


    public Vector3 UpdateObjectPosition_HOMER(Vector3 currentHandPosition)
    {
        Vector3 currentTorsoPosition = HeadPosition + Vector3.down * TorsoOffset;
        float distanceHandToTorso_current = Vector3.Distance(currentTorsoPosition, currentHandPosition);
        float scaledDistance = _distanceObjectToTorso_init / _distanceHandToTorso_init * distanceHandToTorso_current;

        Vector3 torso_to_hand_Direction_current = (currentHandPosition - currentTorsoPosition).normalized;
        Vector3 objectPosition = currentTorsoPosition + torso_to_hand_Direction_current * scaledDistance + _offsetVector;
        return objectPosition;
    }


}
