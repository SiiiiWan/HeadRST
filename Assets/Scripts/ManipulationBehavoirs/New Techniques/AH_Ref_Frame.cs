using System;
using UnityEngine;

public class AH_Ref_Frame : PositionRotationProvider
{
    public AH_Ref_Frame_Global ReferenceFrame;

    public override Vector3 GetPositionOutput(Vector3 currentPosition)
    {
        UpdateDataSource();

        if (ReferenceFrame.IsUpdatingFrame == false)
        {
            if(ReferenceFrame.IsUpdatingFrame_LastFrame == true)
            {
                currentPosition = ReferenceFrame.transform.position;    
            }

            currentPosition += HandData.GetDeltaHandPosition() * GetVisualGain(currentPosition);
        }

        return currentPosition;
    }

    Quaternion _rotationOffset;
    public override Quaternion GetRotationOutput(Quaternion currentRotation)
    {
        UpdateDataSource();

        if (ReferenceFrame.IsUpdatingFrame == false)
        {
            if(ReferenceFrame.IsUpdatingFrame_LastFrame == true)
            {
                currentRotation = ReferenceFrame.transform.rotation * _rotationOffset;    
            }

            currentRotation = HandData.GetDeltaHandRotation() * currentRotation;
        }
        else if (ReferenceFrame.IsUpdatingFrame_LastFrame == false)
        {
            _rotationOffset = Quaternion.Inverse(ReferenceFrame.transform.rotation) * currentRotation;
        
        }

        return currentRotation;
    }
}

