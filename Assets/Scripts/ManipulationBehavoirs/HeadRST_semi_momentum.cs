using UnityEngine;

public enum DriftDirection {forward, backward}

public class HeadRST_semi_momentum : ManipulationTechnique
{

    private float _depthGain;
    private float _depthOffset;

    private Vector3 _accumulatedHandOffset;
    private bool _isDrifting;
    private DriftDirection _registeredDriftDirection;

    public override void OnSingleHandGrabbed(Transform target)
    {
        _depthOffset = Vector3.Distance(EyeGaze.GetInstance().GetGazeRay().origin, target.position);
        // _accumulatedHandOffset = Vector3.zero;
    }

    public override void ApplySingleHandGrabbedBehaviour(Transform target)
    {

        EyeGaze eyeGaze = EyeGaze.GetInstance();
        HeadMovement head = HeadMovement.GetInstance();
        HandPosition hand = HandPosition.GetInstance();

        Vector3 gazeOrigin = eyeGaze.GetGazeRay().origin;
        Vector3 gazeDirection = eyeGaze.GetGazeRay().direction;

        Quaternion deltaRot = hand.GetDeltaHandRotation(usePinchTip: true);
        target.rotation = deltaRot * target.rotation;

        Vector3 deltaHandPos = hand.GetDeltaHandPosition(usePinchTip: true);

        float realTimeDist = Vector3.Distance(EyeGaze.GetInstance().GetGazeRay().origin, target.position);

        if (hand.GetHandSpeed() > 1)
        {
            if (_isDrifting == false)
            {
                _registeredDriftDirection = Vector3.Dot(deltaHandPos, gazeDirection) > 0 ? DriftDirection.forward : DriftDirection.backward;
                _isDrifting = true;
            }
            else
            {
                if (_registeredDriftDirection == DriftDirection.forward && Vector3.Dot(deltaHandPos, gazeDirection) < 0)
                {
                    _isDrifting = false;
                    _depthOffset = Vector3.Distance(EyeGaze.GetInstance().GetGazeRay().origin, target.position);

                }

                if (_registeredDriftDirection == DriftDirection.backward && Vector3.Dot(deltaHandPos, gazeDirection) > 0)
                {
                    _isDrifting = false;
                    _depthOffset = Vector3.Distance(EyeGaze.GetInstance().GetGazeRay().origin, target.position);
                }
            }
        }

        _accumulatedHandOffset += deltaHandPos;

        target.position = gazeOrigin + gazeDirection * Mathf.Clamp(_isDrifting ? realTimeDist : _depthOffset, 1f, 10f) + _accumulatedHandOffset;

        //TODO: implement instant stop
        //TODO: hand is not refiement now

        if (eyeGaze.IsSaccading())
        {
            _accumulatedHandOffset = Vector3.zero;
        }


    }



}

