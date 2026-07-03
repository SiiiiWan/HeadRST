using UnityEngine;

public class MagicPitch : Magic
{
    public override string TechniqueName => "MAGICPITCH";

    protected override void ApplyHeadStateBehaviour()
    {
        Vector3 objectDirection = (GrabbedObject.transform.position - GazeOrigin).normalized;
        HeadDepthOffset = GetHeadDepthOffset(objectDirection);
        GrabbedObject.transform.position += HeadDepthOffset;

        DistanceToGazeAfterAddingHeadDepth = Vector3.Distance(GrabbedObject.transform.position, GazeOrigin);
        GrabbedObject.transform.position = GazeOrigin + objectDirection * Mathf.Clamp(DistanceToGazeAfterAddingHeadDepth, MinDepth, MaxDepth);

        AngleGazeDirectionToObject = Vector3.Angle(GazeDirection, GrabbedObject.transform.position - GazeOrigin);
        if (IsGazeFixating == false && AngleGazeDirectionToObject > theta_thr) CurrentState = StaticState.Gaze;
    }

    public virtual Vector3 GetHeadDepthOffset(Vector3 objectDirection)
    {
        BaseGain = VitLerp(Mathf.Abs(HeadSpeed), G_min, G_max, v_min, v_max);
        EdgeGain = GetFScale();
        PitchGain = EdgeGain * BaseGain;
        return objectDirection * DeltaHeadY * PitchGain;
    }

    public float GetFScale()
    {
        float eyeRange = GetEyeRange(EyeInHeadXAngle, EyeInHeadYAngle);
        float k = 3;
        float boostStartDeg = eyeRange / k;

        float scaleFactor = 1f;
        float gazeAngleFromHead = Vector3.Angle(GazeDirection, HeadForward);

        if (gazeAngleFromHead >= boostStartDeg & Filtered_EyeInHeadAngle > Filtered_EyeInHeadAngle_Pre)
        {
            scaleFactor = LinearDepthFunctionTwoPoints(gazeAngleFromHead, new Vector2(boostStartDeg, 1), new Vector2(eyeRange, k));
        }

        return scaleFactor;
    }

    private float GetEyeRange(float x, float y, float upLim = 15, float downLim = 30, float sideLim = 30)
    {
        if (y >= 0) return (1 - (1 - (upLim / sideLim)) * Mathf.Sin(Mathf.Atan2(y, x))) * sideLim;
        return (1 + (1 - (downLim / sideLim)) * Mathf.Sin(Mathf.Atan2(y, x))) * sideLim;
    }

    protected float LinearDepthFunctionTwoPoints(float x, Vector2 left, Vector2 right)
    {
        float k = (right.y - left.y) / (right.x - left.x);
        float b = right.y - k * right.x;
        return k * x + b;
    }
}
