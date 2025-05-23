using UnityEngine;

public class PinchDetector : Singleton<PinchDetector>
{
    public OVRHand RightHand, LeftHand;
    public bool IsPinching, IsRightPinching, IsLeftPinching;

    void Update()
    {
        IsRightPinching = RightHand.GetFingerIsPinching(OVRHand.HandFinger.Index);
        IsLeftPinching = LeftHand.GetFingerIsPinching(OVRHand.HandFinger.Index);
        IsPinching = IsRightPinching || IsLeftPinching;
    }

}
