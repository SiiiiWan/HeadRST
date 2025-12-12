using UnityEngine;

public class PositionRotationProvider : MonoBehaviour
{
    public EyeGaze GazeData { get; private set; }
    public HandData HandData { get; private set; }
    public HeadMovement HeadData { get; private set; }
    public PinchDetector PinchDetector { get; private set; }


    public void UpdateDataSource()
    {
        GazeData = EyeGaze.GetInstance();
        HeadData = HeadMovement.GetInstance();
        HandData = HandData.GetInstance();
        PinchDetector = PinchDetector.GetInstance();
    }

    public virtual Vector3 GetPositionOutput(Vector3 currentPosition)
    {
        return currentPosition;
    }

    public virtual Quaternion GetRotationOutput(Quaternion currentRotation)
    {
        return currentRotation;
    }

    public float GetVisualGain(Vector3 objectPosition)
    {
        UpdateDataSource();
        return Mathf.Max(1f, Vector3.Distance(objectPosition, GazeData.GetGazeOrigin()) / Vector3.Distance(HandData.GetHandPosition(), GazeData.GetGazeOrigin()));
    }
}
