using UnityEngine;

public enum PinchState
{
    NotPinching,
    OneHandPinching,
    BothHandsPinching
}

public class PinchDetector : Singleton<PinchDetector>
{
    public OVRHand RightHand, LeftHand;
    public bool IsRightPinching, IsLeftPinching;
    public bool IsBothHandsPinching, IsOneHandPinching, IsNoHandPinching;
    public PinchState PinchState = PinchState.NotPinching;

    void Update()
    {
        IsRightPinching = RightHand.GetFingerIsPinching(OVRHand.HandFinger.Index);
        IsLeftPinching = LeftHand.GetFingerIsPinching(OVRHand.HandFinger.Index);

        if (IsRightPinching && IsLeftPinching)
        {
            PinchState = PinchState.BothHandsPinching;
        }
        else if (IsRightPinching || IsLeftPinching)
        {
            PinchState = PinchState.OneHandPinching;
        }
        else
        {
            PinchState = PinchState.NotPinching;
        }

        IsBothHandsPinching = PinchState == PinchState.BothHandsPinching;
        IsOneHandPinching = PinchState == PinchState.OneHandPinching;
        IsNoHandPinching = PinchState == PinchState.NotPinching;
    }

}
