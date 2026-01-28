using UnityEngine;

public class GoGo : VirtualHandProvider
{
    [Range(0f, 1f)] public float D = 0.475f;
    [Range(0f, 1f)] public float k = 1/6f;
    
    Vector3 torsoOffset = new Vector3(0, -0.35f, 0);
    // public override Vector3 UpdatePivot(Vector3 currentPosition)
    // {
    //     UpdateDataSource();

    //     Vector3 Rr = HandData.RightHandPosition - (HeadData.HeadPosition + torsoOffset);
        
    //     if(Rr.magnitude >= D)
    //     {
    //         return HandData.RightHandPosition + Rr.normalized * Mathf.Pow((Rr.magnitude - D) * 100, 2) * k;
    //         // return HandData.RightHandPosition + Rr.normalized * 3;

    //     }
    //     else
    //     {
    //         return HandData.RightHandPosition;
    //     }
    // }

}
