using UnityEngine;

public class HeadRST_semi_momentum : ManipulationTechnique
{

    private bool _isHandMovingOutwards;
    private Vector3 _handPosition;
    private float _depthGain;

    private Vector3 _accumulatedHandOffset;

    public override void Apply(Transform target)
    {
        EyeGaze eyeGaze = EyeGaze.GetInstance();
        HeadMovement head = HeadMovement.GetInstance();

        Vector3 gazeOrigin = eyeGaze.GetGazeRay().origin;
        Vector3 gazeDirection = eyeGaze.GetGazeRay().direction;

        Quaternion deltaRot = HandPosition.GetInstance().GetDeltaHandRotation();
        target.rotation = deltaRot * target.rotation;

        Vector3 deltaHandPos = HandPosition.GetInstance().GetDeltaHandPosition(usePinchTip: true);

        bool _isHandMovingOutwards = Vector3.Dot(deltaHandPos, gazeDirection) > 0;
        bool _isHeadTiltingUpwards = head.DeltaHeadY > 0;

        _depthGain = 1;

        if (_isHandMovingOutwards && _isHeadTiltingUpwards && head.HeadSpeed >= 0.2f)
        {
            _depthGain = 10;
        }

        if (!_isHandMovingOutwards && !_isHeadTiltingUpwards && head.HeadSpeed >= 0.2f)
        {
            _depthGain = 10f;
        }

        target.position += deltaHandPos * _depthGain;
        _accumulatedHandOffset += deltaHandPos;

        target.position = gazeOrigin + gazeDirection * Vector3.Distance(gazeOrigin, target.position) + _accumulatedHandOffset;

        //TODO: implement instant stop
        //TODO: hand is not refiement now

        if (eyeGaze.IsSaccading())
        {
            _accumulatedHandOffset = Vector3.zero;
        }


    }
    


}