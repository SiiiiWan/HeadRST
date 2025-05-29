using UnityEngine;

public class ImplicitGaze : ManipulationTechnique
{
    private float _safeRegionRadius = 6f;
    private float _resetTimeStamp;


    public override void ApplyGrabbedBehaviour(Transform target)
    {
        Vector3 gazeOrigin = EyeGaze.GetInstance().GetGazeRay().origin;
        Vector3 gazeDirection = EyeGaze.GetInstance().GetGazeRay().direction;

        Quaternion deltaRot = HandPosition.GetInstance().GetDeltaHandRotation(usePinchTip: true);
        target.rotation = deltaRot * target.rotation;


        if (IsGazeInSafeRegion(gazeOrigin, gazeDirection, target.position))
        {
            Vector3 deltaPos = HandPosition.GetInstance().GetDeltaHandPosition(usePinchTip: true);
            target.position += deltaPos;
        }
        else
        {
            ResetSafeRegion();

            float distance = Vector3.Distance(gazeOrigin, target.position);
            target.position = gazeOrigin + gazeDirection * distance;
        }
    }

    bool IsGazeInSafeRegion(Vector3 gazeOrigin, Vector3 gazeDirection, Vector3 targetPosition)
    {

        _safeRegionRadius = _safeRegionRadius + 10 * (Time.time - _resetTimeStamp);
        if (_safeRegionRadius > 20f) _safeRegionRadius = 20f;

        float angularDistance = Vector3.Angle(gazeDirection, targetPosition - gazeOrigin);
        return angularDistance <= _safeRegionRadius;
    }
    
    void ResetSafeRegion()
    {
        _safeRegionRadius = 6f;
        _resetTimeStamp = Time.time;
    }
}