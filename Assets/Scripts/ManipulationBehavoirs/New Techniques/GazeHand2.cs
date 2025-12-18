using UnityEngine;

public class GazeHand2 : PositionRotationProvider
{
    Vector3 _gazeDirection;
    float _depth;
    public float _torsoOffset = 0.35f;

    // public override Vector3 GetPositionOutput(Vector3 currentPosition)
    // {
    //     UpdateDataSource();

    //     Vector3 handPosition = HandData.GetDeltaHandPosition(usePinchTip: false);
    //     _gazeDirection = Vector3.Lerp(_gazeDirection, GazeData.GetRawGazeDirection(), 0.1f).normalized;


    //     Vector3 handOffsetFromGrabInit = handPosition - _handInitPosition;
    //     float handOffsetDistanceOnDepthAxis = Vector3.Project(handOffsetFromGrabInit, _gazeDirection).magnitude;

    //     if (handOffsetDistanceOnDepthAxis >= 0.05f) _depth += (handOffsetDistanceOnDepthAxis - 0.05f) * 100f * 0.144f * 100f * (Vector3.Dot(handOffsetFromGrabInit, _gazeDirection) > 0 ? 1 : -1) * Time.deltaTime / 100f;

    //     // _depth = Mathf.Clamp(_depth, 0.05f, 15f); // Do they have this?

    //     Vector3 manubriumPoint = HeadData.HeadPosition + Vector3.down * _torsoOffset;
    //     Vector3 manubriumToHandOffset = handPosition - manubriumPoint;

    //     currentPosition = GazeData.GetRawGazeOrigin() + _gazeDirection * _depth + manubriumToHandOffset;

    //     VirtualHandPosition = currentPosition + VirtualHandOffsetFromObject;
        
    // }

}