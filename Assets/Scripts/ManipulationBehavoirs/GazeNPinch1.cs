using UnityEngine;

public class GazeNPinch1 : ManipulationTechnique
{
    private Linescript _handRayLine;

    private float _targetDepthOffset;

    public override void OnGrabbed(Transform target)
    {
        _targetDepthOffset = Vector3.Distance(target.position, HandPosition.GetInstance().GetHandPosition(usePinchTip: true));

    }

    public override void Apply(Transform target)
    {
        HandPosition hand = HandPosition.GetInstance();

        Vector3 deltaPos = hand.GetDeltaHandPosition(usePinchTip: true);
        target.position += deltaPos;
        
        // target.position = hand.GetHandPosition(usePinchTip: true) + hand.GetHandDirection() * _targetDepthOffset;

        if (HeadMovement.GetInstance().HeadSpeed >= 0.2f && hand.GetHandSpeed() <= 0.5f)
        {
            Vector3 targetDir = (target.position - EyeGaze.GetInstance().GetGazeRay().origin).normalized;
            target.position += new Vector3(targetDir.x, 0, targetDir.z).normalized * HeadMovement.GetInstance().DeltaHeadY * 0.2f;
        }

        // UpdateTargetOffset(target);
        _handRayLine.SetPostion(hand.GetHandPosition(usePinchTip: true), target.position);
    }


    void Awake()
    {
        _handRayLine = new Linescript();
    }
}