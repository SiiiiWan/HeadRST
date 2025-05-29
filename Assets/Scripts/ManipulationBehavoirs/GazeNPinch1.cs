using System;
using UnityEngine;

public enum CentricType
{
    HandCentric,
    HeadCentric,
}
public class GazeNPinch1 : ManipulationTechnique
{
    public CentricType CentricType;
    private Linescript _handRayLine;

    public override void Apply(Transform target)
    {
        HandPosition hand = HandPosition.GetInstance();

        Vector3 gazeOrigin = EyeGaze.GetInstance().GetGazeRay().origin;
        Vector3 gazeDirection = EyeGaze.GetInstance().GetGazeRay().direction;

        _handRayLine.IsVisible = false;

        // if (IsGazeInSafeRegion(gazeOrigin, gazeDirection, target.position))
        if (EyeGaze.GetInstance().IsSaccading() == false)
        {
            Vector3 deltaPos = hand.GetDeltaHandPosition(usePinchTip: true);
            target.position += deltaPos;

            if ((HeadMovement.GetInstance().HeadSpeed >= 0.2f || Math.Abs(HeadMovement.GetInstance().HeadAcc) >= 1f) && hand.GetHandSpeed() <= 0.5f)
            {
                // Vector3 targetDir = (target.position - EyeGaze.GetInstance().GetGazeRay().origin).normalized;
                // target.position += new Vector3(targetDir.x, 0, targetDir.z).normalized * HeadMovement.GetInstance().DeltaHeadY * 0.2f;
                Vector3 startPoint = CentricType == CentricType.HandCentric? hand.GetHandPosition(usePinchTip: true): gazeOrigin;
                Vector3 movementDirection = (target.position - startPoint).normalized;

                Vector3 nextTargetPosition = target.position + movementDirection * HeadMovement.GetInstance().DeltaHeadY * 0.2f;
                // float nextTargetEyeInHeadAngle = Vector3.Angle(HeadMovement.GetInstance().CamDir, nextTargetPosition - HeadMovement.GetInstance().CamPos);
                // if (nextTargetEyeInHeadAngle >= 15f)
                // {
                //     nextTargetPosition = target.position + movementDirection * HeadMovement.GetInstance().DeltaHeadY * 0.5f;
                // }
                //TODO: can add a eye head angle exiding just warp to the depth limits

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
            float distance = Vector3.Distance(gazeOrigin, target.position);
            target.position = gazeOrigin + gazeDirection * distance;
        }

        Quaternion deltaRot = HandPosition.GetInstance().GetDeltaHandRotation(usePinchTip: true);
        target.rotation = deltaRot * target.rotation;

        _handRayLine.SetPostion(hand.GetHandPosition(usePinchTip: true), target.position);
    }


    void Awake()
    {
        _handRayLine = new Linescript();
    }

}