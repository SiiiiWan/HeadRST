using UnityEngine;

public class GazenPinch : ManipulationTechnique
{
    private Vector3 _objectDirectionFromHand;
    public float HandSpeedTHreshold = 0.2f;
    private float _headDepthOffset;

    Quaternion _rotationOffset;
    private Linescript _handRayLine;

    private Vector3 _localOffsetDir;
    private float _localOffsetDistance;
    private Quaternion _localRotationOffset;
    private Transform _handTransform;

    public override void OnGrabbed(Transform target)
    {
        // Use pinch tip or hand anchor as the "hand"
        _handTransform = HandPosition.GetInstance().GetHandTransform(usePinchTip: true);

        Vector3 localOffset = _handTransform.InverseTransformPoint(target.position);
        _localOffsetDir = localOffset.normalized;
        _localOffsetDistance = localOffset.magnitude;
        _localRotationOffset = Quaternion.Inverse(_handTransform.rotation) * target.rotation;
    }

    public override void ApplyGrabbedBehaviour(Transform target)
    {
        HandPosition hand = HandPosition.GetInstance();

        if (HeadMovement.GetInstance().HeadSpeed <= 0.2f)
        {
            // target.position = target.position + HandPosition.GetInstance().GetDeltaHandPosition(usePinchTip: true) * Mathf.Clamp(Vector3.Distance(target.position, Camera.main.transform.position), 1f, 1000f);
            target.position = target.position + hand.GetDeltaHandPosition(usePinchTip: true);
            target.rotation = hand.GetDeltaHandRotation(usePinchTip: true) * target.rotation;

            Vector3 localOffset = _handTransform.InverseTransformPoint(target.position);
            _localOffsetDir = localOffset.normalized;
            _localOffsetDistance = localOffset.magnitude;
            _localRotationOffset = Quaternion.Inverse(_handTransform.rotation) * target.rotation;

            _handRayLine.IsVisible = false;
        }
        else
        {

            // _localOffsetDistance += HeadMovement.GetInstance().DeltaHeadY * 0.2f;

            _localOffsetDistance = Mathf.Clamp(_localOffsetDistance, 0f, 5f);

            if (_handTransform)
            {
                target.position = _handTransform.TransformPoint(_localOffsetDir * _localOffsetDistance);
                target.rotation = _handTransform.rotation * _localRotationOffset;
            }


            // //TODO: can add a eye head angle exiding just warp to the depth limits

            _handRayLine.IsVisible = true;
        }

        if (hand.GetHandSpeed() <= 1f && HeadMovement.GetInstance().HeadSpeed > 0.2f)
        {
            _localOffsetDistance += HeadMovement.GetInstance().DeltaHeadY * 0.3f;
            _localOffsetDistance = Mathf.Clamp(_localOffsetDistance, 0f, 5f);

            if (_handTransform)
            {
                target.position = _handTransform.TransformPoint(_localOffsetDir * _localOffsetDistance);
                target.rotation = _handTransform.rotation * _localRotationOffset;
            }

            _handRayLine.IsVisible = true;
        }

        _handRayLine.SetPostion(hand.GetHandPosition(usePinchTip: true), target.position);

    }

    void Awake()
    {
        _handRayLine = new Linescript();
    }

}