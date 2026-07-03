using UnityEngine;

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

    public float RightHandSpeed_wrist, LeftHandSpeed_wrist;
    public float RightHandSpeed_pinch, LeftHandSpeed_pinch;

    private bool _warnedMissingHandAnchors;

    void Update()
    {
        if (RightHandAnchor == null || LeftHandAnchor == null)
        {
            if (!_warnedMissingHandAnchors)
            {
                Debug.LogWarning("HandData is missing left or right hand anchors. Hand tracking data will not update until the bindings are restored.", this);
                _warnedMissingHandAnchors = true;
            }

            return;
        }

        float deltaTime = Mathf.Max(Time.deltaTime, 0.0001f);

        RightHandPosition_delta = RightHandAnchor.position - RightHandPosition;
        LeftHandPosition_delta = LeftHandAnchor.position - LeftHandPosition;

        RightHandRotation_delta = RightHandAnchor.rotation * Quaternion.Inverse(RightHandRotation);
        LeftHandRotation_delta = LeftHandAnchor.rotation * Quaternion.Inverse(LeftHandRotation);

        RightHandPosition = RightHandAnchor.position;
        LeftHandPosition = LeftHandAnchor.position;

        PinchDetector pinchDetector = PinchDetector.GetInstance();
        Transform rightTip = GetPinchTipTransform(pinchDetector != null ? pinchDetector.RightHand : null);
        if (rightTip)
        {
            RightPinchTipPosition_delta = rightTip.position - RightPinchTipPosition;
            RightPinchTipRotation_delta = rightTip.rotation * Quaternion.Inverse(RightPinchTipRotation);
            RightPinchTipPosition = rightTip.position;
            RightPinchTipRotation = rightTip.rotation;
            RightHandSpeed_pinch = RightPinchTipPosition_delta.magnitude / deltaTime;
        }

        Transform leftTip = GetPinchTipTransform(pinchDetector != null ? pinchDetector.LeftHand : null);
        if (leftTip)
        {
            LeftPinchTipPosition_delta = leftTip.position - LeftPinchTipPosition;
            LeftPinchTipRotation_delta = leftTip.rotation * Quaternion.Inverse(LeftPinchTipRotation);
            LeftPinchTipPosition = leftTip.position;
            LeftPinchTipRotation = leftTip.rotation;
            LeftHandSpeed_pinch = LeftPinchTipPosition_delta.magnitude / deltaTime;
        }

        RightHandRotation = RightHandAnchor.rotation;
        LeftHandRotation = LeftHandAnchor.rotation;

        RightHandSpeed_wrist = RightHandPosition_delta.magnitude / deltaTime;
        LeftHandSpeed_wrist = LeftHandPosition_delta.magnitude / deltaTime;
    }

    private Transform GetPinchTipTransform(OVRHand hand)
    {
        if (hand == null) return null;
        var skeleton = hand.GetComponent<OVRSkeleton>();
        if (skeleton == null || skeleton.Bones == null) return null;

        foreach (var bone in skeleton.Bones)
        {
            if (bone.Id == OVRSkeleton.BoneId.XRHand_ThumbTip)
                return bone.Transform;
        }
        return null;
    }

    public Vector3 GetHandPosition(Handedness dominantHand, bool usePinchTip)
    {
        if (dominantHand == Handedness.left)
        {
            return usePinchTip ? LeftPinchTipPosition : LeftHandPosition;
        }
        else
        {
            return usePinchTip ? RightPinchTipPosition : RightHandPosition;
        }
    }

    public Vector3 GetDeltaHandPosition(Handedness dominantHand, bool usePinchTip)
    {
        if (dominantHand == Handedness.left)
        {
            return usePinchTip ? LeftPinchTipPosition_delta : LeftHandPosition_delta;
        }
        else
        {
            return usePinchTip ? RightPinchTipPosition_delta : RightHandPosition_delta;
        }
    }

    public Quaternion GetDeltaHandRotation(Handedness dominantHand, bool usePinchTip)
    {
        if (dominantHand == Handedness.left)
        {
            return usePinchTip ? LeftPinchTipRotation_delta : LeftHandRotation_delta;
        }
        else
        {
            return usePinchTip ? RightPinchTipRotation_delta : RightHandRotation_delta;
        }
    }

    public float GetHandSpeed(Handedness dominantHand, bool usePinchTip)
    {
        if (dominantHand == Handedness.left)
        {
            return usePinchTip ? LeftHandSpeed_pinch : LeftHandSpeed_wrist;
        }
        else
        {
            return usePinchTip ? RightHandSpeed_pinch : RightHandSpeed_wrist;
        }
    }

}