using UnityEngine;

public class HandPosition : Singleton<HandPosition>
{

    public Transform RightHandAnchor, LeftHandAnchor;

    public Vector3 RightHandPosition, LeftHandPosition;
    public Vector3 RightPinchTipPosition, LeftPinchTipPosition;
    public Quaternion RightHandRotation, LeftHandRotation;
    public Quaternion RightPinchTipRotation, LeftPinchTipRotation;

    public Vector3 RightHandPosition_delta, LeftHandPosition_delta;
    public Vector3 RightPinchTipPosition_delta, LeftPinchTipPosition_delta;
    public Quaternion RightHandRotation_delta, LeftHandRotation_delta;
    public Quaternion RightPinchTipRotation_delta, LeftPinchTipRotation_delta;

    public Vector3 RightHandDirection, LeftHandDirection;
    public Quaternion RightHandDirection_delta, LeftHandDirection_delta;
    public float RightHandSpeed, LeftHandSpeed;

    void Update()
    {
        RightHandPosition_delta = RightHandAnchor.position - RightHandPosition;
        LeftHandPosition_delta = LeftHandAnchor.position - LeftHandPosition;

        RightHandRotation_delta = RightHandAnchor.rotation * Quaternion.Inverse(RightHandRotation);
        LeftHandRotation_delta = LeftHandAnchor.rotation * Quaternion.Inverse(LeftHandRotation);

        RightHandDirection_delta = Quaternion.FromToRotation(RightHandDirection, RightHandAnchor.forward);
        LeftHandDirection_delta = Quaternion.FromToRotation(LeftHandDirection, LeftHandAnchor.forward);

        RightHandPosition = RightHandAnchor.position;
        LeftHandPosition = LeftHandAnchor.position;

        Transform rightTip = GetPinchTipTransform(PinchDetector.GetInstance().RightHand);
        RightPinchTipPosition_delta = rightTip.position - RightPinchTipPosition;
        RightPinchTipRotation_delta = rightTip.rotation * Quaternion.Inverse(RightPinchTipRotation);
        RightPinchTipPosition = rightTip.position;
        RightPinchTipRotation = rightTip.rotation;

        Transform leftTip = GetPinchTipTransform(PinchDetector.GetInstance().LeftHand);
        LeftPinchTipPosition_delta = leftTip.position - LeftPinchTipPosition;
        LeftPinchTipRotation_delta = leftTip.rotation * Quaternion.Inverse(LeftPinchTipRotation);
        LeftPinchTipPosition = leftTip.position;
        LeftPinchTipRotation = leftTip.rotation;


        RightHandRotation = RightHandAnchor.rotation;
        LeftHandRotation = LeftHandAnchor.rotation;

        RightHandSpeed = RightHandPosition_delta.magnitude / Time.deltaTime;
        LeftHandSpeed = LeftHandPosition_delta.magnitude / Time.deltaTime;

        RightHandDirection = RightHandAnchor.forward;
        LeftHandDirection = LeftHandAnchor.forward;
    }

    private Transform GetPinchTipTransform(OVRHand hand)
    {
        if (hand == null) return null;
        var skeleton = hand.GetComponent<OVRSkeleton>();
        if (skeleton == null || skeleton.Bones == null) return null;

        foreach (var bone in skeleton.Bones)
        {
            if (bone.Id == OVRSkeleton.BoneId.XRHand_IndexTip) // Use Hand_IndexTip for OVR
                return bone.Transform;
        }
        return null;
    }


    public Vector3 GetHandPosition(bool usePinchTip)
    {
        if (PinchDetector.GetInstance().IsLeftPinching)
        {
            return usePinchTip ? LeftPinchTipPosition : LeftHandPosition;
        }
        else
        {
            return usePinchTip ? RightPinchTipPosition : RightHandPosition;
        }
    }

    public Vector3 GetDeltaHandPosition(bool usePinchTip)
    {
        if (PinchDetector.GetInstance().IsLeftPinching)
        {
            return usePinchTip ? LeftPinchTipPosition_delta : LeftHandPosition_delta;
        }
        else
        {
            return usePinchTip ? RightPinchTipPosition_delta : RightHandPosition_delta;
        }
    }

    public Quaternion GetHandRotation(bool usePinchTip)
    {
        if (PinchDetector.GetInstance().IsLeftPinching)
        {
            return usePinchTip ? LeftPinchTipRotation : LeftHandRotation;
        }
        else
        {
            return usePinchTip ? RightPinchTipRotation : RightHandRotation;
        }
    }

    public Quaternion GetDeltaHandRotation(bool usePinchTip)
    {
        if (PinchDetector.GetInstance().IsLeftPinching)
        {
            return usePinchTip ? LeftPinchTipRotation_delta : LeftHandRotation_delta;
        }
        else
        {
            return usePinchTip ? RightPinchTipRotation_delta : RightHandRotation_delta;
        }
    }

    public float GetHandSpeed()
    {
        return PinchDetector.GetInstance().IsLeftPinching ? LeftHandSpeed : RightHandSpeed;
    }

    public Vector3 GetHandDirection()
    {
        return PinchDetector.GetInstance().IsLeftPinching ? LeftHandDirection : RightHandDirection;
    }

    public Quaternion GetHandDirectionDelta()
    {
        return PinchDetector.GetInstance().IsLeftPinching ? LeftHandDirection_delta : RightHandDirection_delta;
    }

    public Transform GetHandTransform(bool usePinchTip)
    {
        if (PinchDetector.GetInstance().IsLeftPinching)
        {
            return usePinchTip ? GetPinchTipTransform(PinchDetector.GetInstance().LeftHand) : LeftHandAnchor;
        }
        else
        {
            return usePinchTip ? GetPinchTipTransform(PinchDetector.GetInstance().RightHand) : RightHandAnchor;
        }
    }


}
