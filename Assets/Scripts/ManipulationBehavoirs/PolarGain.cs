using UnityEngine;

public class PolarGain : ManipulationTechnique
{
    private Linescript _handRayLine;
    private float fixedDistance;
    Quaternion _rotationOffsetOnGrab;
    public override void OnSingleHandGrabbed(Transform target)
    {
        HandPosition handData = HandPosition.GetInstance();
        handDirection_pre = handData.GetHandDirection();

        fixedDistance = Vector3.Distance(target.position, Camera.main.transform.position);

        _rotationOffsetOnGrab = Quaternion.FromToRotation(handData.GetHandDirection(), (target.position - handData.GetHandPosition(usePinchTip: false)).normalized);
    }

    private Vector3 handDirection_pre;
    public override void ApplySingleHandGrabbedBehaviour(Transform target)
    {
        HandPosition handData = HandPosition.GetInstance();
        Vector3 handPos = handData.GetHandPosition(usePinchTip: true);
        Vector3 handDirection = handData.GetHandDirection();

        Quaternion delta_Direction = Quaternion.FromToRotation(handDirection_pre, handDirection);
        // float delta_Distance = Distance_HeadToHand - Distance_HeadToHand_pre;

        Vector3 Direction_HeadToTarget = (target.position - Camera.main.transform.position).normalized;
        float Distance_HeadToTarget = Vector3.Distance(target.position, Camera.main.transform.position);

        Vector3 targetDirection_new = delta_Direction * Direction_HeadToTarget;
        float targetDistance_new = fixedDistance;


        // Vector3 deltaPos = HandPosition.GetInstance().GetDeltaHandPosition(usePinchTip: true);
        // target.position += deltaPos * GetAdoptiveGain(target);

        // target.position = Camera.main.transform.position + targetDirection_new * targetDistance_new;
        target.position = handData.GetHandPosition(usePinchTip: false) + _rotationOffsetOnGrab * handDirection * fixedDistance;

        Quaternion deltaRot = HandPosition.GetInstance().GetDeltaHandRotation(usePinchTip: true);
        target.rotation = deltaRot * target.rotation;

        handDirection_pre = handDirection;

        _handRayLine.SetPostion(handData.GetHandPosition(usePinchTip: false), handData.GetHandPosition(usePinchTip: false) + handDirection * 2);
    }

    void Awake()
    {
        _handRayLine = new Linescript();
    }
}