using UnityEngine;

public class MagicModPitch : MagicPitch
{
    public override string TechniqueName => "MAGMODPITCH";

    public float MaxHandSpeed = 0.1f;

    public override Vector3 GetHeadDepthOffset(Vector3 objectDirection)
    {
        HeadDepthOffset_base = base.GetHeadDepthOffset(objectDirection);
        Attenuation = HeadAttenuation(HeadDepthOffset_base);
        return HeadDepthOffset_base * Attenuation;
    }

    private float HeadAttenuation(Vector3 headDepthOffset)
    {
        float attenuation = 1;

        Vector3 projectedHandMovementOnGround = MathFunctions.ProjectVectorOntoPlane(Filtered_HandMovementVector, Vector3.up);
        Vector3 projectedHeadDepthOffsetOnGround = MathFunctions.ProjectVectorOntoPlane(headDepthOffset, Vector3.up);

        if (Vector3.Dot(projectedHeadDepthOffsetOnGround, projectedHandMovementOnGround) < 0 || Vector3.Dot(headDepthOffset, Filtered_HandMovementVector) < 0)
        {
            float maxSpd = MaxHandSpeed;
            float projectedSpeed = HandTranslationSpeed;
            float sqrtPart = Mathf.Sqrt(projectedSpeed / maxSpd);
            attenuation = -sqrtPart + 1f;
        }

        return Mathf.Clamp(attenuation, 0, 1);
    }
}
