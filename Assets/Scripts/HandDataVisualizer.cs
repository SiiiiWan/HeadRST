using UnityEngine;

public class HandDataVisualizer : MonoBehaviour
{
    Linescript rightHandLine;
    Linescript leftHandLine;
    
    void Update()
    {
        HandData handData = HandData.GetInstance();

        if(rightHandLine == null) rightHandLine = new Linescript(0.01f, transform);
        if(leftHandLine == null) leftHandLine = new Linescript(0.01f, transform);
        rightHandLine.SetPosition(handData.RightHandPosition, handData.RightHandPosition + handData.RightHandDirection * 0.2f);
        leftHandLine.SetPosition(handData.LeftHandPosition, handData.LeftHandPosition + handData.LeftHandDirection * 0.2f);
    }
}
