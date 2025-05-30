using UnityEngine;

public class PinchDetector : Singleton<PinchDetector>
{
    public OVRHand RightHand, LeftHand;
    public bool IsOneHandPinching, IsBothHandsPinching, IsRightPinching, IsLeftPinching;

    void Update()
    {
        IsRightPinching = RightHand.GetFingerIsPinching(OVRHand.HandFinger.Index);
        IsLeftPinching = LeftHand.GetFingerIsPinching(OVRHand.HandFinger.Index);

        IsBothHandsPinching = IsRightPinching && IsLeftPinching;
        IsOneHandPinching = (IsRightPinching || IsLeftPinching) && !IsBothHandsPinching;
    }

}
