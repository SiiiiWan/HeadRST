using System;
using UnityEngine;

public enum CentricType
{
    HandCentric,
    HeadCentric,
}

public enum HandGainFunction
{
    isomophic,
    visual,
    prism,
    gazeNpinch,
    HOMER,
    ScaledHOMER,
}

public class GazeNPinch1 : ManipulationTechnique
{
    public CentricType CentricType;
    public HandGainFunction HandGainFunction;
    public bool AddGaze;
    public bool AddHead;
    private Linescript _handRayLine;
    public bool ShowRayLine = true;

    private float scalingConstant = 0.15f; //TODO: fine parameters from papers
    private float minVelocityThreshold = 0.01f;

    public override void ApplySingleHandGrabbedBehaviour(Transform target)
    {
        HandPosition hand = HandPosition.GetInstance();

        Vector3 gazeOrigin = EyeGaze.GetInstance().GetGazeRay().origin;
        Vector3 gazeDirection = EyeGaze.GetInstance().GetGazeRay().direction;

        _handRayLine.IsVisible = false;

        // if (IsGazeInSafeRegion(gazeOrigin, gazeDirection, target.position))
        if (EyeGaze.GetInstance().IsSaccading() == false)
        {
            Vector3 deltaPos = hand.GetDeltaHandPosition(usePinchTip: true);
            target.position += deltaPos * GetTranslationGain(target);

            if ((HeadMovement.GetInstance().HeadSpeed >= 0.2f || Math.Abs(HeadMovement.GetInstance().HeadAcc) >= 1f) && hand.GetHandSpeed() <= 0.5f && AddHead)
            {
                // Vector3 targetDir = (target.position - EyeGaze.GetInstance().GetGazeRay().origin).normalized;
                // target.position += new Vector3(targetDir.x, 0, targetDir.z).normalized * HeadMovement.GetInstance().DeltaHeadY * 0.2f;
                Vector3 startPoint = CentricType == CentricType.HandCentric ? hand.GetHandPosition(usePinchTip: true) : gazeOrigin;
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
                if(ShowRayLine) _handRayLine.IsVisible = true;
            }
        }
        else if (AddGaze)
        {
            float distance = Vector3.Distance(gazeOrigin, target.position);
            target.position = gazeOrigin + gazeDirection * distance;
        }

        Quaternion deltaRot = HandPosition.GetInstance().GetDeltaHandRotation(usePinchTip: true);
        target.rotation = deltaRot * target.rotation;

        _handRayLine.SetPostion(hand.GetHandPosition(usePinchTip: true), target.position);
    }

    public override void ApplyBothHandGrabbedBehaviour(Transform target)
    {
        HandPosition hand = HandPosition.GetInstance();

        if (Mathf.Abs(hand.HandDistance_delta) > 0.0001f)
        {

            float scaleFactor = Vector3.Distance(Camera.main.transform.position, target.position) / Vector3.Distance(Camera.main.transform.position, hand.HandMidPosition);
            target.localScale *= 1f + hand.HandDistance_delta * scaleFactor;
        }
    }

    float GetTranslationGain(Transform target)
    {
        switch (HandGainFunction)
        {
            case HandGainFunction.isomophic:
                return 1f;
            case HandGainFunction.visual:
                return GetVisualGain(target); // Visual gain based on distance
            case HandGainFunction.prism:
                return GetPrismGain(); // Prism gain based on some function
            default:
                return 1f; // Default gain
        }
    }

    float GetVisualGain(Transform target)
    {
        return Vector3.Distance(target.position, Camera.main.transform.position) / Vector3.Distance(HandPosition.GetInstance().GetDeltaHandPosition(usePinchTip: true), Camera.main.transform.position);
    }

    float GetPrismGain()
    {
        float handSpeed = HandPosition.GetInstance().GetHandSpeed();

        if (handSpeed < minVelocityThreshold)
        {
            return 0f;
        }

        return Mathf.Min(1.2f, handSpeed / scalingConstant);

    }

    void Awake()
    {
        _handRayLine = new Linescript();
    }

}