
using UnityEngine;

public class StudyControl : Singleton<StudyControl>
{
    public ManipulationTechnique ManipulationBehavior;
    public ManipulatableObject GrabbedObject;

    void Update()
    {
        if (PinchDetector.GetInstance().PinchState == PinchState.NotPinching && GrabbedObject != null)
        {
            GrabbedObject = null;
        }
        
    }

}