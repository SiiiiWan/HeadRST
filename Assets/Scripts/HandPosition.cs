using UnityEngine;

public class HandPosition : Singleton<HandPosition>
{
    public Transform RightHandAnchor, LeftHandAnchor;
    public Vector3 RightHandPosition, LeftHandPosition;
    public Quaternion RightHandRotation, LeftHandRotation;

    public Vector3 RightHandPosition_delta, LeftHandPosition_delta;
    public Quaternion RightHandRotation_delta, LeftHandRotation_delta;

    void Update()
    {
        RightHandPosition_delta = RightHandAnchor.position - RightHandPosition;
        LeftHandPosition_delta = LeftHandAnchor.position - LeftHandPosition;

        RightHandRotation_delta = RightHandAnchor.rotation * Quaternion.Inverse(RightHandRotation);
        LeftHandRotation_delta = LeftHandAnchor.rotation * Quaternion.Inverse(LeftHandRotation);

        RightHandPosition = RightHandAnchor.position;
        LeftHandPosition = LeftHandAnchor.position;

        RightHandRotation = RightHandAnchor.rotation;
        LeftHandRotation = LeftHandAnchor.rotation;
    }



}
