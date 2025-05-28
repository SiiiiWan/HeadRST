using UnityEngine;

public class GazenPinch : ManipulationTechnique
{
    private Vector3 _objectDirectionFromHand;
    public float HandSpeedTHreshold = 0.2f;
    private float _headDepthOffset;

    Quaternion _rotationOffset;
    private Linescript _handRayLine;

    private Vector3 _localOffset;
    private Quaternion _localRotationOffset;
    private Transform _handTransform;

    public override void OnGrabbed(Transform target)
    {
        // Use pinch tip or hand anchor as the "hand"
        _handTransform = HandPosition.GetInstance().GetHandTransform(usePinchTip: true);

        // Calculate local position and rotation offset as if target is a child of hand
        _localOffset = _handTransform.InverseTransformPoint(target.position);
        _localRotationOffset = Quaternion.Inverse(_handTransform.rotation) * target.rotation;
    }

    public override void Apply(Transform target)
    {
        HandPosition hand = HandPosition.GetInstance();

        if (HeadMovement.GetInstance().HeadSpeed <= 0.2f)
        {
            // target.position = target.position + HandPosition.GetInstance().GetDeltaHandPosition(usePinchTip: true) * Mathf.Clamp(Vector3.Distance(target.position, Camera.main.transform.position), 1f, 1000f);
            target.position = target.position + hand.GetDeltaHandPosition(usePinchTip: true);
            target.rotation = hand.GetDeltaHandRotation(usePinchTip: true) * target.rotation;

            _handRayLine.IsVisible = false;
        }
        else
        {
            target.position = _handTransform.TransformPoint(_localOffset);
            target.rotation = _handTransform.rotation * _localRotationOffset;


            // _headDepthOffset += HeadMovement.GetInstance().DeltaHeadY * 0.2f;

            // //TODO: can add a eye head angle exiding just warp to the depth limits
            // _headDepthOffset = Mathf.Clamp(_headDepthOffset, 0f, 10f);
            // target.position = hand.GetHandPosition(usePinchTip: true) + _objectDirectionFromHand * _headDepthOffset;

            _handRayLine.IsVisible = true;
        }

        _handRayLine.SetPostion(hand.GetHandPosition(usePinchTip: true), target.position);

    }

    void Awake()
    {
        _handRayLine = new Linescript();
    }

}