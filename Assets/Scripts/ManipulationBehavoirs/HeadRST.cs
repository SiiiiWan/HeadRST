using UnityEngine;

public class HeadRST : ManipulationTechnique
{

    public float minHeadSpd = 0.1f;
    public float maxHeadSpd = 0.3f;

    public float minGainDeg = 30;
    public float maxGainDeg = 10;

    private float _depthOffset;
    private Vector3 _accumulatedHandOffset;


    public override void OnGrabbed(Transform target)
    {
        _depthOffset = Vector3.Distance(EyeGaze.GetInstance().GetGazeRay().origin, target.position);
    }

    public override void Apply(Transform target)
    {
        EyeGaze eyeGaze = EyeGaze.GetInstance();
        HeadMovement head = HeadMovement.GetInstance();

        Vector3 gazeOrigin = eyeGaze.GetGazeRay().origin;
        Vector3 gazeDirection = eyeGaze.GetGazeRay().direction;

        Vector3 deltaHandPos = HandPosition.GetInstance().GetDeltaHandPosition(usePinchTip: true);

        Quaternion deltaRot = HandPosition.GetInstance().GetDeltaHandRotation();
        target.rotation = deltaRot * target.rotation;

        if (head.HeadSpeed >= 0.1f)
        {
            float offset;

            float max_gain = 0.4f;
            float min_gain = 0.4f / 3f;

            offset = head.DeltaHeadY * VitLerp(head.HeadSpeed, min_gain, max_gain, minHeadSpd, maxHeadSpd);

            _depthOffset += offset;

            target.position = gazeOrigin + gazeDirection * Mathf.Clamp(_depthOffset, 1f, 10f) + _accumulatedHandOffset;

        }

        _accumulatedHandOffset += deltaHandPos;

        if (eyeGaze.IsSaccading())
        {
            _accumulatedHandOffset = Vector3.zero;
        }

        target.position += deltaHandPos;

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