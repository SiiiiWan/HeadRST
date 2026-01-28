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
    public bool IsBothHandsPinching, IsOneHandPinching, IsNoHandPinching, IsNoHandPinching_LastFrame, IsOneHandPinching_LastFrame;
    public PinchState PinchState = PinchState.NotPinching;
    public float PinchThreshold = 0.01f; // Adjust this threshold as needed

    public GameObject righHandPinchBall_index, righHandPinchBall_thumb, leftHandPinchBall_index, leftHandPinchBall_thumb, lefthandPinchBall_hand, righthandPinchBall_hand;
    void Update()
    {
        UpdatePinchBalls();

        // IsRightPinching = RightHand.GetFingerIsPinching(OVRHand.HandFinger.Index);
        // IsLeftPinching = LeftHand.GetFingerIsPinching(OVRHand.HandFinger.Index);

        IsRightPinching = Vector3.Distance(righHandPinchBall_thumb.transform.position, righHandPinchBall_index.transform.position) < PinchThreshold; // Adjust threshold as needed
        IsLeftPinching = Vector3.Distance(leftHandPinchBall_thumb.transform.position, leftHandPinchBall_index.transform.position) < PinchThreshold; // Adjust threshold as needed

        Settings settings = Settings.GetInstance();

        if (settings.DominantHand == Handedness_v.Right)
        {
            if (IsRightPinching)
            {
                PinchState = PinchState.OneHandPinching;
            }
            else
            {
                PinchState = PinchState.NotPinching;
            }
        }
        else
        {
            if (IsLeftPinching)
            {
                PinchState = PinchState.OneHandPinching;
            }
            else
            {
                PinchState = PinchState.NotPinching;
            }
        }

        IsBothHandsPinching = PinchState == PinchState.BothHandsPinching;
        IsOneHandPinching = PinchState == PinchState.OneHandPinching;

        IsNoHandPinching_LastFrame = IsNoHandPinching;
        IsOneHandPinching_LastFrame = IsOneHandPinching;
        IsNoHandPinching = PinchState == PinchState.NotPinching;
    }

    private void UpdatePinchBalls()
    {
        if (RightHand == null) return;
        var skeleton = RightHand.GetComponent<OVRSkeleton>();
        if (skeleton == null || skeleton.Bones == null) return;

        foreach (var bone in skeleton.Bones)
        {
            if (bone.Id == OVRSkeleton.BoneId.XRHand_IndexTip)
            {
                righHandPinchBall_index.transform.position = bone.Transform.position;
                righHandPinchBall_index.transform.rotation = bone.Transform.rotation;
            }
            if (bone.Id == OVRSkeleton.BoneId.XRHand_ThumbTip)
            {
                righHandPinchBall_thumb.transform.position = bone.Transform.position;
                righHandPinchBall_thumb.transform.rotation = bone.Transform.rotation;
            }
        }

        if (LeftHand == null) return;
        skeleton = LeftHand.GetComponent<OVRSkeleton>();
        if (skeleton == null || skeleton.Bones == null) return;
        foreach (var bone in skeleton.Bones)
        {
            if (bone.Id == OVRSkeleton.BoneId.XRHand_IndexTip)
            {
                leftHandPinchBall_index.transform.position = bone.Transform.position;
                leftHandPinchBall_index.transform.rotation = bone.Transform.rotation;
            }
            if (bone.Id == OVRSkeleton.BoneId.XRHand_ThumbTip) // Use Hand_ThumbTip for OVR
            {
                leftHandPinchBall_thumb.transform.position = bone.Transform.position;
                leftHandPinchBall_thumb.transform.rotation = bone.Transform.rotation;
            }
        }

        lefthandPinchBall_hand.transform.position = HandData.GetInstance().LeftHandPosition;
        righthandPinchBall_hand.transform.position = HandData.GetInstance().RightHandPosition;
    }

}
