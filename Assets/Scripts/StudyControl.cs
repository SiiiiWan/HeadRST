
using System.Collections.Generic;
using UnityEngine;

public class StudyControl : Singleton<StudyControl>
{
    public ManipulationTechnique ManipulationBehavior;
    public ManipulatableObject GrabbedObject;

    public List<GameObject> DockingTargets;

    void Update()
    {
        if (PinchDetector.GetInstance().PinchState == PinchState.NotPinching && GrabbedObject != null)
        {
            GrabbedObject = null;
        }

    }

    public void Reset()
    {   
        foreach (GameObject bunny in DockingTargets)
        {
            bunny.SetActive(true);
        }
    }

}