using UnityEngine;

public interface IObjectPositionRotationProvider
{
    public Vector3 GetPositionOutput(Vector3 currentPosition, Handedness_v inputHand);
    public Quaternion GetRotationOutput(Quaternion currentRotation, Handedness_v inputHand);
}

