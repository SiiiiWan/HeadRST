using UnityEngine;

public class HandDataVisualizer : MonoBehaviour
{
    Linescript rightHandLine;
    Linescript leftHandLine;

    private void Awake() 
    {
        rightHandLine = new Linescript(0.01f, transform);
        leftHandLine = new Linescript(0.01f, transform);
    }
    
    void Update()
    {
        HandData handData = HandData.GetInstance();

        rightHandLine.SetPosition(handData.RightHandPosition, handData.RightHandPosition + handData.RightHandDirection * 0.2f);
        leftHandLine.SetPosition(handData.LeftHandPosition, handData.LeftHandPosition + handData.LeftHandDirection * 0.2f);
    }
}
