using UnityEngine;

public enum Handedness { left, right }

public class StudyControl : Singleton<StudyControl>
{

    public Handedness DominantHand = Handedness.right;
    public ManipulationTechnique ManipulationBehavior;

    public Vector3 GetVirtualHandPosition(bool isRightHand)
    {
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