using UnityEngine;

public enum Handedness { left, right }

public class StudyControl : Singleton<StudyControl>
{

    public Handedness DominantHand { get; private set; } = Handedness.right;
    public ManipulationTechnique ManipulationBehavior;

    public Vector3 GetVirtualHandPosition(bool isRightHand)
    {
        PinchDetector pinchDetector = PinchDetector.GetInstance();

        if (pinchDetector.IsOneHandPinching)
        {
            if (pinchDetector.IsRightPinching)
                DominantHand = Handedness.right;
            else
                DominantHand = Handedness.left;
        }

        if (isRightHand)
            {
                if (DominantHand == Handedness.right)
                {
                    return ManipulationBehavior.VirtualHandPosition;
                }
                else
                {
                    return HandData.GetInstance().RightHandPosition;
                }
            }
            else
            {
                if (DominantHand == Handedness.left)
                {
                    return ManipulationBehavior.VirtualHandPosition;
                }
                else
                {
                    return HandData.GetInstance().LeftHandPosition;
                }
            }
    }
    
}