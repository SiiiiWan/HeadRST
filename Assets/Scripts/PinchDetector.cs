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

    public bool IsBothHandsPinching, IsOneHandPinching, IsNoHandPinching, IsNoHandPinching_LastFrame, IsOneHandPinching_LastFrame, IsBothHandsPinching_LastFrame;
    public PinchState PinchState = PinchState.NotPinching;
    public float PinchThreshold = 0.01f; // Adjust this threshold as needed

    public GameObject righHandPinchBall_index, righHandPinchBall_thumb, leftHandPinchBall_index, leftHandPinchBall_thumb, lefthandPinchBall_hand, righthandPinchBall_hand;

    public delegate void PinchAction();
    public static event PinchAction OnRightHandPinch_Close, OnRightHandPinch_Release, OnLeftHandPinch_Close, OnLeftHandPinch_Release;

    private bool _isRightPinching;
    public bool IsRightPinching
    {
        get { return _isRightPinching; }
        set 
        {
            if(value != _isRightPinching)
            {
                _isRightPinching = value;
                if(_isRightPinching)
                {
                    OnRightHandPinch_Close?.Invoke();
                }
                else
                {
                    OnRightHandPinch_Release?.Invoke();
                }
            }
        }
    }
    
    private bool _isLeftPinching;
    public bool IsLeftPinching
    {
        get { return _isLeftPinching; }
        set 
        {
            if(value != _isLeftPinching)
            {
                _isLeftPinching = value;
                if(_isLeftPinching)
                {
                    OnLeftHandPinch_Close?.Invoke();
                }
                else
                {
                    OnLeftHandPinch_Release?.Invoke();
                }
            }
        }
    }


    void Update()
    {
        IsNoHandPinching_LastFrame = IsNoHandPinching;
        IsOneHandPinching_LastFrame = IsOneHandPinching;
        IsBothHandsPinching_LastFrame = IsBothHandsPinching;
        
        UpdatePinchBalls();

        // IsRightPinching = RightHand.GetFingerIsPinching(OVRHand.HandFinger.Index);
        // IsLeftPinching = LeftHand.GetFingerIsPinching(OVRHand.HandFinger.Index);

        IsRightPinching = Vector3.Distance(righHandPinchBall_thumb.transform.position, righHandPinchBall_index.transform.position) < PinchThreshold; // Adjust threshold as needed
        IsLeftPinching = Vector3.Distance(leftHandPinchBall_thumb.transform.position, leftHandPinchBall_index.transform.position) < PinchThreshold; // Adjust threshold as needed

        if(IsRightPinching && IsLeftPinching)
        {
            PinchState = PinchState.BothHandsPinching;
        }
        else if(IsRightPinching || IsLeftPinching)
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
