using UnityEngine;

public enum PinchState
{
    NotPinchingOrGrabbing,
    OneHandPinching,
    BothHandsPinching,
    OneHandGrabbing,
    BothHandsGrabbing
}

public class PinchDetector : Singleton<PinchDetector>
{
    public OVRHand RightHand, LeftHand;
    public PinchState PinchState = PinchState.NotPinchingOrGrabbing;
    public bool IsRightPinching, IsLeftPinching, IsRightGrabbing, IsLeftGrabbing;
    public bool IsNoHandPinchingOrGrabbing_LastFrame { get; private set; }

    [Header("Settings and Bindings")]
    public float PinchThreshold = 0.015f;
    public float GrabThreshold = 0.05f;
    public GameObject righ_index, righ_thumb, right_mid, right_ring, right_palm, left_index, left_thumb, left_mid, left_ring, left_palm;

    void Update()
    {
        UpdatePinchBalls();

        IsRightPinching = GetDistance(righ_thumb, righ_index) < PinchThreshold;
        IsLeftPinching = GetDistance(left_thumb, left_index) < PinchThreshold;

        IsRightGrabbing = (GetDistance(right_mid, right_palm) < GrabThreshold) & (GetDistance(right_ring, right_palm) < GrabThreshold);
        IsLeftGrabbing = (GetDistance(left_mid, left_palm) < GrabThreshold) & (GetDistance(left_ring, left_palm) < GrabThreshold);

        PinchState = PinchState.NotPinchingOrGrabbing;

        if (IsRightPinching || IsLeftPinching)
        {
            PinchState = PinchState.OneHandPinching;

            if (IsRightPinching && IsLeftPinching)
                PinchState = PinchState.BothHandsPinching;
        }

        if (IsRightGrabbing || IsLeftGrabbing)
        {
            PinchState = PinchState.OneHandGrabbing;

            if (IsRightGrabbing && IsLeftGrabbing)
                PinchState = PinchState.BothHandsGrabbing;
        }

        // IsNoHandPinchingOrGrabbing_LastFrame = PinchState == PinchState.NotPinchingOrGrabbing;
    }

    private float GetDistance(GameObject obj1, GameObject obj2)
    {
        return Vector3.Distance(obj1.transform.position, obj2.transform.position);
    }

    private void UpdatePinchBalls()
    {
        if (RightHand == null) return;
        var skeleton = RightHand.GetComponent<OVRSkeleton>();
        if (skeleton == null || skeleton.Bones == null) return;

        foreach (var bone in skeleton.Bones)
        {
            if (bone.Id == OVRSkeleton.BoneId.XRHand_IndexTip) // Use Hand_IndexTip for OVR
                righ_index.transform.position = bone.Transform.position;
            if (bone.Id == OVRSkeleton.BoneId.XRHand_ThumbTip) // Use Hand_ThumbTip for OVR
                righ_thumb.transform.position = bone.Transform.position;
            if (bone.Id == OVRSkeleton.BoneId.XRHand_MiddleTip) // Use Hand_MiddleTip for OVR
                right_mid.transform.position = bone.Transform.position;
            if (bone.Id == OVRSkeleton.BoneId.XRHand_RingTip) // Use Hand_RingTip for OVR
                right_ring.transform.position = bone.Transform.position;
            if (bone.Id == OVRSkeleton.BoneId.XRHand_Palm) // Use Hand_Palm for OVR
                right_palm.transform.position = bone.Transform.position;
        }

        if (LeftHand == null) return;
        skeleton = LeftHand.GetComponent<OVRSkeleton>();
        if (skeleton == null || skeleton.Bones == null) return;
        foreach (var bone in skeleton.Bones)
        {
            if (bone.Id == OVRSkeleton.BoneId.XRHand_IndexTip) // Use Hand_IndexTip for OVR
                left_index.transform.position = bone.Transform.position;
            if (bone.Id == OVRSkeleton.BoneId.XRHand_ThumbTip) // Use Hand_ThumbTip for OVR
                left_thumb.transform.position = bone.Transform.position;
            if (bone.Id == OVRSkeleton.BoneId.XRHand_MiddleTip) // Use Hand_MiddleTip for OVR
                left_mid.transform.position = bone.Transform.position;
            if (bone.Id == OVRSkeleton.BoneId.XRHand_RingTip) // Use Hand_RingTip for OVR
                left_ring.transform.position = bone.Transform.position;
            if (bone.Id == OVRSkeleton.BoneId.XRHand_Palm) // Use Hand_Palm for OVR
                left_palm.transform.position = bone.Transform.position;
        }
    }

    public bool IsNoHandPinchingOrGrabbing
    {
        get { return PinchState == PinchState.NotPinchingOrGrabbing; }
    }

    public bool IsOneHandPinching
    {
        get { return PinchState == PinchState.OneHandPinching; }
    }

    public bool IsOneHandGrabbing
    {
        get { return PinchState == PinchState.OneHandGrabbing; }
    }

    public bool IsBothHandsPinching
    {
        get { return PinchState == PinchState.BothHandsPinching; }
    }

    public bool IsBothHandsGrabbing
    {
        get { return PinchState == PinchState.BothHandsGrabbing; }
    }

}
