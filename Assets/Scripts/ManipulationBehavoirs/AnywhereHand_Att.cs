
using System;
using UnityEngine;

public class AnywhereHand_Att : AnywhereHand
{
    public float Attenuation { get; protected set; } = 1;
    public float MaxHandSpeed = 0.1f;
    public override Vector3 GetHeadDepthOffset(Vector3 objectDirection)
    {

        float max_gain = (MaxDepth - MinDepth) / MaxGainDeg;
        float min_gain = (MaxDepth - MinDepth) / MinGainDeg;

        min_gain = min_gain / (1 + (Mathf.Pow(2, 4) - 1) * HandTranslationSpeed / MaxHandSpeed);

        BaseGain = VitLerp(Math.Abs(HeadSpeed), min_gain, max_gain, MinHeadSpeed, MaxHeadSpeed);
        EdgeGain = EyeHeadGain();
        Vector3 headDepthOffset = objectDirection * DeltaHeadY * BaseGain;

        return headDepthOffset * HeadAttenuation(headDepthOffset);

    }

        

    float HeadAttenuation(Vector3 headDepthOffset)
    {
        float attenuation = 1;

        Vector3 projectedHandMovementOnGround = MathFunctions.ProjectVectorOntoPlane(Filtered_HandMovementVector, Vector3.up);
        Vector3 projectedHeadDepthOffsetOnGround = MathFunctions.ProjectVectorOntoPlane(headDepthOffset, Vector3.up);

        // if (Filtered_EyeInHeadAngle < Filtered_EyeInHeadAngle_Pre && Vector3.Dot(headDepthOffset, Filtered_HandMovementVector) < 0 && Filtered_EyeInHeadAngle > EyeInHeadYAngle_OnGazeFixation) // eye in head angle is decreasing
        if (Vector3.Dot(projectedHeadDepthOffsetOnGround, projectedHandMovementOnGround) < 0 || Vector3.Dot(headDepthOffset, Filtered_HandMovementVector) < 0)
        {
            float maxSpd = MaxHandSpeed;

            // Vector3 gazeOriginToHand = PinchPosition - GazeOrigin;
            // float handToGazeOriginDistance = (Vector3.Dot(GazeDirection, gazeOriginToHand) > 0) ? Vector3.Project(gazeOriginToHand, headDepthOffset).magnitude : 0;
            // float minRatio = 0.1f;
            // if (handToGazeOriginDistance < 0.3f)
            // {
            //     float k = Mathf.Clamp(minRatio + (1 - minRatio) / 0.3f * handToGazeOriginDistance, minRatio, 1);
            //     maxSpd = maxSpd * k;
            // }

            // float projectedSpeed = Vector3.Project(Filtered_HandMovementVector, headDepthOffset).magnitude / Time.deltaTime;
            float projectedSpeed = HandTranslationSpeed;

            // attenuation = 1 - projectedSpeed / maxSpd;

            float sqrtPart = Mathf.Sqrt(projectedSpeed / maxSpd);
            float distance = MathFunctions.ProjectVectorOntoPlane(GrabbedObject.transform.position - GazeOrigin, Vector3.up).magnitude;
            // float exponent = 2f * distance / MaxDepth;
            float exponent = 2f * distance * distance;


            attenuation = -Mathf.Pow(sqrtPart, exponent) + 1f;

            // attenuation = 1 / (1 + Mathf.Pow((float)Math.E, 10 * (distance + 10) * (projectedSpeed - maxSpd/2)));

            // attenuation = 1 - Mathf.Sqrt(projectedSpeed / maxSpd);
        }

        return Mathf.Clamp(attenuation, 0, 1);
    }
}