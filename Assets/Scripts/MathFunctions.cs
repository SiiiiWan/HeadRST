using UnityEngine;

public static class MathFunctions
{
    public static float Deg2Meter(float deg, float depth)
    {
        return 2 * depth * Mathf.Tan(deg * Mathf.Deg2Rad /2);
    }

    public static float Meter2Deg(float meter, float depth)
    {
        return 2 * Mathf.Atan(meter/2/depth) * Mathf.Rad2Deg;
    }

    public static float AngleFrom_XZ_Plane(Vector3 vec)
    {
        // Project vector onto the XZ plane
        Vector3 vec_xz = new Vector3(vec.x, 0, vec.z);

        // Calculate the angle between the forward vector and its projection on the XZ plane
        float angle = Vector3.Angle(vec, vec_xz);

        // Determine if the angle is upwards or downwards by checking the sign of the y component
        if (vec.y < 0)
        {
            angle = -angle;
        }

        return angle;
    }

    public static float AngleAroundAxis(this Vector3 from, Vector3 to, Vector3 axis, bool isClockwisePositive = true)
    {
        // from: head forward
        // axis: the axis want to remove
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

    public static Vector3 ReverseAngleAroundAxis(Vector3 from, float angle, Vector3 axis, bool isClockwisePositive = true)
    {
    
        float angleRad = angle * Mathf.Deg2Rad;

        Vector3 right = isClockwisePositive
            ? Vector3.Cross(axis, from)
            : Vector3.Cross(from, axis);

        Vector3 perpendicular = Vector3.Cross(axis, right);

        Vector3 rotatedVector = Mathf.Sin(angleRad) * right + Mathf.Cos(angleRad) * perpendicular;

        return rotatedVector.normalized;
    }

    public static Vector3 FloatToVector3(float num)
    {
        return new Vector3(num, num, num);
    }


    public static float RotationSpeed (Vector3 v_pre, Vector3 v, float dt)
    {
        // var uPlusV = v + v_pre; // distance
        // var uMinusV = v - v_pre; // half of object size
        // var isa = 2 * Mathf.Atan2(Vector3.Magnitude(uMinusV), Vector3.Magnitude(uPlusV)); // visual angle equation
        // float isv = Mathf.Rad2Deg * isa / dt;
        
        // return isv;

        return Vector3.Angle(v, v_pre) / dt;
    }

    public static Vector3 IntersectionPointOnVirualPlane(Ray ray, Vector3 planeDirection, float depth)
    {
        // Calculate the normal of the plane (which is the head direction)
        Vector3 planeNormal = planeDirection.normalized;

        // Calculate the distance from the origin to the plane
        float distance = depth;

        // Calculate the intersection point
        float denominator = Vector3.Dot(ray.direction.normalized, planeNormal);
        if (denominator != 0)
        {
            float t = distance / denominator;
            return ray.origin + ray.direction.normalized * t;
        }
        else
        {
            // If the denominator is zero, the ray is parallel to the plane, so there is no intersection
            return Vector3.zero;
        }
    }

    public static bool IsRayHitObj(Ray ray, GameObject obj)
    {
        if(Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
        {
            if(hit.transform == obj.transform) return true;
        }

        return false;
    }

    public static Vector3 ProjectVec3ToVec3(Vector3 from, Vector3 to)
    {
        float dotProduct = Vector3.Dot(from, to);

        // Calculate the squared magnitude of vec2
        float magSquared = to.sqrMagnitude;

        // Calculate the projection of vec1 onto vec2
        return (dotProduct / magSquared) * to;
    }

    public static Vector3 ProjectVec3ToPlane(Vector3 from, Vector3 to)
    {
        float dotProduct = Vector3.Dot(from, to);

        // Calculate the squared magnitude of vec2
        float magSquared = to.sqrMagnitude;

        // Calculate the projection of vec1 onto vec2
        return (dotProduct / magSquared) * to;
    }


    public static Vector3 ProjectVectorOntoPlane(Vector3 vector, Vector3 planeNormal)
    {
        planeNormal = planeNormal.normalized;

        return vector - Vector3.Dot(vector, planeNormal) * planeNormal;
    }

    public static float Slop(Vector2 v1, Vector2 v2)
    {
        return (v2.y - v1.y) / (v2.x - v1.x);
    }

}
