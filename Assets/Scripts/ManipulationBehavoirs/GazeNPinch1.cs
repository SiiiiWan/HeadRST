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

        // target.position = hand.GetHandPosition(usePinchTip: true) + hand.GetHandDirection() * _targetDepthOffset;

        Vector3 gazeOrigin = EyeGaze.GetInstance().GetGazeRay().origin;
        Vector3 gazeDirection = EyeGaze.GetInstance().GetGazeRay().direction;

        _handRayLine.IsVisible = false;


        // if (IsGazeInSafeRegion(gazeOrigin, gazeDirection, target.position))
        if(EyeGaze.GetInstance().IsSaccading() == false)
        {
            Vector3 deltaPos = hand.GetDeltaHandPosition(usePinchTip: true);
            target.position += deltaPos;

            if (HeadMovement.GetInstance().HeadSpeed >= 0.2f && hand.GetHandSpeed() <= 0.5f)
            {
                // Vector3 targetDir = (target.position - EyeGaze.GetInstance().GetGazeRay().origin).normalized;
                // target.position += new Vector3(targetDir.x, 0, targetDir.z).normalized * HeadMovement.GetInstance().DeltaHeadY * 0.2f;
                Vector3 movementDirection = (target.position - hand.GetHandPosition(usePinchTip: true)).normalized;

                Vector3 nextTargetPosition = target.position + movementDirection * HeadMovement.GetInstance().DeltaHeadY * 0.2f;
                float nextTargetDistToHand = Vector3.Distance(nextTargetPosition, hand.GetHandPosition(usePinchTip: true));
                
                if (nextTargetDistToHand >= 0.05f & nextTargetDistToHand <= 10f)
                {
                    target.position = nextTargetPosition;
                }
                _handRayLine.IsVisible = true;

            }
        }
        else
        {
            ResetSafeRegion();

            float distance = Vector3.Distance(gazeOrigin, target.position);
            target.position = gazeOrigin + gazeDirection * distance;
        }

        Quaternion deltaRot = HandPosition.GetInstance().GetDeltaHandRotation(usePinchTip: true);
        target.rotation = deltaRot * target.rotation;
        // UpdateTargetOffset(target);
        _handRayLine.SetPostion(hand.GetHandPosition(usePinchTip: true), target.position);
    }


    void Awake()
    {
        _handRayLine = new Linescript();
    }
    
    private float _safeRegionRadius = 6f;
    private float _resetTimeStamp;

    bool IsGazeInSafeRegion(Vector3 gazeOrigin, Vector3 gazeDirection, Vector3 targetPosition)
    {

        _safeRegionRadius = _safeRegionRadius + 10 * (Time.time - _resetTimeStamp);
        if (_safeRegionRadius > 20f) _safeRegionRadius = 20f;

        float angularDistance = Vector3.Angle(gazeDirection, targetPosition - gazeOrigin);

        _safeRegionRadius = 6f; //TODO: remove this line, just for testing
        return angularDistance <= _safeRegionRadius;
    }
    
    void ResetSafeRegion()
    {
        _safeRegionRadius = 6f;
        _resetTimeStamp = Time.time;
    }
}