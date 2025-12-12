using UnityEngine;

public class VisualGainHand : PositionRotationProvider
{
    public override Vector3 GetPositionOutput(Vector3 currentPosition)
    {
        UpdateDataSource();

        currentPosition += HandData.GetDeltaHandPosition() * GetVisualGain(currentPosition);

        return currentPosition;
    }

    public override Quaternion GetRotationOutput(Quaternion currentRotation)
    {
        UpdateDataSource();

        return HandData.GetDeltaHandRotation() * currentRotation;
    }
}
