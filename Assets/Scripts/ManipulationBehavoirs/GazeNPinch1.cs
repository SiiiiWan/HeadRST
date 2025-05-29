using UnityEngine;

public class GazeNPinch1 : ManipulationTechnique
{
    private Linescript _handRayLine;
    private Transform _handTransform;

    private Vector3 _localOffsetDir;
    private float _localOffsetDistance;
    private Quaternion _localRotationOffset;

    public override void OnGrabbed(Transform target)
    {
        // Use pinch tip or hand anchor as the "hand"
        _handTransform = HandPosition.GetInstance().GetHandTransform(usePinchTip: false);

        UpdateTargetOffset(target);
    }

    public override void Apply(Transform target)
    {
        HandPosition hand = HandPosition.GetInstance();

        Vector3 deltaPos = hand.GetDeltaHandPosition(usePinchTip: true);
        target.position += deltaPos;
        

        if (HeadMovement.GetInstance().HeadSpeed >= 0.2f && hand.GetHandSpeed() <= 0.8f)
        {
            _localOffsetDistance += HeadMovement.GetInstance().DeltaHeadY * 0.15f;
            _localOffsetDistance = Mathf.Clamp(_localOffsetDistance, 0.1f, 5f);

            if (_handTransform)
            {
                target.position = _handTransform.TransformPoint(_localOffsetDir * _localOffsetDistance);
                // target.rotation = _handTransform.rotation * _localRotationOffset;
            }
        }

        UpdateTargetOffset(target);
        _handRayLine.SetPostion(hand.GetHandPosition(usePinchTip: true), target.position);
    }

    void UpdateTargetOffset(Transform target)
    {
        if (!_handTransform) return;

        Vector3 localOffset = _handTransform.InverseTransformPoint(target.position);
        _localOffsetDir = localOffset.normalized;
        _localOffsetDistance = localOffset.magnitude;
        _localRotationOffset = Quaternion.Inverse(_handTransform.rotation) * target.rotation;
    }

    void Awake()
    {
        _handRayLine = new Linescript();
    }
}