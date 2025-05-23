using UnityEngine;

public class ManipulatableObject : MonoBehaviour
{

    private bool _isHitbyGaze;
    private bool _isGrabed;

    void Update()
    {
        _isHitbyGaze = EyeGaze.GetInstance().GetGazeHitTrans() == transform;

    
        if (_isHitbyGaze && PinchDetector.GetInstance().IsPinching)
        {
            _isGrabed = true;
        }

        if (!PinchDetector.GetInstance().IsPinching)
        {
            _isGrabed = false;
        }
    
        if(_isGrabed && PinchDetector.GetInstance().IsLeftPinching)
        {
            transform.position += HandPosition.GetInstance().LeftHandPosition_delta;
            transform.rotation = HandPosition.GetInstance().LeftHandRotation_delta * transform.rotation;
        }
        else if(_isGrabed && PinchDetector.GetInstance().IsRightPinching)
        {
            transform.position += HandPosition.GetInstance().RightHandPosition_delta;
            transform.rotation = HandPosition.GetInstance().RightHandRotation_delta * transform.rotation;
        }



        // transform.GetComponent<Outline>().enabled = _isHitbyGaze;
    }
    
}
