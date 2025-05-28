using UnityEngine;

public class GazenPinch : ManipulationTechnique
{
    private Vector3 _objectDirectionFromHand;
    public float HandSpeedTHreshold = 0.2f;
    private float _headDepthOffset;

    public GameObject test;

    private Linescript _handRayLine;

    public override void OnGrabbed(Transform target)
    {
        Vector3 objectOffset = target.position - HandPosition.GetInstance().GetHandPosition(usePinchTip: true);
        _objectDirectionFromHand = objectOffset.normalized;
        _headDepthOffset = objectOffset.magnitude;
    }

    public override void Apply(Transform target)
    {
        HandPosition hand = HandPosition.GetInstance();

        Quaternion deltaRot = hand.GetDeltaHandRotation(usePinchTip: true);
        // target.rotation = deltaRot * target.rotation;



        if (HeadMovement.GetInstance().HeadSpeed <= 0.2f)
        {
            // target.position = target.position + HandPosition.GetInstance().GetDeltaHandPosition(usePinchTip: true) * Mathf.Clamp(Vector3.Distance(target.position, Camera.main.transform.position), 1f, 1000f);
            target.position = target.position + hand.GetDeltaHandPosition(usePinchTip: true);

            _handRayLine.IsVisible = false;
        }
        else
        {
            _objectDirectionFromHand = hand.GetHandDirectionDelta() * _objectDirectionFromHand;

            // _headDepthOffset += HeadMovement.GetInstance().DeltaHeadY * 0.2f;

            // //TODO: can add a eye head angle exiding just warp to the depth limits
            // _headDepthOffset = Mathf.Clamp(_headDepthOffset, 0f, 10f);
            target.position = hand.GetHandPosition(usePinchTip: true) + _objectDirectionFromHand * _headDepthOffset;

            _handRayLine.IsVisible = true;
        }

        test.transform.position = target.position;

        _handRayLine.SetPostion(hand.GetHandPosition(usePinchTip: true), target.position);

    }

    void Awake()
    {
        _handRayLine = new Linescript();
    }

}