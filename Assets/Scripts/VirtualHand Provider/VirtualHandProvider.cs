using UnityEngine;

public class VirtualHandProvider : MonoBehaviour
{
    public EyeGaze GazeData { get; private set; }
    public HandData HandData { get; private set; }
    public HeadMovement HeadData { get; private set; }
    public PinchDetector PinchDetector { get; private set; }

    protected Vector3 _pivot_redirected;

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

    public virtual Pose GetVirtualHandPose(bool isRightHand)
    {
        _pivot_redirected = UpdatePivot(_pivot_redirected);
        
        if(isRightHand)
        {
            return new Pose(HandData.GetInstance().RightHandPosition, HandData.GetInstance().RightHandRotation);
        }
        else
        {
            return new Pose(HandData.GetInstance().LeftHandPosition, HandData.GetInstance().LeftHandRotation);
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

