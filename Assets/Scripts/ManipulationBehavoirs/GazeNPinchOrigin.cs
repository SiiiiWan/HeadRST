using UnityEngine;

public class GazeNPinchOrigin : ManipulationTechnique
{
    public bool useVisualGain_Plus = true;

    public override void ApplySingleHandGrabbedBehaviour(Transform target)
    {
        Vector3 deltaPos = HandPosition.GetInstance().GetDeltaHandPosition(usePinchTip: true);
        target.position += deltaPos * (useVisualGain_Plus ? GetVisualGain_Plus(target) : GetVisualGain(target));

        Quaternion deltaRot = HandPosition.GetInstance().GetDeltaHandRotation(usePinchTip: true);
        target.rotation = deltaRot * target.rotation;
    }

    float GetOriginGain(Transform target)
    {
        return Vector3.Distance(target.position, Camera.main.transform.position);
    }

    float GetVisualGain(Transform target)
    {
        // return Vector3.Distance(target.position, HandPosition.GetInstance().GetHandPosition(usePinchTip: false)) / Vector3.Distance(HandPosition.GetInstance().GetHandPosition(usePinchTip: true), HandPosition.GetInstance().GetHandPosition(usePinchTip: false));
        // return Vector3.Distance(target.position,HandPosition.GetInstance().LeftHandPosition) / Vector3.Distance(HandPosition.GetInstance().GetHandPosition(usePinchTip: true), HandPosition.GetInstance().LeftHandPosition);

        return Vector3.Distance(target.position, Camera.main.transform.position) / Vector3.Distance(HandPosition.GetInstance().GetHandPosition(usePinchTip: true), Camera.main.transform.position);
    }

    float GetVisualGain_Plus(Transform target)
    {
        EyeGaze gazeData = EyeGaze.GetInstance();
        HandPosition handData = HandPosition.GetInstance();

        Vector3 gazeOrigin = gazeData.GetGazeRay().origin;
        Vector3 gazeDirection = gazeData.GetGazeRay().direction;

        Vector3 gazeToTargetVector = target.position - gazeOrigin;
        float targetDistance = gazeToTargetVector.magnitude;

        Vector3 gazeToHandVector = handData.GetHandPosition(usePinchTip: true) - gazeOrigin;
        Vector3 gazeToHandVectorProjected = Vector3.Project(gazeToHandVector, gazeDirection);
        float handDistance = gazeToHandVectorProjected.magnitude;

        return targetDistance / handDistance;
    }

    float GetAdoptiveGain(Transform target)
    {
        float d = Vector3.Distance(target.position, Camera.main.transform.position);
        return Mathf.Clamp(0.016f * d * d * d - 0.488f * d * d + 5.254f * d + 0.875f, .044f, 24.36f);
    }
}