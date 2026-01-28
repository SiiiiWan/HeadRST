using UnityEngine;

public class GazeAndPinch : VirtualHandProvider
{

    public override void UpdateVirtualHandPoses()
    {
        ObjectManager objectManager = ObjectManager.GetInstance();
        objectManager.AllowDirectGrab = false;
        objectManager.AllowIndirectGrab = true;

        VirtualHandPoses = new VirtualDoubleHandPoses(
            new Pose(HandData.GetInstance().LeftHandPosition, HandData.GetInstance().LeftHandRotation),
            new Pose(HandData.GetInstance().RightHandPosition, HandData.GetInstance().RightHandRotation)
        );
    }
}