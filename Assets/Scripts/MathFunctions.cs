using UnityEngine;

public static class MathFunctions
{
    public static float Deg2Meter(float deg, float depth)
    {
        return 2 * depth * Mathf.Tan(deg * Mathf.Deg2Rad / 2);
    }

    public static float AngleFrom_XZ_Plane(Vector3 vec)
    {
        Vector3 vec_xz = new Vector3(vec.x, 0, vec.z);
        float angle = Vector3.Angle(vec, vec_xz);

        if (vec.y < 0)
        {
            angle = -angle;
        }

        return angle;
    }

    public static float AngleAroundAxis(this Vector3 from, Vector3 to, Vector3 axis, bool isClockwisePositive = true)
    {
        Vector3 right;
        if (isClockwisePositive)
        {
            right = Vector3.Cross(axis, from);
            from = Vector3.Cross(right, axis);
        }
        else
        {
            right = Vector3.Cross(from, axis);
            from = Vector3.Cross(axis, right);
        }

        return Mathf.Atan2(Vector3.Dot(to, right), Vector3.Dot(to, from)) * Mathf.Rad2Deg;
    }

    public static Vector3 ProjectVectorOntoPlane(Vector3 vector, Vector3 planeNormal)
    {
        planeNormal = planeNormal.normalized;
        return vector - Vector3.Dot(vector, planeNormal) * planeNormal;
    }
}
