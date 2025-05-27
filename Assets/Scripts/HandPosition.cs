using UnityEngine;

public class HandPosition : Singleton<HandPosition>
{
    public Transform RightHandAnchor, LeftHandAnchor;

    public Vector3 RightHandPosition, LeftHandPosition;
    public Vector3 RightPinchTipPosition, LeftPinchTipPosition;
    public Quaternion RightHandRotation, LeftHandRotation;

    public Vector3 RightHandPosition_delta, LeftHandPosition_delta;
    public Vector3 RightPinchTipPosition_delta, LeftPinchTipPosition_delta;
    public Quaternion RightHandRotation_delta, LeftHandRotation_delta;

    public float RightHandSpeed, LeftHandSpeed;

    void Update()
    {
        RightHandPosition_delta = RightHandAnchor.position - RightHandPosition;
        LeftHandPosition_delta = LeftHandAnchor.position - LeftHandPosition;

        RightPinchTipPosition_delta = GetPinchTipPosition(PinchDetector.GetInstance().RightHand) - RightPinchTipPosition;
        LeftPinchTipPosition_delta = GetPinchTipPosition(PinchDetector.GetInstance().LeftHand) - LeftPinchTipPosition;

        RightHandRotation_delta = RightHandAnchor.rotation * Quaternion.Inverse(RightHandRotation);
        LeftHandRotation_delta = LeftHandAnchor.rotation * Quaternion.Inverse(LeftHandRotation);

        RightHandPosition = RightHandAnchor.position;
        LeftHandPosition = LeftHandAnchor.position;

        RightPinchTipPosition = GetPinchTipPosition(PinchDetector.GetInstance().RightHand);
        LeftPinchTipPosition = GetPinchTipPosition(PinchDetector.GetInstance().LeftHand);


        RightHandRotation = RightHandAnchor.rotation;
        LeftHandRotation = LeftHandAnchor.rotation;

        RightHandSpeed = RightHandPosition_delta.magnitude / Time.deltaTime;
        LeftHandSpeed = LeftHandPosition_delta.magnitude / Time.deltaTime;
    }

    private Vector3 GetPinchTipPosition(OVRHand hand)
    {
        if (hand == null) return Vector3.zero;
        var skeleton = hand.GetComponent<OVRSkeleton>();
        if (skeleton == null || skeleton.Bones == null) return Vector3.zero;

        // Index tip is BoneId.Hand_IndexTip
        foreach (var bone in skeleton.Bones)
        {
            if (bone.Id == OVRSkeleton.BoneId.Hand_IndexTip)
                return bone.Transform.position;
        }
        return Vector3.zero;
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

    public Quaternion GetHandRotation()
    {
        return PinchDetector.GetInstance().IsLeftPinching ? LeftHandRotation : RightHandRotation;
    }

    public Quaternion GetDeltaHandRotation()
    {
        return PinchDetector.GetInstance().IsLeftPinching ? LeftHandRotation_delta : RightHandRotation_delta;
    }

    public float GetHandSpeed()
    {
        return PinchDetector.GetInstance().IsLeftPinching ? LeftHandSpeed : RightHandSpeed;
    }
}
