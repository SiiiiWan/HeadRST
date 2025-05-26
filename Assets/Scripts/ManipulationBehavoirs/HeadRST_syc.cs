using UnityEngine;
using UnityEngine.Assertions.Comparers;

public class HeadRST_sync : ManipulationTechnique
{

    private float _headToObjectDistance;

    public override void Apply(Transform target)
    {
        EyeGaze eyeGaze = EyeGaze.GetInstance();
        HeadMovement head = HeadMovement.GetInstance();

        Vector3 gazeOrigin = eyeGaze.GetGazeRay().origin;
        Vector3 gazeDirection = eyeGaze.GetGazeRay().direction;

        Quaternion deltaRot = HandPosition.GetInstance().GetDeltaHandRotation();
        target.rotation = deltaRot * target.rotation;


        Vector3 handPosition = HandPosition.GetInstance().GetHandPosition(usePinchTip: true);
        Vector3 deltaPos = HandPosition.GetInstance().GetDeltaHandPosition(usePinchTip: true);



        if (eyeGaze.IsSaccading())
        {
            target.position = gazeOrigin + gazeDirection * Vector3.Distance(gazeOrigin, target.position);
        }


    }
    


}