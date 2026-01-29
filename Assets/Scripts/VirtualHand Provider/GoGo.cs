using UnityEngine;
using static OVRPlugin;

public class GoGo : VirtualHandProvider
{
    [Range(0f, 1f)] public float D = 0.475f;
    [Range(0f, 1f)] public float k = 1/6f;


    public override void UpdateVirtualHandPoses()
    {
        ObjectManager objectManager = ObjectManager.GetInstance();
        objectManager.AllowDirectGrab = true;
        objectManager.AllowIndirectGrab = false;

        // Get Tracking Data
        UpdateDataSource();

        // // Get Hand Data
        Vector3 rightHandPosition = HandData.RightHandPosition;
        Vector3 leftHandPosition = HandData.LeftHandPosition;

        // // Get Torso Data
        Posef torsoPose = BodyData.BodyState?.JointLocations[5].Pose ?? default;
        Vector3 torsoPosition = new Vector3(torsoPose.Position.x, torsoPose.Position.y, -torsoPose.Position.z);

        Vector3 Rr = rightHandPosition - torsoPosition;
        Vector3 Lr = leftHandPosition - torsoPosition;

        Vector3 rightVirtualHandPosition = rightHandPosition;
        Vector3 leftVirtualHandPosition = leftHandPosition;

        if(Rr.magnitude >= D)
        {
            rightVirtualHandPosition = rightHandPosition + Rr.normalized * Mathf.Pow((Rr.magnitude - D) * 100, 2) * k;
        }

        if(Lr.magnitude >= D)
        {
            leftVirtualHandPosition = leftHandPosition + Lr.normalized * Mathf.Pow((Lr.magnitude - D) * 100, 2) * k;
        }

        VirtualHandPoses = new VirtualDoubleHandPoses(
            new Pose(leftVirtualHandPosition, HandData.LeftHandRotation),
            new Pose(rightVirtualHandPosition, HandData.RightHandRotation)
        );
    }

}
