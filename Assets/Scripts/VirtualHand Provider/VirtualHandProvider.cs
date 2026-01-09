using UnityEngine;

public class VirtualHandProvider : MonoBehaviour
{
    public EyeGaze GazeData { get; private set; }
    public HandData HandData { get; private set; }
    public HeadMovement HeadData { get; private set; }
    public PinchDetector PinchDetector { get; private set; }

    protected Vector3 _currentPivotPoint;

    public void UpdateDataSource()
    {
        GazeData = EyeGaze.GetInstance();
        HeadData = HeadMovement.GetInstance();
        HandData = HandData.GetInstance();
        PinchDetector = PinchDetector.GetInstance();
    }

    public virtual Vector3 UpdatePivot(Vector3 currentPivot)
    {
        return currentPivot;
    }

    public virtual Vector3 GetVirtualHandPosition(bool isRightHand)
    {
        _currentPivotPoint = UpdatePivot(_currentPivotPoint);
        
        if(isRightHand)
        {
            return HandData.GetInstance().RightHandPosition;
        }
        else
        {
            return HandData.GetInstance().LeftHandPosition;
        }
    }

    public virtual Quaternion GetVirtualHandRotation(bool isRightHand)
    {
        if(isRightHand)
        {
            return HandData.GetInstance().RightHandRotation;
        }
        else
        {
            return HandData.GetInstance().LeftHandRotation;
        }
    }

    public float GetVisualGain(Vector3 objectPosition)
    {
        UpdateDataSource();
        return Mathf.Max(1f, Vector3.Distance(objectPosition, GazeData.GetGazeOrigin()) / Vector3.Distance(HandData.GetHandPosition(), GazeData.GetGazeOrigin()));
        // TODO: Get hand position here use pinch 
    }

    public virtual bool IsGazeRedirecting()
    {
        return false;
    }

}

