using UnityEngine;

public class HeadRST : ManipulationTechnique
{

    public float minHeadSpd = 0.2f;
    public float maxHeadSpd = 0.6f;

    public float minGainDeg = 30;
    public float maxGainDeg = 10;

    public override void Apply(Transform target)
    {
        EyeGaze eyeGaze = EyeGaze.GetInstance();
        HeadMovement head = HeadMovement.GetInstance();

        Vector3 gazeOrigin = eyeGaze.GetGazeRay().origin;
        Vector3 gazeDirection = eyeGaze.GetGazeRay().direction;

        Vector3 deltaPos = PinchDetector.GetInstance().IsLeftPinching ? HandPosition.GetInstance().LeftPinchTipPosition_delta : HandPosition.GetInstance().RightPinchTipPosition_delta;
        target.position += deltaPos;

        Quaternion deltaRot = PinchDetector.GetInstance().IsLeftPinching ? HandPosition.GetInstance().LeftHandRotation_delta : HandPosition.GetInstance().RightHandRotation_delta;
        target.rotation = deltaRot * target.rotation;

        float distance = Vector3.Distance(gazeOrigin, target.position);

        if (eyeGaze.IsSaccading() || head.HeadSpeed >= 0.2f)
        {
            float offset;

            float max_gain = 0.4f;
            float min_gain = 0.4f / 3f;

            offset = head.DeltaHeadY * VitLerp(head.HeadSpeed, min_gain, max_gain, minHeadSpd, maxHeadSpd);

            distance = distance + offset;
            distance = Mathf.Clamp(distance, 1f, 5f);
            target.position = gazeOrigin + gazeDirection * distance;
        }

    }
    

    float VitLerp(float x, float k1 = 0.30f, float k2 = 1.5f, float v1 = 0.05f, float v2 = 0.15f)
    {
        if(x <= v1)
            return k1;

        if(x >= v2)
            return k2;
        
        return k1 + (k2-k1) / (v2-v1) * (x-v1);
    }

}