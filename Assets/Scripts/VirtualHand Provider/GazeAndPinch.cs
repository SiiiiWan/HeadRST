using UnityEngine;

public class GazeAndPinch : VirtualHandProvider, IObjectPositionRotationProvider
{
    public Vector3 GetPositionOutput(Vector3 currentPosition, Handedness_v inputHand)
    {
        Vector3 handPosition = inputHand == Handedness_v.Right ? HandData.GetInstance().RightPinchTipPosition : HandData.GetInstance().LeftPinchTipPosition;
        Vector3 handPosition_delta = inputHand == Handedness_v.Right ? HandData.GetInstance().RightPinchTipPosition_delta : HandData.GetInstance().LeftPinchTipPosition_delta;
        Vector3 gazeOrigin = EyeGaze.GetInstance().GetGazeRay().origin;

        float visualGain = Mathf.Max(1, Vector3.Distance(gazeOrigin, currentPosition) / Vector3.Distance(gazeOrigin, handPosition));
        currentPosition += visualGain * handPosition_delta;
        return currentPosition;
    }

    public Quaternion GetRotationOutput(Quaternion currentRotation, Handedness_v inputHand)
    {
        currentRotation = inputHand == Handedness_v.Right ? HandData.GetInstance().RightPinchTipRotation_delta * currentRotation : HandData.GetInstance().LeftPinchTipRotation_delta * currentRotation;
        return currentRotation;
    }

    public override void UpdateVirtualHandPoses()
    {
        ObjectManager objectManager = ObjectManager.GetInstance();
        objectManager.ManipulationMode = ManipulationMode.Indirect;
        objectManager.ObjectPositionRotationProvider = this;

        VirtualHandPoses = new VirtualDoubleHandPoses(
            new Pose(HandData.GetInstance().LeftHandPosition, HandData.GetInstance().LeftHandRotation),
            new Pose(HandData.GetInstance().RightHandPosition, HandData.GetInstance().RightHandRotation)
        );
    }
}