using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public enum Hand
{
    Left,
    Right
}

public class HandData : Singleton<HandData>
{

    [Tooltip("The time window (in seconds) over which to average hand speed for stability checks.")]
    [SerializeField] private float _movementTimeWindow = 0.3f; // 300ms
    private readonly List<(float time, float speed)> _handSpeedHistory = new List<(float, float)>();

    public Transform RightHandAnchor, LeftHandAnchor;
    public OVRHand RightOVRHand, LeftOVRHand;

    public Vector3 RightHandPosition, LeftHandPosition;
    public Vector3 RightPinchTipPosition, LeftPinchTipPosition;
    public Quaternion RightHandRotation, LeftHandRotation;
    public Quaternion RightPinchTipRotation, LeftPinchTipRotation;

    public Vector3 RightHandPosition_delta, LeftHandPosition_delta;
    public Vector3 RightPinchTipPosition_delta, LeftPinchTipPosition_delta;
    public Quaternion RightHandRotation_delta, LeftHandRotation_delta;
    public Quaternion RightPinchTipRotation_delta, LeftPinchTipRotation_delta;

    public Vector3 RightPalmPosition, LeftPalmPosition;

    public Vector3 RightHandDirection, LeftHandDirection;
    public Quaternion RightHandDirection_delta, LeftHandDirection_delta;
    public float RightHandSpeed_wrist, LeftHandSpeed_wrist;
    public float RightHandSpeed_pinch, LeftHandSpeed_pinch;
    public float HandDistance, HandDistance_delta;
    public Vector3 HandMidPosition, HandMidPosition_delta;

    void Update()
    {
        if(RightOVRHand.IsDataValid == false || LeftOVRHand.IsDataValid == false)
        {
            RightHandPosition_delta = Vector3.zero;
            LeftHandPosition_delta = Vector3.zero;

            RightHandRotation_delta = Quaternion.identity;
            LeftHandRotation_delta = Quaternion.identity;

            RightHandDirection_delta = Quaternion.identity;
            LeftHandDirection_delta = Quaternion.identity;

            HandDistance_delta = 0f;
            HandMidPosition_delta = Vector3.zero;

            RightPinchTipPosition_delta = Vector3.zero;
            LeftPinchTipPosition_delta = Vector3.zero;

            RightPinchTipRotation_delta = Quaternion.identity;
            LeftPinchTipRotation_delta = Quaternion.identity;

            return; // Skip update if hand data is not valid
        }

        RightHandPosition_delta = RightHandAnchor.position - RightHandPosition;
        LeftHandPosition_delta = LeftHandAnchor.position - LeftHandPosition;

        RightHandRotation_delta = RightHandAnchor.rotation * Quaternion.Inverse(RightHandRotation);
        LeftHandRotation_delta = LeftHandAnchor.rotation * Quaternion.Inverse(LeftHandRotation);

        RightHandDirection_delta = Quaternion.FromToRotation(RightHandDirection, RightHandAnchor.forward);
        LeftHandDirection_delta = Quaternion.FromToRotation(LeftHandDirection, LeftHandAnchor.forward);

        RightHandPosition = RightHandAnchor.position;
        LeftHandPosition = LeftHandAnchor.position;

        HandDistance_delta = Vector3.Distance(RightHandPosition, LeftHandPosition) - HandDistance;
        HandDistance = Vector3.Distance(RightHandPosition, LeftHandPosition);

        HandMidPosition_delta = (RightHandPosition + LeftHandPosition) / 2 - HandMidPosition;
        HandMidPosition = (RightHandPosition + LeftHandPosition) / 2;

        Transform rightTip = GetPinchTipTransform(PinchDetector.GetInstance().RightHand);
        if (rightTip)
        {
            RightPinchTipPosition_delta = rightTip.position - RightPinchTipPosition;
            RightPinchTipRotation_delta = rightTip.rotation * Quaternion.Inverse(RightPinchTipRotation);
            RightPinchTipPosition = rightTip.position;
            RightPinchTipRotation = rightTip.rotation;
            RightHandSpeed_pinch = RightPinchTipPosition_delta.magnitude / Time.deltaTime;
        }


        Transform leftTip = GetPinchTipTransform(PinchDetector.GetInstance().LeftHand);
        if (leftTip)
        {
            LeftPinchTipPosition_delta = leftTip.position - LeftPinchTipPosition;
            LeftPinchTipRotation_delta = leftTip.rotation * Quaternion.Inverse(LeftPinchTipRotation);
            LeftPinchTipPosition = leftTip.position;
            LeftPinchTipRotation = leftTip.rotation;
            LeftHandSpeed_pinch = LeftPinchTipPosition_delta.magnitude / Time.deltaTime;
        }

        Transform rightPalm = GetPalmTransform(PinchDetector.GetInstance().RightHand);
        if (rightPalm)
        {
            RightPalmPosition = rightPalm.position;
        }

        Transform leftPalm = GetPalmTransform(PinchDetector.GetInstance().LeftHand);
        if (leftPalm)
        {
            LeftPalmPosition = leftPalm.position;
        }

        RightHandRotation = RightHandAnchor.rotation;
        LeftHandRotation = LeftHandAnchor.rotation;

        RightHandSpeed_wrist = RightHandPosition_delta.magnitude / Time.deltaTime;
        LeftHandSpeed_wrist = LeftHandPosition_delta.magnitude / Time.deltaTime;

        RightHandDirection = RightHandAnchor.forward;
        LeftHandDirection = LeftHandAnchor.forward;

        // Update speed history
        _handSpeedHistory.Add((Time.time, GetHandSpeed()));
        _handSpeedHistory.RemoveAll(entry => Time.time - entry.time > _movementTimeWindow);
    }

