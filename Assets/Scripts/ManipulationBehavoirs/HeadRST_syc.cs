using UnityEngine;
using UnityEngine.Assertions.Comparers;

public class HeadRST_sync : ManipulationTechnique
{

    public bool HeadBoost = true;

    private float _depthGain;
    private float _depthOffset;

    private Vector3 _accumulatedHandOffset;

    public override void OnGrabbed(Transform target)
    {
        _depthOffset = Vector3.Distance(EyeGaze.GetInstance().GetGazeRay().origin, target.position);
        // _accumulatedHandOffset = Vector3.zero;
    }

    public override void ApplyGrabbedBehaviour(Transform target)
    {
        EyeGaze eyeGaze = EyeGaze.GetInstance();
        HeadMovement head = HeadMovement.GetInstance();

        Vector3 gazeOrigin = eyeGaze.GetGazeRay().origin;
        Vector3 gazeDirection = eyeGaze.GetGazeRay().direction;

        Quaternion deltaRot = HandPosition.GetInstance().GetDeltaHandRotation(usePinchTip: true);
        target.rotation = deltaRot * target.rotation;

        Vector3 deltaHandPos = HandPosition.GetInstance().GetDeltaHandPosition(usePinchTip: true);

        bool _isHandMovingOutwards = Vector3.Dot(deltaHandPos, gazeDirection) > 0;
        bool _isHeadTiltingUpwards = head.DeltaHeadY > 0;

        _depthGain = 1;

        if (_isHandMovingOutwards && _isHeadTiltingUpwards && head.HeadSpeed >= 0.1f)
        {
            _depthGain = 10;
        }

        if (!_isHandMovingOutwards && !_isHeadTiltingUpwards && head.HeadSpeed >= 0.1f)
        {
            _depthGain = 10f;
        }


        // target.position += deltaHandPos;

        // target.position = gazeOrigin + gazeDirection * Vector3.Distance(gazeOrigin, target.position);

        if(HeadBoost) _depthOffset += Vector3.Dot(deltaHandPos, gazeDirection) * _depthGain;

        _accumulatedHandOffset += deltaHandPos;

        if (eyeGaze.IsSaccading())
        {
            _accumulatedHandOffset = Vector3.zero;
        }

        target.position = gazeOrigin + gazeDirection * Mathf.Clamp(_depthOffset, 1f, 10f) + _accumulatedHandOffset;

    }
    


}