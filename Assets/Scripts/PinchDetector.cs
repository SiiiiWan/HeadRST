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
    public bool IsBothHandsPinching, IsOneHandPinching, IsNoHandPinching, IsNoHandPinching_LastFrame;
    public PinchState PinchState = PinchState.NotPinching;
    public float PinchThreshold = 0.01f;

    public GameObject righHandPinchBall_index, righHandPinchBall_thumb, leftHandPinchBall_index, leftHandPinchBall_thumb;
    void Update()
    {
        UpdatePinchBalls();

        IsRightPinching = Vector3.Distance(righHandPinchBall_thumb.transform.position, righHandPinchBall_index.transform.position) < PinchThreshold;
        IsLeftPinching = Vector3.Distance(leftHandPinchBall_thumb.transform.position, leftHandPinchBall_index.transform.position) < PinchThreshold;

        StudyControl studyControl = StudyControl.GetInstance();

        if (studyControl.DominantHand == Handedness.right)
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
                righHandPinchBall_index.transform.position = bone.Transform.position;
            if (bone.Id == OVRSkeleton.BoneId.XRHand_ThumbTip)
                righHandPinchBall_thumb.transform.position = bone.Transform.position;
        }

        if (LeftHand == null) return;
        skeleton = LeftHand.GetComponent<OVRSkeleton>();
        if (skeleton == null || skeleton.Bones == null) return;
        foreach (var bone in skeleton.Bones)
        {
            if (bone.Id == OVRSkeleton.BoneId.XRHand_IndexTip)
                leftHandPinchBall_index.transform.position = bone.Transform.position;
            if (bone.Id == OVRSkeleton.BoneId.XRHand_ThumbTip)
                leftHandPinchBall_thumb.transform.position = bone.Transform.position;
        }
    }

}
