using UnityEngine;

public class GazeNPinchOrigin : ManipulationTechnique
{

    // float GetVisualGain_Plus(Transform target)
    // {
    //     EyeGaze gazeData = EyeGaze.GetInstance();
    //     HandData handData = global::HandData.GetInstance();

    //     Vector3 gazeOrigin = gazeData.GetGazeRay().origin;
    //     Vector3 gazeDirection = gazeData.GetGazeRay().direction;

    //     Vector3 gazeToTargetVector = target.position - gazeOrigin;
    //     float targetDistance = gazeToTargetVector.magnitude;

    //     Vector3 gazeToHandVector = handData.GetHandPosition(usePinchTip: true) - gazeOrigin;
    //     Vector3 gazeToHandVectorProjected = Vector3.Project(gazeToHandVector, gazeDirection);
    //     float handDistance = gazeToHandVectorProjected.magnitude;

    //     return targetDistance / handDistance;
    // }


}