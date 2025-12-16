using UnityEngine;

public class VisualGainHand : PositionRotationProvider
{
    public override Vector3 GetPositionOutput(Vector3 currentPosition)
    {
        UpdateDataSource();

        currentPosition += HandData.GetDeltaHandPosition() * GetVisualGain(currentPosition);

        return currentPosition;
    }

}
