using UnityEngine;

public class VirtualDoubleHandPoses
{
    public Pose LeftHandPose;
    public Pose RightHandPose;

    public VirtualDoubleHandPoses(Pose leftHandPose, Pose rightHandPose)
    {
        LeftHandPose = leftHandPose;
        RightHandPose = rightHandPose;
    }

    public Pose GetVirtualHandPose(Handedness_v handedness)
    {
        if(handedness == Handedness_v.Right)
        {
            return RightHandPose;
        }
        else
        {
            return LeftHandPose;
        }
    }
}

public class VirtualHandProvider : MonoBehaviour
{
    public EyeGaze GazeData { get; private set; }
    public HandData HandData { get; private set; }
    public HeadMovement HeadData { get; private set; }
    public OVRBody BodyData { get; private set; }
    public PinchDetector PinchDetector { get; private set; }

    public VirtualDoubleHandPoses VirtualHandPoses;

    public void UpdateDataSource()
    {
        GazeData = EyeGaze.GetInstance();
        HeadData = HeadMovement.GetInstance();
        HandData = HandData.GetInstance();
        BodyData = Settings.GetInstance().BodyTracking;
        PinchDetector = PinchDetector.GetInstance();
    }

    public virtual void UpdateVirtualHandPoses()
    {
        VirtualHandPoses = new VirtualDoubleHandPoses(
            new Pose(HandData.GetInstance().LeftHandPosition, HandData.GetInstance().LeftHandRotation),
            new Pose(HandData.GetInstance().RightHandPosition, HandData.GetInstance().RightHandRotation)
        );
    }

    public virtual float GetVirtualHandScalingFactor()
    {
        return 1.0f;
    }

    public virtual bool IsGazeRedirecting()
    {
        return false;
    }
}

