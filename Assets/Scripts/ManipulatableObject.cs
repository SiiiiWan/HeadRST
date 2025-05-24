using UnityEngine;
using UnityEngine.InputSystem.Controls;

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
    
        if(_isGrabed && PinchDetector.GetInstance().IsPinching)
        {
            Vector3 deltaPos = PinchDetector.GetInstance().IsLeftPinching ? HandPosition.GetInstance().LeftHandPosition_delta : HandPosition.GetInstance().RightHandPosition_delta;
            Quaternion deltaRot = PinchDetector.GetInstance().IsLeftPinching ? HandPosition.GetInstance().LeftHandRotation_delta : HandPosition.GetInstance().RightHandRotation_delta;

            transform.position += deltaPos;
            transform.rotation = deltaRot * transform.rotation;
        }



        // transform.GetComponent<Outline>().enabled = _isHitbyGaze;
    }
    
    

}
