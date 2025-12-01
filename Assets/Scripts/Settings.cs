using UnityEngine;


public enum DominantHand
{
    right,
    left
}
public class Settings : Singleton<Settings>
{
    public DominantHand DominantHand = DominantHand.right;
    public ManipulationTechnique ManipulationBehavior;


    public Vector3 GetVirtualHandPosition(bool isRightHand)
    {
        if (isRightHand)
        {
            if (DominantHand == DominantHand.right)
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
            if (DominantHand == DominantHand.left)
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
