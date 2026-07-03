using UnityEngine;
using UnityEngine.Serialization;

public class MagicModPitch : MagicPitch
{
    public override string TechniqueName => "MAGMODPITCH";

    [FormerlySerializedAs("MaxHandSpeed")]
    public float v_hmax = 0.1f;

    public Vector3 HeadDepthOffset_base { get; protected set; }
    public float Attenuation { get; protected set; } = 1;

    public override Vector3 GetHeadDepthOffset(Vector3 objectDirection)
    {
        HeadDepthOffset_base = base.GetHeadDepthOffset(objectDirection);
        Attenuation = GetAttenuation(HeadDepthOffset_base);
        return (1f - Attenuation) * HeadDepthOffset_base;
    }

    private float GetAttenuation(Vector3 headDepthOffset)
    {
        float attenuation = 0f;

        Vector3 projectedHandMovementOnGround = MathFunctions.ProjectVectorOntoPlane(Filtered_HandMovementVector, Vector3.up);
        Vector3 projectedHeadDepthOffsetOnGround = MathFunctions.ProjectVectorOntoPlane(headDepthOffset, Vector3.up);

        if (Vector3.Dot(headDepthOffset, Filtered_HandMovementVector) < 0 || Vector3.Dot(projectedHeadDepthOffsetOnGround, projectedHandMovementOnGround) < 0)
        {
            attenuation = Mathf.Min(Mathf.Sqrt(HandTranslationSpeed / v_hmax), 1f);
        }

        return Mathf.Clamp(attenuation, 0, 1);
    }
}
