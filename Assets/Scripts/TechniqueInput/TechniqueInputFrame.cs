using UnityEngine;

public class TechniqueInputFrame
{
    public Handedness DominantHand { get; set; }

    public Vector3 PinchPosition { get; set; }
    public Vector3 PinchPositionDelta { get; set; }
    public Quaternion PinchRotationDelta { get; set; }
    public Vector3 WristPosition { get; set; }
    public Vector3 WristPositionDelta { get; set; }
    public float HandTranslationSpeed { get; set; }
    public float HandRotationSpeed { get; set; }
    public bool IsHandStabilized { get; set; }
    public Vector3 FilteredHandMovementVector { get; set; }

    public PinchState PinchState { get; set; }
    public bool IsOneHandPinching { get; set; }
    public bool IsNoHandPinchingLastFrame { get; set; }

    public Ray GazeRay { get; set; }
    public Vector3 GazeOrigin => GazeRay.origin;
    public Vector3 GazeDirection => GazeRay.direction.normalized;
    public bool IsGazeFixating { get; set; }
    public bool WasGazeFixating { get; set; }
    public Vector3 GazeFixationCentroid { get; set; }
    public bool IsGazeSaccading { get; set; }
    public Vector3 GazeDirectionOnGazeFixation { get; set; }
    public Vector3 HeadDirectionOnGazeFixation { get; set; }
    public float EyeInHeadYAngle { get; set; }
    public float EyeInHeadXAngle { get; set; }
    public float FilteredEyeInHeadAngle { get; set; }
    public float FilteredEyeInHeadAnglePrevious { get; set; }
    public float EyeInHeadYAngleOnGazeFixation { get; set; }

    public Vector3 HeadForward { get; set; }
    public Vector3 HeadRight { get; set; }
    public Vector3 HeadPosition { get; set; }
    public bool IsHeadFixating { get; set; }
    public bool WasHeadFixating { get; set; }
    public Vector3 HeadFixationCentroid { get; set; }
    public float HeadSpeed { get; set; }
    public float HeadYAngle { get; set; }
    public float DeltaHeadY { get; set; }
    public float LimitHeadYUp { get; set; }
    public float LimitHeadYDown { get; set; }
    public float HeadYAngleOnGazeFixation { get; set; }
}