    private Transform GetPinchTipTransform(OVRHand hand)
    {
        if (hand == null) return null;
        var skeleton = hand.GetComponent<OVRSkeleton>();
        if (skeleton == null || skeleton.Bones == null) return null;

        foreach (var bone in skeleton.Bones)
        {
            if (bone.Id == OVRSkeleton.BoneId.XRHand_ThumbTip) // Use Hand_IndexTip for OVR //XRHand_IndexTip
                return bone.Transform;
        }
        return null;
    }

    private Transform GetPalmTransform(OVRHand hand)
    {
        if (hand == null) return null;
        var skeleton = hand.GetComponent<OVRSkeleton>();
        if (skeleton == null || skeleton.Bones == null) return null;

        foreach (var bone in skeleton.Bones)
        {
            if (bone.Id == OVRSkeleton.BoneId.XRHand_Palm) // Use Hand_WristRoot for OVR //XRHand_Palm
                return bone.Transform;
        }
        return null;
    }


    public Vector3 GetHandPosition(bool usePinchTip = true)
    {
        if (Settings.GetInstance().DominantHand == Handedness_v.Left)
        {
            return usePinchTip ? LeftPinchTipPosition : LeftHandPosition;
        }
        else
        {
            return usePinchTip ? RightPinchTipPosition : RightHandPosition;
        }
    }

    public Vector3 GetDeltaHandPosition(bool usePinchTip = true)
    {
        if (Settings.GetInstance().DominantHand == Handedness_v.Left)
        {
            return usePinchTip ? LeftPinchTipPosition_delta : LeftHandPosition_delta;
        }
        else
        {
            return usePinchTip ? RightPinchTipPosition_delta : RightHandPosition_delta;
        }
    }

    public Quaternion GetHandRotation(bool usePinchTip)
    {
        if (Settings.GetInstance().DominantHand == Handedness_v.Left)
        {
            return usePinchTip ? LeftPinchTipRotation : LeftHandRotation;
        }
        else
        {
            return usePinchTip ? RightPinchTipRotation : RightHandRotation;
        }
    }

    public Quaternion GetDeltaHandRotation(bool usePinchTip = true)
    {
        if (Settings.GetInstance().DominantHand == Handedness_v.Left)
        {
            return usePinchTip ? LeftPinchTipRotation_delta : LeftHandRotation_delta;
        }
        else
        {
            return usePinchTip ? RightPinchTipRotation_delta : RightHandRotation_delta;
        }
    }

    public float GetHandRotationSpeed(bool usePinchTip = true)
    {
        GetDeltaHandRotation(usePinchTip).ToAngleAxis(out float rotationAngle, out Vector3 axis);
        return rotationAngle / Time.deltaTime;
    }

    public float GetHandSpeed(bool usePinchTip = true)
    {
        if (Settings.GetInstance().DominantHand == Handedness_v.Left)
        {
            return usePinchTip ? LeftHandSpeed_pinch : LeftHandSpeed_wrist;
        }
        else
        {
            return usePinchTip ? RightHandSpeed_pinch : RightHandSpeed_wrist;
        }
    }

    public bool IsHandActivelyMoving(float movementSpeedThreshold = 0.05f)
    {
        if (_handSpeedHistory.Count == 0)
        {
            return false;
        }

        // To be robust, ensure the history buffer is reasonably full
        float historyTimeSpan = _handSpeedHistory.Last().time - _handSpeedHistory.First().time;
        if (historyTimeSpan < _movementTimeWindow * 0.8f)
        {
            return false; // Not enough data for a reliable average yet
        }

        float averageSpeed = _handSpeedHistory.Average(entry => entry.speed);
        return averageSpeed > movementSpeedThreshold;
    }

    public Vector3 GetHandDirection()
    {
        return Settings.GetInstance().DominantHand == Handedness_v.Left ? LeftHandDirection : RightHandDirection;
    }

    public Quaternion GetHandDirectionDelta()
    {
        return Settings.GetInstance().DominantHand == Handedness_v.Left ? LeftHandDirection_delta : RightHandDirection_delta;
    }

    public Transform GetHandTransform(bool usePinchTip)
    {
        if (Settings.GetInstance().DominantHand == Handedness_v.Left)
        {
            return usePinchTip ? GetPinchTipTransform(PinchDetector.GetInstance().LeftHand) : LeftHandAnchor;
        }
        else
        {
            return usePinchTip ? GetPinchTipTransform(PinchDetector.GetInstance().RightHand) : RightHandAnchor;
        }
    }


}
