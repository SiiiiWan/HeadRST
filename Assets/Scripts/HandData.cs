using UnityEngine;

public enum Hand
{
    Left,
    Right
}

public class HandData : Singleton<HandData>
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
    public float RightHandSpeed_wrist, LeftHandSpeed_wrist;
    public float RightHandSpeed_pinch, LeftHandSpeed_pinch;
    public float HandDistance, HandDistance_delta;
    public Vector3 HandMidPosition, HandMidPosition_delta;

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

        HandDistance_delta = Vector3.Distance(RightHandPosition, LeftHandPosition) - HandDistance;
        HandDistance = Vector3.Distance(RightHandPosition, LeftHandPosition);

        HandMidPosition_delta = (RightHandPosition + LeftHandPosition) / 2 - HandMidPosition;
        HandMidPosition = (RightHandPosition + LeftHandPosition) / 2;

        Transform rightTip = GetPinchTipTransform(PinchDetector.GetInstance().RightHand);
        if (rightTip)
        {
            RightPinchTipPosition_delta = rightTip.position - RightPinchTipPosition;
            RightPinchTipRotation_delta = rightTip.rotation * Quaternion.Inverse(RightPinchTipRotation);
            RightPinchTipPosition = rightTip.position;
            RightPinchTipRotation = rightTip.rotation;
            RightHandSpeed_pinch = RightPinchTipPosition_delta.magnitude / Time.deltaTime;
        }


        Transform leftTip = GetPinchTipTransform(PinchDetector.GetInstance().LeftHand);
        if (leftTip)
        {
            LeftPinchTipPosition_delta = leftTip.position - LeftPinchTipPosition;
            LeftPinchTipRotation_delta = leftTip.rotation * Quaternion.Inverse(LeftPinchTipRotation);
            LeftPinchTipPosition = leftTip.position;
            LeftPinchTipRotation = leftTip.rotation;
            LeftHandSpeed_pinch = LeftPinchTipPosition_delta.magnitude / Time.deltaTime;
        }



        RightHandRotation = RightHandAnchor.rotation;
        LeftHandRotation = LeftHandAnchor.rotation;

        RightHandSpeed_wrist = RightHandPosition_delta.magnitude / Time.deltaTime;
        LeftHandSpeed_wrist = LeftHandPosition_delta.magnitude / Time.deltaTime;

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
            if (bone.Id == OVRSkeleton.BoneId.XRHand_ThumbTip) // Use Hand_IndexTip for OVR //XRHand_IndexTip
                return bone.Transform;
        }
        return null;
    }


    public Vector3 GetHandPosition(bool usePinchTip)
    {
        if (StudyControl.GetInstance().DominantHand == Handedness.left)
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
        if (StudyControl.GetInstance().DominantHand == Handedness.left)
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
        if (StudyControl.GetInstance().DominantHand == Handedness.left)
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
        if (StudyControl.GetInstance().DominantHand == Handedness.left)
        {
            return usePinchTip ? LeftPinchTipRotation_delta : LeftHandRotation_delta;
        }
        else
        {
            return usePinchTip ? RightPinchTipRotation_delta : RightHandRotation_delta;
        }
    }

    public float GetHandRotationSpeed(bool usePinchTip)
    {
        GetDeltaHandRotation(usePinchTip).ToAngleAxis(out float rotationAngle, out Vector3 axis);
        return rotationAngle / Time.deltaTime;
    }

    public float GetHandSpeed(bool usePinchTip)
    {
        if (StudyControl.GetInstance().DominantHand == Handedness.left)
        {
            return usePinchTip ? LeftHandSpeed_pinch : LeftHandSpeed_wrist;
        }
        else
        {
            return usePinchTip ? RightHandSpeed_pinch : RightHandSpeed_wrist;
        }
    }

    public Vector3 GetHandDirection()
    {
        return StudyControl.GetInstance().DominantHand == Handedness.left ? LeftHandDirection : RightHandDirection;
    }

    public Quaternion GetHandDirectionDelta()
    {
        return StudyControl.GetInstance().DominantHand == Handedness.left ? LeftHandDirection_delta : RightHandDirection_delta;
    }

    public Transform GetHandTransform(bool usePinchTip)
    {
        if (StudyControl.GetInstance().DominantHand == Handedness.left)
        {
            return usePinchTip ? GetPinchTipTransform(PinchDetector.GetInstance().LeftHand) : LeftHandAnchor;
        }
        else
        {
            return usePinchTip ? GetPinchTipTransform(PinchDetector.GetInstance().RightHand) : RightHandAnchor;
        }
    }


}
