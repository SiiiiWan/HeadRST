using UnityEngine;


public enum ManipulationBehaviorNames
{
    OneToOne,
    ImplicitGaze
}

public static class ManipulationBehaviors
{
    public static void OneToOne(Transform target)
    {
        Vector3 deltaPos = PinchDetector.GetInstance().IsLeftPinching ? HandPosition.GetInstance().LeftHandPosition_delta : HandPosition.GetInstance().RightHandPosition_delta;
        Quaternion deltaRot = PinchDetector.GetInstance().IsLeftPinching ? HandPosition.GetInstance().LeftHandRotation_delta : HandPosition.GetInstance().RightHandRotation_delta;

        target.position += deltaPos;
        target.rotation = deltaRot * target.rotation;
    }

    public static void ImplicitGaze(Transform target)
    {

        // target.position = Vector3.Lerp(target.position, target.position + deltaPos, 0.5f);
        // target.rotation = Quaternion.Slerp(target.rotation, deltaRot * target.rotation, 0.5f);
    }

    // public static void PositionOnly(Transform target, Vector3 deltaPos, Quaternion deltaRot)
    // {
    //     target.position += deltaPos;
    // }
}