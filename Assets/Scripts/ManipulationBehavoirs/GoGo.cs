using System.IO;
using UnityEngine;

public class GoGO : ManipulationTechnique
{
    float _armLength = 30; // in meters
    public Vector3 VirtualHandPosition;


    public override void Update()
    {
        base.Update();

        Vector3 torsoPosition = Camera.main.transform.position - new Vector3(0, 0.3f, 0);

        float handDistance = Vector3.Distance(torsoPosition, WristPosition) * 100;

        if (handDistance <= _armLength)
        {
            VirtualHandPosition = WristPosition;
        }
        else
        {
            print("extension");
            Vector3 direction = (WristPosition - torsoPosition).normalized;
            VirtualHandPosition = torsoPosition + direction * (handDistance +  (handDistance - _armLength) * (handDistance - _armLength) / 6f);
        }

    }

    public override Vector3 GetVirtualHandPosition()
    {

        return VirtualHandPosition;
    }
}